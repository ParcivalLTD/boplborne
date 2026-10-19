using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>Tunable numbers for the mouse slice (hold left mouse and drag across a target).</summary>
    [CreateAssetMenu(menuName = "Grace Run/Player/Slice Data", fileName = "SliceData")]
    public class SliceData : ScriptableObject
    {
        [Header("Gesture")]
        [SerializeField, Min(0f), Tooltip("World units the cursor must move while held before the press becomes a slice (shorter = a click, which fires Ability 1 on release).")]
        private float _startDistance = 0.6f;
        [SerializeField, Min(0.05f), Tooltip("A stroke stops cutting after this long, even if the button is still held.")]
        private float _maxStrokeTime = 0.6f;
        [SerializeField, Min(0f), Tooltip("Seconds after a stroke ends before the next one can start.")]
        private float _cooldown = 0.2f;

        [Header("Hit")]
        [SerializeField, Min(0.5f), Tooltip("Only the part of the cursor path within this many units of the player cuts.")]
        private float _reach = 3.5f;
        [SerializeField, Min(0)] private int _damage = 10;
        [SerializeField, Min(0f)] private float _staminaCost = 12f;
        [SerializeField, Min(0f), Tooltip("Push speed along the direction of the cut.")]
        private float _knockback = 6f;
        [SerializeField, Min(0f), Tooltip("Extra upward push speed.")]
        private float _knockbackLift = 3f;

        [Header("Trail (visual only)")]
        [SerializeField, Min(2)] private int _maxTrailPoints = 24;
        [SerializeField, Min(0.01f)] private float _trailWidth = 0.18f;
        [SerializeField, Min(0.01f)] private float _trailFadeTime = 0.18f;

        public float StartDistance => _startDistance;
        public float MaxStrokeTime => _maxStrokeTime;
        public float Cooldown => _cooldown;
        public float Reach => _reach;
        public int Damage => _damage;
        public float StaminaCost => _staminaCost;
        public float Knockback => _knockback;
        public float KnockbackLift => _knockbackLift;
        public int MaxTrailPoints => _maxTrailPoints;
        public float TrailWidth => _trailWidth;
        public float TrailFadeTime => _trailFadeTime;
    }
}
