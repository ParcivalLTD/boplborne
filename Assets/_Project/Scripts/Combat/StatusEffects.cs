using System.Collections.Generic;
using GraceRun.Bosses;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Timed statuses on a player, the dummy or a boss. The server writes them (an ability hit);
    /// every PC shows them: a shrunk body scales down (so it is harder to hit), and a slowed one
    /// moves slower on the PC that simulates it (the owner for a player, the server for a boss).
    /// A shrunk attacker deals less damage (see <see cref="DamageService"/>).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class StatusEffects : NetworkBehaviour
    {
        private static readonly Dictionary<ulong, StatusEffects> ById = new();

        private readonly NetworkVariable<double> _slowUntil = new();
        private readonly NetworkVariable<float> _slowMultiplier = new(1f);
        private readonly NetworkVariable<double> _shrinkUntil = new();
        private readonly NetworkVariable<float> _shrinkScale = new(1f);

        private Health _health;
        private PlayerMovement _movement;
        private BossMotor _motor;
        private Vector3 _baseScale;

        private double Now => NetworkManager.ServerTime.Time;

        /// <summary>True while a shrink is on (every PC).</summary>
        public bool IsShrunk => IsSpawned && Now < _shrinkUntil.Value;

        /// <summary>True while a slow is on (every PC).</summary>
        public bool IsSlowed => IsSpawned && Now < _slowUntil.Value;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _movement = GetComponent<PlayerMovement>();
            _motor = GetComponent<BossMotor>();
            _baseScale = transform.localScale;
        }

        public override void OnNetworkSpawn() => ById[NetworkObjectId] = this;

        public override void OnNetworkDespawn()
        {
            ById.Remove(NetworkObjectId);
            transform.localScale = _baseScale;
            if (_motor != null) _motor.StatusSpeedMultiplier = 1f;
        }

        private void Update()
        {
            if (!IsSpawned) return;
            var wanted = IsShrunk ? _baseScale * _shrinkScale.Value : _baseScale;
            var step = _health.Combat.StatusScaleSpeed * Time.deltaTime;
            transform.localScale = Vector3.MoveTowards(transform.localScale, wanted, step);

            var slowed = IsSlowed;
            if (_movement != null && IsOwner && slowed) _movement.ApplySlow(_slowMultiplier.Value, Time.deltaTime * 2f);
            if (_motor != null && IsServer) _motor.StatusSpeedMultiplier = slowed ? _slowMultiplier.Value : 1f;
        }

        /// <summary>The statuses of the spawned object <paramref name="networkObjectId"/>, if it has any.</summary>
        public static bool TryFind(ulong networkObjectId, out StatusEffects status) =>
            ById.TryGetValue(networkObjectId, out status) && status != null;

        /// <summary>Damage scale for hits this body deals: lower while shrunk.</summary>
        public float OutgoingDamageMultiplier => IsShrunk ? _health.Combat.ShrunkDamageMultiplier : 1f;

        /// <summary>Server only: puts <paramref name="kind"/> on for <paramref name="seconds"/>; a new status refreshes an older one.</summary>
        public void ServerApply(StatusKind kind, float seconds, float strength)
        {
            if (!IsServer || seconds <= 0f || _health.IsDead) return;
            var until = Now + seconds;
            switch (kind)
            {
                case StatusKind.Slow:
                    _slowMultiplier.Value = Mathf.Clamp(strength, 0.05f, 1f);
                    _slowUntil.Value = until;
                    break;
                case StatusKind.Shrink:
                    _shrinkScale.Value = Mathf.Clamp(strength, 0.1f, 1f);
                    _shrinkUntil.Value = until;
                    break;
            }
        }
    }
}
