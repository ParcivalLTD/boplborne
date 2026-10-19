using System;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Hit points of a player, boss or dummy. Only the server writes HP (server-write
    /// NetworkVariable); every client gets <see cref="Changed"/> and <see cref="Died"/> from the
    /// synced value. Hits arrive through <see cref="ReportHit"/> and are resolved by
    /// <see cref="DamageService"/>; <see cref="HitTaken"/> drives the hit juice on every PC.
    /// </summary>
    public class Health : NetworkBehaviour
    {
        [SerializeField, Tooltip("Any data asset that implements IHealthStats (PlayerStatsData, TrainingDummyData...).")]
        private ScriptableObject _stats;
        [SerializeField] private CombatData _combat;
        [SerializeField] private Team _team = Team.Neutral;

        private readonly NetworkVariable<int> _current = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _maxOverride = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _godMode = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private IHealthStats _healthStats;

        /// <summary>Raised on every client (and the server) when HP changes: previous, current.</summary>
        public event Action<int, int> Changed;

        /// <summary>Raised on every client (and the server) when HP reaches 0.</summary>
        public event Action Died;

        /// <summary>
        /// Raised once per landed hit on every PC, for cosmetics. The PC that detected the hit
        /// raises it at once; the server echoes it to everyone else, so nobody waits or doubles up.
        /// </summary>
        public event Action<DamageInfo> HitTaken;

        public int Current => _current.Value;
        public int Max => HealthRules.Max(_maxOverride.Value, _healthStats != null ? _healthStats.MaxHP : 1);
        public float Normalized => (float)_current.Value / Max;
        public bool IsDead => IsSpawned && _current.Value <= 0;
        public Team Team => _team;
        public bool GodMode => _godMode.Value;
        public CombatData Combat => _combat;

        private void Awake()
        {
            _healthStats = _stats as IHealthStats;
            if (_healthStats == null) Debug.LogError($"{name}: Health needs a stats asset that implements IHealthStats.", this);
        }

        private void OnValidate()
        {
            if (_stats == null || _stats is IHealthStats) return;
            Debug.LogWarning($"{_stats.name} does not implement IHealthStats.", this);
            _stats = null;
        }

        public override void OnNetworkSpawn()
        {
            _current.OnValueChanged += OnCurrentChanged;
            if (IsServer) _current.Value = Max;
        }

        public override void OnNetworkDespawn()
        {
            _current.OnValueChanged -= OnCurrentChanged;
        }

        /// <summary>
        /// Called by whichever PC detected the hit (attacker or victim side): plays the hit
        /// feedback here right away and sends the hit to the server, which resolves it.
        /// </summary>
        public void ReportHit(DamageInfo info)
        {
            if (!IsSpawned || IsDead) return;
            if (!DamageService.IsIgnored(this, info)) HitTaken?.Invoke(info);
            ReportHitRpc(info);
        }

        /// <summary>Server only: tells every PC except the reporter (who already played it) that a hit landed.</summary>
        public void ServerBroadcastHit(DamageInfo info, ulong reporterClientId)
        {
            if (IsServer) HitTakenRpc(info, RpcTarget.Not(reporterClientId, RpcTargetUse.Temp));
        }

        /// <summary>Debug: turns god mode on or off for this target.</summary>
        [Rpc(SendTo.Server)]
        public void SetGodModeRpc(bool on) => _godMode.Value = on;

        /// <summary>Debug: refills HP.</summary>
        [Rpc(SendTo.Server)]
        public void RestoreFullRpc() => ServerRestoreFull();

        /// <summary>Debug: sets HP (clamped to 0..Max).</summary>
        [Rpc(SendTo.Server)]
        public void DebugSetCurrentRpc(int value) => ServerSetCurrent(value);

        /// <summary>
        /// Server only: replaces the stats max HP (boss HP scaling, Iron Skin) and refills to it.
        /// 0 or less clears the override, back to the stats asset's max HP.
        /// </summary>
        public void ServerSetMax(int max)
        {
            if (!IsServer) return;
            _maxOverride.Value = HealthRules.OverrideFor(max);
            _current.Value = Max;
        }

        /// <summary>Server only: sets HP, clamped to 0..Max.</summary>
        public void ServerSetCurrent(int value)
        {
            if (!IsServer) return;
            _current.Value = Mathf.Clamp(value, 0, Max);
        }

        /// <summary>Server only: back to full HP.</summary>
        public void ServerRestoreFull() => ServerSetCurrent(Max);

        [Rpc(SendTo.Server)]
        private void ReportHitRpc(DamageInfo info, RpcParams rpcParams = default) =>
            DamageService.Apply(this, info, rpcParams.Receive.SenderClientId);

        [Rpc(SendTo.SpecifiedInParams)]
        private void HitTakenRpc(DamageInfo info, RpcParams rpcParams) => HitTaken?.Invoke(info);

        private void OnCurrentChanged(int previous, int current)
        {
            Changed?.Invoke(previous, current);
            if (previous > 0 && current <= 0) Died?.Invoke();
        }
    }
}
