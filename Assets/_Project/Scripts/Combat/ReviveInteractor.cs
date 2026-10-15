using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Owner only: while this player is alive and holds Interact next to a teammate's gravestone,
    /// tells that gravestone's <see cref="ReviveSystem"/> (on the server) that a revive is held.
    /// Letting go, walking away or going down stops it.
    /// </summary>
    public class ReviveInteractor : NetworkBehaviour
    {
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Health _health;
        [SerializeField] private ReviveData _data;

        private ReviveSystem _holding;

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
        }

        public override void OnNetworkDespawn() => Release();

        private void OnDisable() => Release();

        private void Update()
        {
            var wanted = _input.InteractHeld && !_health.IsDead ? NearestGrave() : null;
            if (wanted == _holding) return;
            Release();
            if (wanted == null) return;
            _holding = wanted;
            _holding.HoldRpc(true);
        }

        private ReviveSystem NearestGrave()
        {
            Vector2 position = transform.position;
            ReviveSystem nearest = null;
            var best = float.MaxValue;
            foreach (var grave in ReviveSystem.All)
            {
                if (grave == null || grave.Health == _health || !grave.HasGrave) continue;
                var distance = (grave.GravePosition - position).sqrMagnitude;
                if (distance > _data.ReviveRange * _data.ReviveRange || distance >= best) continue;
                best = distance;
                nearest = grave;
            }
            return nearest;
        }

        private void Release()
        {
            if (_holding != null && _holding.IsSpawned) _holding.HoldRpc(false);
            _holding = null;
        }
    }
}
