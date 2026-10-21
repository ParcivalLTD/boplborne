using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Tunable values for a sticky <see cref="WebZone"/> floor hazard spawned by The Brood Mother.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Web Zone Data", fileName = "WebZoneData")]
    public class WebZoneData : ScriptableObject
    {
        [SerializeField, Min(0.5f)] private float _lifetime = 5.5f;
        [SerializeField, Min(0.5f), Tooltip("Horizontal half-width of the sticky floor patch.")]
        private float _radius = 2.6f;
        [SerializeField, Min(0.2f), Tooltip("Vertical reach above the floor.")]
        private float _height = 1.4f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Player run speed multiplier while standing in the web.")]
        private float _speedMultiplier = 0.45f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Boss walk speed multiplier while it stands in the web: lure it in.")]
        private float _bossSpeedMultiplier = 0.7f;
        [SerializeField, Min(0f)] private float _fadeOutTime = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _baseAlpha = 0.75f;

        public float Lifetime => _lifetime;
        public float Radius => _radius;
        public float Height => _height;
        public float SpeedMultiplier => _speedMultiplier;
        public float BossSpeedMultiplier => _bossSpeedMultiplier;
        public float FadeOutTime => _fadeOutTime;
        public float BaseAlpha => _baseAlpha;
    }
}
