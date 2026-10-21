using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// One weapon: a light combo, a heavy attack and optional aerial, up and down attacks, all in
    /// the <see cref="LightAttackHit"/> shape (offsets and knockback for a player facing right).
    /// A weapon without an aerial / up / down attack uses its light combo there. Tune the assets
    /// under Data/Weapons, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Weapon Data", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string _displayName = "Fists";
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Tooltip("Placeholder weapon shape: length and thickness in world units. 0 length draws nothing.")]
        private Vector2 _visualSize = Vector2.zero;

        [Header("Art (Art/Weapons)")]
        [SerializeField, Tooltip("The held weapon, drawn pointing right with the grip at its pivot. Empty draws nothing.")]
        private Sprite _sprite;
        [SerializeField, Tooltip("The swoosh drawn during the swing, convex side pointing right.")]
        private Sprite _smear;
        [SerializeField, Tooltip("Idle, run, jump and fall with this weapon in hand (an override of the player's Animator). Empty: the plain clips, and the weapon rests on the back.")]
        private AnimatorOverrideController _animations;
        [SerializeField] private WeaponSwingStyle _swingStyle;
        [SerializeField, Tooltip("The body poses with this weapon drawn in. Empty: the body plays the punch clips (Fists) and the weapon sprite is drawn on top.")]
        private WeaponBodyPoses _bodyPoses;
        [SerializeField, Range(0f, 180f), Tooltip("Arc style: degrees above the aim the weapon is raised before the strike.")]
        private float _windAngle = 70f;
        [SerializeField, Range(0f, 270f), Tooltip("Arc style: degrees the weapon sweeps through.")]
        private float _sweepAngle = 110f;
        [SerializeField, Min(0f), Tooltip("Arc style: grip distance from the player's centre. Thrust style: how far the grip is pushed out.")]
        private float _swingReach = 0.5f;
        [SerializeField, Min(0f), Tooltip("Smear size multiplier.")]
        private float _smearScale = 1f;

        [Header("Attacks")]
        [SerializeField] private LightAttackHit[] _combo = { default };
        [SerializeField] private LightAttackHit _heavy;
        [SerializeField] private bool _hasAerial;
        [SerializeField] private LightAttackHit _aerial;
        [SerializeField] private bool _hasUp;
        [SerializeField] private LightAttackHit _up;
        [SerializeField] private bool _hasDown;
        [SerializeField] private LightAttackHit _down;

        [Header("Chaining")]
        [SerializeField, Min(0f), Tooltip("A press this long before the current swing ends still starts the next one.")]
        private float _inputBuffer = 0.3f;
        [SerializeField, Min(0f), Tooltip("After a swing ends, a press within this time continues the combo instead of restarting it.")]
        private float _chainWindow = 0.35f;

        [Header("Sounds (empty = the generic cues in PlayerFeedbackData / HitFeedbackData)")]
        [SerializeField, Tooltip("Light swings.")] private AudioCue _swingCue;
        [SerializeField, Tooltip("The last light hit and Heavy.")] private AudioCue _finisherCue;
        [SerializeField, Tooltip("A light hit lands.")] private AudioCue _hitCue;
        [SerializeField, Tooltip("A heavy hit lands (empty: the hit cue).")] private AudioCue _heavyHitCue;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f), Tooltip("Run speed multiplier while swinging.")]
        private float _moveSpeedMultiplier = 0.2f;

        public string DisplayName => _displayName;
        public Color Color => _color;
        public Vector2 VisualSize => _visualSize;
        public Sprite Sprite => _sprite;
        public WeaponBodyPoses BodyPoses => _bodyPoses;
        public AnimatorOverrideController Animations => _animations;
        public Sprite Smear => _smear;
        public float SmearScale => _smearScale;
        public WeaponSwingLook SwingLook => new(_swingStyle, _windAngle, _sweepAngle, _swingReach);
        public float InputBuffer => _inputBuffer;
        public float ChainWindow => _chainWindow;
        public float MoveSpeedMultiplier => _moveSpeedMultiplier;
        public AudioCue SwingCue => _swingCue;
        public AudioCue FinisherCue => _finisherCue;
        public AudioCue HitCue => _hitCue;
        public AudioCue HeavyHitCue => _heavyHitCue != null ? _heavyHitCue : _hitCue;

        /// <summary>The attacks flattened for <see cref="LightAttackCombo"/>.</summary>
        public WeaponMoveSet BuildMoves() => new(_combo, _heavy,
            _hasAerial ? _aerial : (LightAttackHit?)null,
            _hasUp ? _up : (LightAttackHit?)null,
            _hasDown ? _down : (LightAttackHit?)null);
    }
}
