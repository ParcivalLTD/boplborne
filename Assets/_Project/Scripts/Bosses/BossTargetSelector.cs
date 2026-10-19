using System.Collections.Generic;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Server side: picks which living player the boss goes after and switches to someone else
    /// every <see cref="BossData.TargetSwitchInterval"/> seconds (never in the middle of an attack).
    /// The target id is a NetworkVariable so every PC can show it later.
    /// </summary>
    public class BossTargetSelector : NetworkBehaviour
    {
        [SerializeField] private BossData _data;

        private readonly NetworkVariable<ulong> _targetId = new(
            BossTargeting.NoTarget, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly Dictionary<ulong, Health> _healthById = new();
        private readonly List<ulong> _living = new();
        private System.Random _random;
        private Transform _target;
        private Health _targetHealth;
        private float _switchTimer;

        public ulong TargetId => _targetId.Value;

        /// <summary>Server only: the target's transform, or null when nobody is alive.</summary>
        public Transform Target => HasTarget ? _target : null;

        /// <summary>Server only: false while the boss is attacking, so the target stays put.</summary>
        public bool AllowSwitch { get; set; } = true;

        /// <summary>Server only: optional linked twin selector whose target this boss avoids when 2+ players are alive.</summary>
        public BossTargetSelector PartnerSelector { get; set; }

        private bool HasTarget => _target != null && _targetHealth != null && _targetHealth.IsSpawned && !_targetHealth.IsDead;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            _random = new System.Random(System.Environment.TickCount ^ (int)NetworkObjectId);
            Switch();
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            _switchTimer -= Time.deltaTime;
            if (!HasTarget || (_switchTimer <= 0f && AllowSwitch)) Switch();
        }

        /// <summary>Server only: forces an immediate target re-evaluation (e.g. after linking twins).</summary>
        public void ServerReselect()
        {
            if (IsServer && IsSpawned) Switch();
        }

        private void Switch()
        {
            CollectLiving();
            var partnerTarget = PartnerSelector != null && PartnerSelector.IsSpawned
                ? PartnerSelector.TargetId
                : BossTargeting.NoTarget;
            var id = partnerTarget != BossTargeting.NoTarget
                ? Behaviours.BellTwinMath.ChooseSplitTarget(_targetId.Value, partnerTarget, _living, _random.Next())
                : BossTargeting.Choose(_targetId.Value, _living, _random.Next());
            _targetId.Value = id;
            _switchTimer = _data.TargetSwitchInterval;

            if (id != BossTargeting.NoTarget && _healthById.TryGetValue(id, out var health))
            {
                _targetHealth = health;
                _target = health.transform;
            }
            else
            {
                _targetHealth = null;
                _target = null;
            }
        }

        private void CollectLiving()
        {
            _living.Clear();
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                var id = player.NetworkObjectId;
                if (!_healthById.TryGetValue(id, out var health) || health == null)
                {
                    health = player.GetComponent<Health>();
                    _healthById[id] = health;
                }
                if (health != null && !health.IsDead) _living.Add(id);
            }
        }
    }
}
