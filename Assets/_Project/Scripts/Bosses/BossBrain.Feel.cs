using System;
using GraceRun.Combat;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>What the boss gauge above its head shows.</summary>
    public enum BossGaugeKind : byte
    {
        None,
        Poise,
        Interrupt,
        Stagger
    }

    /// <summary>
    /// The fight-feel half of <see cref="BossBrain"/>: phase-change roar, weak point bonus, enrage,
    /// poise and stagger, interrupts, player-count pressure, run variants and ability adaptation.
    /// All decisions are server side; clients read the synced flags to show them.
    /// </summary>
    public partial class BossBrain : IDamageModifier
    {
        private const int GaugeSteps = 100;

        /// <summary>Share of the interrupt damage each required contributor must deal.</summary>
        private const float InterruptContributorShare = 0.15f;

        private readonly NetworkVariable<byte> _variant = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<bool> _transitioning = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<bool> _staggered = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<bool> _enraged = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<byte> _gauge = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<byte> _gaugeKind = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private readonly NetworkVariable<ushort> _adaptedSource = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly PoiseMeter _poise = new();
        private readonly PatternMemory _memory = new();
        private readonly AdaptationTracker _adapt = new();
        private float _fightTime;
        private float _stateTimer;
        private float _stateTotal;
        private float _pendingStaggerTime;
        private bool _transitionPending;
        private bool _staggerPending;
        private float _interruptNeeded;
        private float _interruptTaken;
        private int _interruptContributors = 1;
        private readonly System.Collections.Generic.Dictionary<ulong, float> _interruptBy = new();
        private int _players = 1;

        /// <summary>Raised on every PC when the boss starts its phase change roar.</summary>
        public event Action TransitionStarted;

        /// <summary>Raised on every PC when the boss is staggered (poise broken or attack interrupted).</summary>
        public event Action StaggerStarted;

        public BossVariant Variant => (BossVariant)_variant.Value;
        public bool IsTransitioning => _transitioning.Value;
        public bool IsStaggered => _staggered.Value;
        public bool IsEnraged => _enraged.Value;
        public float GaugeFill => _gauge.Value / (float)GaugeSteps;
        public BossGaugeKind GaugeKind => (BossGaugeKind)_gaugeKind.Value;

        /// <summary>The ability id hash the boss has adapted to (0 = none); see <see cref="DamageSource.OfAbility"/>.</summary>
        public ushort AdaptedSource => _adaptedSource.Value;

        /// <summary>Multiplier for this boss's attack damage (the Brutal variant).</summary>
        public float DamageScale => BossFeelMath.DamageScale(Variant);

        private BossFeelSettings Feel => _data.Feel;

        /// <summary>Server only: a hazard (web) slows this boss while it stands in it. Call every physics step.</summary>
        public void ServerApplyEnvironmentSlow(float multiplier) => _motor.ServerApplyEnvironmentSlow(multiplier);

        private void FeelNetworkSpawn()
        {
            _transitioning.OnValueChanged += OnTransitioningChanged;
            _staggered.OnValueChanged += OnStaggeredChanged;
        }

        private void FeelNetworkDespawn()
        {
            _transitioning.OnValueChanged -= OnTransitioningChanged;
            _staggered.OnValueChanged -= OnStaggeredChanged;
        }

        private void OnTransitioningChanged(bool previous, bool current)
        {
            if (current) TransitionStarted?.Invoke();
        }

        private void OnStaggeredChanged(bool previous, bool current)
        {
            if (current) StaggerStarted?.Invoke();
        }

        /// <summary>Server, at spawn: rolls the variant and sets up poise. Returns the HP multiplier.</summary>
        private float FeelServerSpawn(int players)
        {
            _players = Mathf.Max(1, players);
            var variant = BossFeelMath.PickVariant(Feel.VariantChance, _random.NextDouble(), _random.Next());
            _variant.Value = (byte)variant;
            _poise.Configure(BossFeelMath.ScaledPool(Feel.PoiseMax, _data.HpScalePerExtraPlayer, _players) *
                             (variant == BossVariant.Relentless ? 1.5f : 1f));
            _memory.Clear();
            _adapt.Clear();
            _fightTime = 0f;
            _transitionPending = _staggerPending = false;
            _enraged.Value = _transitioning.Value = _staggered.Value = false;
            _adaptedSource.Value = 0;
            ApplySpeed();
            SetGauge(Feel.PoiseMax > 0f ? BossGaugeKind.Poise : BossGaugeKind.None, 0f);
            return BossFeelMath.HpScale(variant);
        }

        private void ApplySpeed() =>
            _motor.ServerSetSpeedMultiplier(BossFeelMath.SpeedScale(Variant) * (_enraged.Value ? Feel.EnrageSpeedScale : 1f));

        /// <summary>Server, each frame. True when the boss is busy roaring or staggered and must not choose an attack.</summary>
        private bool FeelTick()
        {
            if (State == BossState.Dead) return true;
            var dt = Time.deltaTime;
            if (HasLivingTarget) _fightTime += dt;
            if (!_enraged.Value && Feel.EnrageAfter > 0f && _fightTime >= Feel.EnrageAfter)
            {
                _enraged.Value = true;
                ApplySpeed();
            }

            if (State == BossState.Transitioning || State == BossState.Staggered)
            {
                _stateTimer -= dt;
                if (State == BossState.Staggered) SetGauge(BossGaugeKind.Stagger, _stateTotal > 0f ? _stateTimer / _stateTotal : 0f);
                if (_stateTimer <= 0f) EndTimedState();
                return true;
            }

            _poise.Tick(dt, Feel.PoiseRegen, Feel.PoiseRegenDelay);
            if (_interruptNeeded <= 0f) SetGauge(_poise.Max > 0f ? BossGaugeKind.Poise : BossGaugeKind.None, _poise.Normalized);
            return (State == BossState.Idle || State == BossState.Approaching) && FeelBeginPending();
        }

        private bool HasLivingTarget => _targets.Target != null;

        /// <summary>Starts a waiting roar or stagger. Roar first. True when one started.</summary>
        private bool FeelBeginPending()
        {
            if (_transitionPending)
            {
                BeginTransition();
                return true;
            }
            if (!_staggerPending) return false;
            BeginStagger(_pendingStaggerTime > 0f ? _pendingStaggerTime : Feel.StaggerTime);
            return true;
        }

        private void BeginTransition()
        {
            _transitionPending = false;
            _staggerPending = false;
            State = BossState.Transitioning;
            _stateTimer = _stateTotal = Feel.TransitionTime;
            _motor.ServerStop();
            _transitioning.Value = true;
            ShovePlayers();
        }

        private void BeginStagger(float seconds)
        {
            _staggerPending = false;
            _pendingStaggerTime = 0f;
            _interruptNeeded = 0f;
            State = BossState.Staggered;
            _stateTimer = _stateTotal = seconds;
            _motor.ServerStop();
            _poise.Reset();
            _staggered.Value = true;
            SetGauge(BossGaugeKind.Stagger, 1f);
        }

        private void EndTimedState()
        {
            _transitioning.Value = false;
            _staggered.Value = false;
            _poise.Reset();
            EnterIdle();
        }

        private void ShovePlayers()
        {
            var origin = transform.position;
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || !player.IsSpawned) continue;
                var offset = player.transform.position.x - origin.x;
                if (Mathf.Abs(offset) > Feel.TransitionShoveRadius) continue;
                if (!player.TryGetComponent(out Knockback knockback)) continue;
                var side = offset < 0f ? -1f : 1f;
                knockback.PushRpc(new Vector2(side * Feel.TransitionShove.x, Feel.TransitionShove.y));
            }
        }

        /// <summary>Server: called when the HP threshold flips the boss into phase 2.</summary>
        private void FeelOnPhase2()
        {
            if (Feel.TransitionTime > 0f) _transitionPending = true;
        }

        /// <summary>Server: when an attack starts. Rolls a feint and arms the interrupt.</summary>
        private bool FeelOnAttackStart(int attackId, AttackData attack)
        {
            _memory.Record(attackId);
            _interruptNeeded = attack.InterruptDamage > 0
                ? BossFeelMath.ScaledPool(attack.InterruptDamage, _data.HpScalePerExtraPlayer, _players)
                : 0f;
            _interruptTaken = 0f;
            _interruptBy.Clear();
            _interruptContributors = Mathf.Min(attack.InterruptContributors, _players);
            if (_interruptNeeded > 0f) SetGauge(BossGaugeKind.Interrupt, 0f);

            if (attack.Behaviour != null || attack.FollowUp != null || attack.InterruptDamage > 0) return false;
            var chance = Feel.FeintChance(_phase.Value) * (Variant == BossVariant.Relentless ? 1.5f : 1f);
            return _random.NextDouble() < chance;
        }

        /// <summary>Server: the wind-up ended, so it can no longer be interrupted.</summary>
        private void FeelOnAttackLeftWindup() => _interruptNeeded = 0f;

        /// <summary>Server: after an attack ends. Returns true when a roar or stagger took over the idle.</summary>
        private bool FeelAfterAttack()
        {
            _interruptNeeded = 0f;
            return FeelBeginPending();
        }

        /// <summary>Server: shapes the idle pause after an attack: pressure (skip it) or a reposition step.</summary>
        private void FeelShapeIdle()
        {
            var pressure = BossFeelMath.PressureChance(_players, Feel.PressureChancePerPlayer) +
                           (Variant == BossVariant.Relentless ? 0.1f : 0f);
            if (_random.NextDouble() < pressure)
            {
                _idleTimer = 0.05f;
                return;
            }

            var target = _targets.Target;
            if (target == null || _random.NextDouble() >= Feel.RepositionChance) return;
            var distance = Mathf.Abs(target.position.x - transform.position.x);
            var toward = _motor.DirectionTo(target.position.x);
            var direction = distance < Feel.RepositionBackOffRange ? -toward : toward;
            _motor.ServerMoveToward(transform.position.x + direction * 8f);
            _idleTimer = Feel.RepositionTime * _idleMultiplier;
        }

        private float FeelIdleScale() =>
            BossFeelMath.IdleScale(_players, Feel.IdleShrinkPerExtraPlayer) * BossFeelMath.IdleScale(Variant) *
            (_enraged.Value ? Feel.EnrageIdleScale : 1f);

        /// <summary>Server: true when attack <paramref name="attackId"/> would be a third repeat in a row.</summary>
        private bool FeelIsRepeat(int attackId) => _memory.IsBlocked(attackId, Feel.MaxRepeats);

        /// <inheritdoc />
        public int ModifyIncoming(int amount, DamageInfo info)
        {
            if (!IsServer || State == BossState.Dead) return amount;
            if (_transitioning.Value) return 0;

            var scale = 1f;
            if (InWeakPointWindow()) scale *= Feel.WeakPointMultiplier;
            if (_staggered.Value) scale *= Feel.StaggerDamageMultiplier;
            if (info.Source > DamageSource.Melee)
            {
                if (info.Source == _adaptedSource.Value) scale *= Feel.AdaptResistance;
                _adapt.Record(info.Source, Time.time);
                _adaptedSource.Value = _adapt.Dominant(Time.time, Feel.AdaptWindow, Feel.AdaptMinHits, Feel.AdaptShare);
            }

            var dealt = Mathf.Max(1, Mathf.RoundToInt(amount * scale));
            if (!_staggered.Value) AddPoiseAndInterrupt(dealt, info.AttackerId);
            return dealt;
        }

        private bool InWeakPointWindow() =>
            _runner.Phase == AttackPhase.Recovery && _runner.Attack != null &&
            BossFeelMath.InWeakPointWindow(true, _runner.SinceRecovery, _runner.Attack.PunishWindow, Feel.WeakPointGrace);

        private void AddPoiseAndInterrupt(int dealt, ulong attackerId)
        {
            if (State == BossState.Attacking && _runner.Phase == AttackPhase.Windup && _interruptNeeded > 0f)
            {
                _interruptTaken += dealt;
                _interruptBy[attackerId] = (_interruptBy.TryGetValue(attackerId, out var mine) ? mine : 0f) + dealt;
                SetGauge(BossGaugeKind.Interrupt, _interruptTaken / _interruptNeeded);
                if (_interruptTaken >= _interruptNeeded && HasEnoughContributors())
                {
                    Interrupt();
                    return;
                }
            }
            if (_poise.Add(dealt)) _staggerPending = true;
        }

        private bool HasEnoughContributors()
        {
            var share = _interruptNeeded * InterruptContributorShare;
            var count = 0;
            foreach (var pair in _interruptBy)
                if (pair.Value >= share) count++;
            return count >= _interruptContributors;
        }

        private void Interrupt()
        {
            _interruptNeeded = 0f;
            _pendingStaggerTime = Feel.InterruptStaggerTime;
            _staggerPending = true;
            _runner.ServerCancel();
            if (State != BossState.Attacking) FeelBeginPending();
        }

        private void SetGauge(BossGaugeKind kind, float fill)
        {
            var steps = (byte)Mathf.Clamp(Mathf.RoundToInt(fill * GaugeSteps), 0, GaugeSteps);
            if (_gaugeKind.Value != (byte)kind) _gaugeKind.Value = (byte)kind;
            if (_gauge.Value != steps) _gauge.Value = steps;
        }
    }
}
