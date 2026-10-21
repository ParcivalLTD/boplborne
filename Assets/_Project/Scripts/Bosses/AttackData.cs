using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// One boss attack: timing, damage, hitbox, telegraph and how the brain picks it.
    /// Tune the asset in the boss folder under Data/Bosses, never the code.
    /// Offsets are relative to the boss and written for a boss facing right; they mirror when it faces left.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Attack Data", fileName = "AttackData")]
    public class AttackData : ScriptableObject
    {
        [SerializeField] private string _id = "attack";

        [Header("Timing (seconds)")]
        [SerializeField, Min(0f)] private float _windup = 0.8f;
        [SerializeField, Min(0f)] private float _active = 0.15f;
        [SerializeField, Min(0f)] private float _recovery = 1f;
        [SerializeField, Min(0f), Tooltip("How long the gold weak point shows at the start of the recovery. Capped at the recovery time.")]
        private float _punishWindow = 0.8f;

        [Header("Hit")]
        [SerializeField, Min(0)] private int _damage = 30;
        [SerializeField, Tooltip("Velocity given to the player hit; x points away from the boss.")]
        private Vector2 _knockback = new(8f, 6f);
        [SerializeField] private Vector2 _hitboxOffset = new(3.5f, -1.5f);
        [SerializeField] private Vector2 _hitboxSize = new(4f, 3f);

        [Header("Telegraph")]
        [SerializeField] private TelegraphType _telegraphType = TelegraphType.GroundZone;
        [SerializeField, Min(0), Tooltip("Index into the boss Telegraph glow parts: which body part glows red.")]
        private int _glowPart;
        [SerializeField, Tooltip("Where the gold weak point appears during recovery.")]
        private Vector2 _weakPointOffset = new(0f, 1f);

        [Header("Lunge (server moves the boss during the active phase)")]
        [SerializeField, Min(0f), Tooltip("Units the boss dashes forward. 0 = stands still.")]
        private float _lungeDistance;
        [SerializeField, Min(0f), Tooltip("Seconds the dash takes, from the start of the active phase. Keep it shorter than Active so the hitbox stays armed a moment after the boss stops (covers client interpolation).")]
        private float _lungeTime = 0.2f;

        [Header("Chain, behaviour and sound")]
        [SerializeField, Tooltip("Played right after this attack ends, on every PC, with no pause and no new pick. Same facing and target.")]
        private AttackData _followUp;
        [SerializeField, Tooltip("Optional boss-specific behaviour (spawning minions, sticky webs, ceiling drop).")]
        private BossAttackBehaviour _behaviour;
        [SerializeField, Tooltip("Telegraph sound played when the wind-up starts (one per boss telegraph).")]
        private AudioCue _windupCue;
        [SerializeField, Tooltip("Impact sound played when the attack goes live (the active phase starts). Empty: silent.")]
        private AudioCue _strikeCue;

        [Header("Choice")]
        [SerializeField, Min(0f)] private float _weight = 1f;
        [SerializeField, Min(0f), Tooltip("Seconds after this attack ends before the brain may pick it again.")]
        private float _cooldown;
        [SerializeField, Min(0f), Tooltip("Horizontal distance to the target, in units.")]
        private float _minRange;
        [SerializeField, Min(0f)] private float _maxRange = 7f;

        public string Id => _id;
        public float Windup => _windup;
        public float Active => _active;
        public float Recovery => _recovery;
        public float PunishWindow => Mathf.Min(_punishWindow, _recovery);
        public float Duration => _windup + _active + _recovery;
        public int Damage => _damage;
        public Vector2 Knockback => _knockback;
        public Vector2 HitboxOffset => _hitboxOffset;
        public Vector2 HitboxSize => _hitboxSize;
        public TelegraphType TelegraphType => _telegraphType;
        public int GlowPart => _glowPart;
        public Vector2 WeakPointOffset => _weakPointOffset;
        public float LungeDistance => _lungeDistance;
        public float LungeTime => Mathf.Min(_lungeTime, _active);
        public AttackData FollowUp => _followUp;
        public BossAttackBehaviour Behaviour => _behaviour;
        public AudioCue WindupCue => _windupCue;
        public AudioCue StrikeCue => _strikeCue;
        public float Weight => _weight;
        public float Cooldown => _cooldown;
        public float MinRange => _minRange;
        public float MaxRange => _maxRange;

        /// <summary>Where the attack is <paramref name="elapsed"/> seconds after its start tick.</summary>
        public AttackPhase PhaseAt(double elapsed)
        {
            if (elapsed < 0d) return AttackPhase.Pending;
            if (elapsed < _windup) return AttackPhase.Windup;
            if (elapsed < _windup + _active) return AttackPhase.Active;
            return elapsed < Duration ? AttackPhase.Recovery : AttackPhase.None;
        }
    }
}
