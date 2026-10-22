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
    public partial class BossBrain : NetworkBehaviour
    {
        [SerializeField] private BossData _data;

        private static readonly List<BossBrain> AllActive = new();

        private readonly NetworkVariable<byte> _phase = new(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _encounterMaxHP = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly List<AttackOption> _options = new();
        private readonly List<bool> _ready = new();
        private Health _health;
        private BossAttackRunner _runner;
        private BossMotor _motor;
        private BossTargetSelector _targets;
        private System.Random _random;
        private int _seed;
        private bool _hasSeed;
        private bool _isLinkedBody;
        private float _idleMultiplier = 1f;
        private float[] _readyAt;
        private float _idleTimer;
        private int _attackId = -1;

        /// <summary>The primary living (or last spawned) boss on this PC, if any.</summary>
        public static BossBrain Active { get; private set; }

        /// <summary>All currently spawned boss bodies on this PC (1 for normal bosses, 2 for The Bell Twins).</summary>
        public static IReadOnlyList<BossBrain> All => AllActive;

        /// <summary>Raised on every PC when the active boss changes or despawns (null).</summary>
        public static event Action<BossBrain> ActiveChanged;

        /// <summary>True when no boss is spawned or every spawned boss body has reached 0 HP.</summary>
        public static bool AreAllDefeated
        {
            get
            {
                if (AllActive.Count == 0) return true;
                for (var i = 0; i < AllActive.Count; i++)
                    if (AllActive[i] != null && AllActive[i].IsSpawned && !AllActive[i]._health.IsDead) return false;
                return true;
            }
        }

        /// <summary>Sum of current HP across all spawned boss bodies on this PC.</summary>
        public static int EncounterCurrentHP
        {
            get
            {
                var sum = 0;
                for (var i = 0; i < AllActive.Count; i++)
                    if (AllActive[i] != null && !AllActive[i]._isEscort.Value && AllActive[i]._health.IsSpawned) sum += Mathf.Max(0, AllActive[i]._health.Current);
                return sum;
            }
        }

        /// <summary>Total encounter max HP (shared across linked twins even after the first twin despawns).</summary>
        public static int EncounterMaxHP
        {
            get
            {
                if (Active != null && Active._encounterMaxHP.Value > 0) return Active._encounterMaxHP.Value;
                var sum = 0;
                for (var i = 0; i < AllActive.Count; i++)
                    if (AllActive[i] != null && !AllActive[i]._isEscort.Value && AllActive[i]._health.IsSpawned) sum += AllActive[i]._health.Max;
                return Mathf.Max(1, sum);
            }
        }

        /// <summary>Normalized 0..1 HP for the top boss bar across one or multiple linked boss bodies.</summary>
        public static float EncounterNormalizedHP =>
            AllActive.Count > 0 ? Mathf.Clamp01((float)EncounterCurrentHP / EncounterMaxHP) : 0f;

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

        /// <summary>Server only, before Spawn: sets the random seed for this floor's boss.</summary>
        public void ServerSetSeed(int seed)
        {
            _seed = seed;
            _hasSeed = true;
        }

        public override void OnNetworkSpawn()
        {
            _phase.OnValueChanged += OnPhaseValueChanged;
            if (!AllActive.Contains(this)) AllActive.Add(this);
            RefreshActive();
            FeelNetworkSpawn();
            if (!IsServer) return;

            _idleMultiplier = 1f;
            _random = new System.Random(_hasSeed ? _seed : Environment.TickCount);
            _readyAt = new float[CountAttackIds()];
            var players = Mathf.Max(1, NetworkManager.ConnectedClientsIds.Count);
            var hpScale = FeelServerSpawn(players);
            if (!_isLinkedBody)
            {
                var scaled = Mathf.RoundToInt(BossMath.ScaledMaxHP(_data.MaxHP, _data.HpScalePerExtraPlayer, players) * hpScale);
                _health.ServerSetMax(scaled);
                _encounterMaxHP.Value = scaled;
            }
            _health.Changed += OnHealthChanged;
            _health.Died += OnDied;
            _runner.PhaseChanged += OnAttackPhaseChanged;
            EnterIdle();
        }

        public override void OnNetworkDespawn()
        {
            _phase.OnValueChanged -= OnPhaseValueChanged;
            FeelNetworkDespawn();
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnDied;
            _runner.PhaseChanged -= OnAttackPhaseChanged;
            AllActive.Remove(this);
            RefreshActive();
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            if (FeelTick()) return;
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

        /// <summary>Server only: configures this boss body as part of a multi-body linked encounter (The Bell Twins).</summary>
        public void ServerConfigureLinkedBody(int bodyMaxHP, int totalEncounterMaxHP)
        {
            if (!IsServer) return;
            _isLinkedBody = true;
            _encounterMaxHP.Value = Mathf.Max(1, totalEncounterMaxHP);
            _health.ServerSetMax(bodyMaxHP);
        }

        /// <summary>Server only: enters Phase 2 and speeds up movement and attack cadence (surviving Bell Twin).</summary>
        public void ServerEnrage(float speedMultiplier, float idleMultiplier)
        {
            if (!IsServer || State == BossState.Dead) return;
            _phase.Value = 2;
            _idleMultiplier = Mathf.Clamp(idleMultiplier, 0.2f, 1f);
            _motor.ServerSetSpeedMultiplier(speedMultiplier);
        }

        /// <summary>Server only: a ghost's poke holds an idle or walking boss in place a moment. Attacks run on.</summary>
        public void ServerStagger(float seconds)
        {
            if (!IsServer || (State != BossState.Idle && State != BossState.Approaching)) return;
            State = BossState.Idle;
            _idleTimer = Mathf.Max(_idleTimer, seconds);
            _motor.ServerStop();
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
            foreach (var attack in attacks) _options.Add(AttackOption.From(attack));

            var distance = Mathf.Abs(target.position.x - transform.position.x);
            BuildReady(attacks.Length, true);
            var index = AttackPicker.Pick(_options, _ready, distance, _random.NextDouble());
            if (index < 0)
            {
                BuildReady(attacks.Length, false);
                index = AttackPicker.Pick(_options, _ready, distance, _random.NextDouble());
            }
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

        private void BuildReady(int count, bool avoidRepeats)
        {
            _ready.Clear();
            for (var i = 0; i < count; i++)
            {
                var id = _data.AttackId(_phase.Value, i);
                _ready.Add(Time.time >= _readyAt[id] && !(avoidRepeats && FeelIsRepeat(id)));
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
            var feint = FeelOnAttackStart(attackId, _data.AttackById(attackId));
            _runner.ServerStart(attackId, _targets.TargetId, facing, feint);
        }

        private void EnterIdle()
        {
            State = BossState.Idle;
            _idleTimer = _data.IdleTime * _idleMultiplier * FeelIdleScale();
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
            if (phase == AttackPhase.Windup) State = BossState.Attacking;
            if (phase == AttackPhase.Recovery) State = BossState.Recovering;
            if (phase == AttackPhase.Active || phase == AttackPhase.Recovery) FeelOnAttackLeftWindup();
            if (phase != AttackPhase.None || _attackId < 0) return;

            _readyAt[_attackId] = Time.time + _data.AttackById(_attackId).Cooldown;
            _attackId = -1;
            _targets.AllowSwitch = true;
            if (FeelAfterAttack()) return;
            EnterIdle();
            FeelShapeIdle();
        }

        private void OnHealthChanged(int previous, int current)
        {
            if (_isLinkedBody || _phase.Value >= 2 || !BossMath.IsInPhase2(current, _health.Max, _data.Phase2Threshold)) return;
            _phase.Value = 2;
            FeelOnPhase2();
        }

        private void OnDied()
        {
            RefreshActive();
            if (!IsServer) return;
            State = BossState.Dead;
            _transitioning.Value = false;
            _staggered.Value = false;
            _motor.ServerStop();
            _runner.ServerCancel();
            StartCoroutine(DespawnAfterDelay());
        }

        private static void RefreshActive()
        {
            BossBrain next = null;
            for (var i = 0; i < AllActive.Count; i++)
            {
                var candidate = AllActive[i];
                if (candidate == null || !candidate.IsSpawned) continue;
                if (next == null) next = candidate;
                if (!candidate._health.IsDead)
                {
                    next = candidate;
                    break;
                }
            }
            if (Active == next) return;
            Active = next;
            ActiveChanged?.Invoke(Active);
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
