using System.Collections.Generic;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// The gravestone side of a downed player. The owner drops the gravestone where it fell
    /// (owner-write position); the server times the revive: a living teammate holds Interact near
    /// the gravestone for <see cref="ReviveData.HoldSeconds"/>, or a lone player's extra life runs
    /// out the same timer on its own. Then the server sets HP to the revive share.
    /// Every PC reads <see cref="Progress"/> from the shared start time to draw the ring.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class ReviveSystem : NetworkBehaviour
    {
        private static readonly List<ReviveSystem> _all = new();

        [SerializeField] private ReviveData _data;

        private readonly NetworkVariable<Vector2> _grave = new(
            Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<bool> _hasGrave = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<double> _reviveStart = new(
            ReviveRules.NotReviving, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[1];
        private Health _health;
        private Health _reviver;
        private bool _soloRevive;
        private int _livesUsed;

        /// <summary>Every spawned player's revive system on this PC.</summary>
        public static IReadOnlyList<ReviveSystem> All => _all;

        public ReviveData Data => _data;
        public Health Health => _health;
        public bool HasGrave => _hasGrave.Value && _health.IsDead;
        public Vector2 GravePosition => _grave.Value;
        public bool IsReviving => _reviveStart.Value >= 0d;

        /// <summary>0..1 fill of the revive ring, on every PC.</summary>
        public float Progress => ReviveRules.Progress(_reviveStart.Value, NetworkManager.ServerTime.Time, _data.HoldSeconds);

        /// <summary>Server only: a solo extra life is bringing this player back, so they don't count as down.</summary>
        public bool ServerWillRevive => _soloRevive;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        public override void OnNetworkSpawn()
        {
            _all.Add(this);
            _health.Changed += OnHealthChanged;
        }

        public override void OnNetworkDespawn()
        {
            _all.Remove(this);
            _health.Changed -= OnHealthChanged;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || !IsReviving) return;
            if (!_soloRevive && !ReviverStillValid())
            {
                ServerCancel();
                return;
            }
            if (Progress >= 1f) ServerCompleteRevive();
        }

        /// <summary>Server only: a new floor (fight start or restart) gives back the solo extra life.</summary>
        public void ServerResetForFloor()
        {
            if (!IsServer) return;
            _livesUsed = 0;
            ServerCancel();
        }

        /// <summary>Server only: a downed player comes back at once with the revive share (ghosts revive at the draft).</summary>
        public void ServerReviveNow()
        {
            if (IsServer && _health.IsDead) ServerCompleteRevive();
        }

        /// <summary>
        /// A living player's PC says it started (<paramref name="holding"/> true) or stopped holding
        /// Interact on this gravestone. The server checks the reviver is alive and close enough.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void HoldRpc(bool holding, RpcParams rpcParams = default)
        {
            var reviver = PlayerHealth(rpcParams.Receive.SenderClientId);
            if (reviver == null || reviver == _health) return;

            if (!holding)
            {
                if (reviver == _reviver) ServerCancel();
                return;
            }
            if (!_health.IsDead || IsReviving || !InServerRange(reviver)) return;

            _reviver = reviver;
            _reviveStart.Value = NetworkManager.ServerTime.Time;
        }

        private void OnHealthChanged(int previous, int current)
        {
            if (previous > 0 && current <= 0)
            {
                if (IsOwner) DropGrave();
                if (IsServer) ServerOnDowned();
            }
            else if (previous <= 0 && current > 0)
            {
                if (IsOwner) _hasGrave.Value = false;
                if (IsServer) ServerCancel();
            }
        }

        private void DropGrave()
        {
            Vector2 spot = transform.position;
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = Layers.Mask(Layers.GroundIndex) };
            if (Physics2D.Raycast(spot, Vector2.down, filter, _groundHits, _data.GraveSnapDistance) > 0)
                spot = _groundHits[0].point;
            _grave.Value = spot;
            _hasGrave.Value = true;
        }

        private void ServerOnDowned()
        {
            var players = NetworkManager.ConnectedClientsIds.Count;
            if (!ReviveRules.UsesSoloLife(players, _livesUsed, _data.SoloExtraLivesPerFloor)) return;
            _livesUsed++;
            _soloRevive = true;
            _reviveStart.Value = NetworkManager.ServerTime.Time;
        }

        private void ServerCompleteRevive()
        {
            var hp = ReviveRules.ReviveHp(_health.Max, _data.ReviveHpFraction);
            ServerCancel();
            _health.ServerSetCurrent(hp);
        }

        private void ServerCancel()
        {
            _reviver = null;
            _soloRevive = false;
            if (IsServer && _reviveStart.Value >= 0d) _reviveStart.Value = ReviveRules.NotReviving;
        }

        private bool ReviverStillValid() =>
            _health.IsDead && _reviver != null && _reviver.IsSpawned && !_reviver.IsDead && InServerRange(_reviver);

        private bool InServerRange(Health reviver) =>
            _hasGrave.Value && ReviveRules.InRange(reviver.transform.position, _grave.Value,
                _data.ReviveRange + _data.ServerRangeSlack);

        private Health PlayerHealth(ulong clientId)
        {
            if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client)) return null;
            var player = client.PlayerObject;
            return player != null ? player.GetComponent<Health>() : null;
        }
    }
}
