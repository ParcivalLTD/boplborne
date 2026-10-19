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

        [Header("Movement")]
        [SerializeField, Range(0f, 1f), Tooltip("Run speed multiplier while swinging.")]
        private float _moveSpeedMultiplier = 0.2f;

        public string DisplayName => _displayName;
        public Color Color => _color;
        public Vector2 VisualSize => _visualSize;
        public float InputBuffer => _inputBuffer;
        public float ChainWindow => _chainWindow;
        public float MoveSpeedMultiplier => _moveSpeedMultiplier;

        /// <summary>The attacks flattened for <see cref="LightAttackCombo"/>.</summary>
        public WeaponMoveSet BuildMoves() => new(_combo, _heavy,
            _hasAerial ? _aerial : (LightAttackHit?)null,
            _hasUp ? _up : (LightAttackHit?)null,
            _hasDown ? _down : (LightAttackHit?)null);
    }
}
