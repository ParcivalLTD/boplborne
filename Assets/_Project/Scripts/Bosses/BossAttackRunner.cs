using System;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Plays the current boss attack on every PC (server and clients) from the shared start tick
    /// in <see cref="BossAttackState"/>: wind-up, active (victim-side hitbox armed), recovery.
    /// Because every PC times it from server time, all screens show the telegraph at the same
    /// moment and each player is only hit if they are in the hitbox on their own screen.
    /// Follow-up attacks (<see cref="AttackData.FollowUp"/>) play straight after, from the same tick.
    /// </summary>
    public class BossAttackRunner : NetworkBehaviour
    {
        [SerializeField] private BossData _data;
        [SerializeField, Tooltip("Victim-side hitbox on the BossAttack layer.")]
        private Hitbox _hitbox;
        [SerializeField] private BoxCollider2D _hitboxShape;

        private readonly NetworkVariable<BossAttackState> _current = new(
            BossAttackState.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private BossBrain _brain;
        private AttackData _first;
        private AttackData _attack;
        private BossAttackState _playing = BossAttackState.None;
        private double _firstStartTime;
        private double _startTime;
        private ushort _sequence;

        /// <summary>Raised on every PC when the attack moves to a new phase (None when it ends or is cancelled).</summary>
        public event Action<AttackPhase> PhaseChanged;

        /// <summary>The link of the chain playing now (the first attack, or one of its follow-ups).</summary>
        public AttackData Attack => _attack;
        public AttackPhase Phase { get; private set; }
        public sbyte Facing => _playing.Facing;
        public ulong TargetId => _playing.TargetId;
        public int PlayingId => _playing.AttackId;

        /// <summary>Counts up with every attack the server starts; identical on every PC, so it can seed shared random picks.</summary>
        public ushort Sequence => _playing.Sequence;
        public bool IsFeint => _playing.Feint;

        /// <summary>Seconds since the current link's start on this PC (negative while pending).</summary>
        public double Elapsed => _attack != null && IsSpawned ? NetworkManager.ServerTime.Time - _startTime : 0d;

        /// <summary>Seconds since the recovery began (negative before it). A feint has no active phase.</summary>
        public float SinceRecovery => _attack == null ? -1f : (float)Elapsed - _attack.Windup - (_playing.Feint ? 0f : _attack.Active);

        private void Awake() => _brain = GetComponent<BossBrain>();

        public override void OnNetworkSpawn()
        {
            _current.OnValueChanged += OnCurrentChanged;
            Begin(_current.Value);
        }

        public override void OnNetworkDespawn()
        {
            _current.OnValueChanged -= OnCurrentChanged;
            Stop();
        }

        private void Update()
        {
            if (_first == null || !IsSpawned) return;
            var linkStart = 0d;
            var link = _playing.Feint ? _first : AttackChain.LinkAt(_first, NetworkManager.ServerTime.Time - _firstStartTime, out linkStart);
            var linkChanged = link != _attack;
            if (linkChanged)
            {
                if (_attack != null && _attack.Behaviour != null)
                    _attack.Behaviour.OnPhaseChanged(this, _attack, AttackPhase.None);
                _attack = link;
                _startTime = _firstStartTime + linkStart;
            }

            var phase = _playing.Feint ? FeintPhaseAt(_attack, Elapsed) : _attack.PhaseAt(Elapsed);
            if (phase != Phase || linkChanged) SetPhase(phase);
        }

        /// <summary>Server only: starts attack <paramref name="attackId"/> a little in the future (the start lead).</summary>
        public void ServerStart(int attackId, ulong targetId, sbyte facing, bool feint = false)
        {
            if (!IsServer) return;
            var time = NetworkManager.ServerTime;
            var leadTicks = Mathf.CeilToInt(_data.AttackStartLead * time.TickRate);
            _current.Value = new BossAttackState
            {
                Sequence = ++_sequence,
                AttackId = (short)attackId,
                StartTick = time.Tick + leadTicks,
                TargetId = targetId,
                Facing = facing,
                Feint = feint
            };
        }

        /// <summary>Server only: stops the attack on every PC (boss died, fight reset).</summary>
        public void ServerCancel()
        {
            if (IsServer) _current.Value = BossAttackState.None;
        }

        private void OnCurrentChanged(BossAttackState previous, BossAttackState current) => Begin(current);

        private void Begin(BossAttackState state)
        {
            Stop();
            if (state.IsNone) return;

            _first = _data.AttackById(state.AttackId);
            if (_first == null) return;
            _attack = _first;
            _playing = state;
            _firstStartTime = new NetworkTime(NetworkManager.ServerTime.TickRate, state.StartTick).Time;
            _startTime = _firstStartTime;
            Update();
        }

        private void Stop()
        {
            if (_attack != null && Phase != AttackPhase.None) SetPhase(AttackPhase.None);
            _first = null;
            _attack = null;
            _playing = BossAttackState.None;
        }

        private void SetPhase(AttackPhase phase)
        {
            if (Phase == AttackPhase.Active) _hitbox.Disarm();
            Phase = phase;
            if (phase == AttackPhase.Active) ArmHitbox();
            if (_attack != null && _attack.Behaviour != null)
                _attack.Behaviour.OnPhaseChanged(this, _attack, phase);

            PhaseChanged?.Invoke(phase);
            if (phase != AttackPhase.None) return;
            _first = null;
            _attack = null;
        }

        private AttackPhase FeintPhaseAt(AttackData attack, double elapsed)
        {
            if (elapsed < 0d) return AttackPhase.Pending;
            if (elapsed < attack.Windup) return AttackPhase.Windup;
            return elapsed < attack.Windup + _data.Feel.FeintRecovery ? AttackPhase.Recovery : AttackPhase.None;
        }

        private void ArmHitbox()
        {
            var facing = _playing.Facing;
            var scale = _brain != null ? _brain.DamageScale : 1f;
            _hitboxShape.size = _attack.HitboxSize;
            _hitboxShape.offset = new Vector2(_attack.HitboxOffset.x * facing, _attack.HitboxOffset.y);
            _hitbox.Arm(Mathf.RoundToInt(_attack.Damage * scale), new Vector2(_attack.Knockback.x * facing, _attack.Knockback.y));
        }
    }
}
