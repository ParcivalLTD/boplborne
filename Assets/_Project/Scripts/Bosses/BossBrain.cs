using System;
using System.Collections;
using System.Collections.Generic;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Server-only boss AI: idle, choose an attack (weights, range, cooldowns, phase), walk closer
    /// when nothing reaches, run the attack through <see cref="BossAttackRunner"/>, recover, repeat.
    /// Scales max HP to the player count on spawn and enters phase 2 at the threshold.
    /// Clients only read the phase.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(BossAttackRunner), typeof(BossMotor))]
    [RequireComponent(typeof(BossTargetSelector))]
    public class BossBrain : NetworkBehaviour
    {
        [SerializeField] private BossData _data;

        private readonly NetworkVariable<byte> _phase = new(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly List<AttackOption> _options = new();
        private readonly List<bool> _ready = new();
        private Health _health;
        private BossAttackRunner _runner;
        private BossMotor _motor;
        private BossTargetSelector _targets;
        private System.Random _random;
        private float[] _readyAt;
        private float _idleTimer;
        private int _attackId = -1;

        /// <summary>The spawned boss on this PC, if any.</summary>
        public static BossBrain Active { get; private set; }

        /// <summary>Raised on every PC when a boss spawns or despawns (null).</summary>
        public static event Action<BossBrain> ActiveChanged;

        /// <summary>Raised on every PC when the phase changes (1 or 2).</summary>
        public event Action<int> PhaseChanged;

        public BossData Data => _data;
        public Health Health => _health;
        public int Phase => _phase.Value;
        public ulong TargetId => _targets.TargetId;

        /// <summary>Server only.</summary>
        public BossState State { get; private set; }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _runner = GetComponent<BossAttackRunner>();
            _motor = GetComponent<BossMotor>();
            _targets = GetComponent<BossTargetSelector>();
        }

        public override void OnNetworkSpawn()
        {
            _phase.OnValueChanged += OnPhaseValueChanged;
            Active = this;
            ActiveChanged?.Invoke(this);
            if (!IsServer) return;

            _random = new System.Random(Environment.TickCount);
            _readyAt = new float[CountAttackIds()];
            var players = Mathf.Max(1, NetworkManager.ConnectedClientsIds.Count);
            _health.ServerSetMax(BossMath.ScaledMaxHP(_data.MaxHP, _data.HpScalePerExtraPlayer, players));
            _health.Changed += OnHealthChanged;
            _health.Died += OnDied;
            _runner.PhaseChanged += OnAttackPhaseChanged;
            EnterIdle();
        }

        public override void OnNetworkDespawn()
        {
            _phase.OnValueChanged -= OnPhaseValueChanged;
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnDied;
            _runner.PhaseChanged -= OnAttackPhaseChanged;
            if (Active != this) return;
            Active = null;
            ActiveChanged?.Invoke(null);
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            switch (State)
            {
                case BossState.Idle:
                    FaceTarget();
                    _idleTimer -= Time.deltaTime;
                    if (_idleTimer <= 0f) Choose();
                    break;
                case BossState.Approaching:
                    Choose();
                    break;
            }
        }

        private void Choose()
        {
            var target = _targets.Target;
            if (target == null)
            {
                EnterIdle();
                return;
            }

            var attacks = _data.AttacksFor(_phase.Value);
            _options.Clear();
            _ready.Clear();
            foreach (var attack in attacks) _options.Add(AttackOption.From(attack));
            for (var i = 0; i < attacks.Length; i++) _ready.Add(Time.time >= _readyAt[_data.AttackId(_phase.Value, i)]);

            var distance = Mathf.Abs(target.position.x - transform.position.x);
            var index = AttackPicker.Pick(_options, _ready, distance, _random.NextDouble());
            if (index >= 0)
            {
                StartAttack(_data.AttackId(_phase.Value, index), target.position.x);
            }
            else if (AttackPicker.IsOutOfReach(_options, _ready, distance))
            {
                State = BossState.Approaching;
                _motor.ServerMoveToward(target.position.x);
            }
            else
            {
                EnterIdle();
            }
        }

        private void StartAttack(int attackId, float targetX)
        {
            var facing = _motor.DirectionTo(targetX);
            _motor.ServerStop();
            _motor.ServerFace(facing);
            _targets.AllowSwitch = false;
            _attackId = attackId;
            State = BossState.Attacking;
            _runner.ServerStart(attackId, _targets.TargetId, facing);
        }

        private void EnterIdle()
        {
            State = BossState.Idle;
            _idleTimer = _data.IdleTime;
            _motor.ServerStop();
        }

        private void FaceTarget()
        {
            var target = _targets.Target;
            if (target != null) _motor.ServerFace(_motor.DirectionTo(target.position.x));
        }

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            if (State == BossState.Dead) return;
            if (phase == AttackPhase.Recovery) State = BossState.Recovering;
            if (phase != AttackPhase.None || _attackId < 0) return;

            _readyAt[_attackId] = Time.time + _data.AttackById(_attackId).Cooldown;
            _attackId = -1;
            _targets.AllowSwitch = true;
            EnterIdle();
        }

        private void OnHealthChanged(int previous, int current)
        {
            if (_phase.Value < 2 && BossMath.IsInPhase2(current, _health.Max, _data.Phase2Threshold)) _phase.Value = 2;
        }

        private void OnDied()
        {
            State = BossState.Dead;
            _motor.ServerStop();
            _runner.ServerCancel();
            StartCoroutine(DespawnAfterDelay());
        }

        private IEnumerator DespawnAfterDelay()
        {
            yield return new WaitForSeconds(_data.DespawnDelay);
            if (IsSpawned) NetworkObject.Despawn();
        }

        private int CountAttackIds() => _data.AttacksFor(1).Length + (_data.AttacksFor(2) == _data.AttacksFor(1) ? 0 : _data.AttacksFor(2).Length);

        private void OnPhaseValueChanged(byte previous, byte current) => PhaseChanged?.Invoke(current);
    }
}
