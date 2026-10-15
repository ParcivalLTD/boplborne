using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Look of every boss telegraph: the red "dodge now" glow and zone, and the gold "hit now" weak point.
    /// Red and gold are reserved for gameplay signals (docs/GDD.md). Asset under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Telegraph Data", fileName = "TelegraphData")]
    public class TelegraphData : ScriptableObject
    {
        [SerializeField] private Color _telegraphRed = new(1f, 0.231f, 0.231f, 1f);
        [SerializeField] private Color _weakPointGold = new(1f, 0.788f, 0.290f, 1f);

        [Header("Glow on the boss part")]
        [SerializeField, Range(0f, 1f)] private float _glowMinAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _glowMaxAlpha = 0.9f;
        [SerializeField, Min(0f), Tooltip("Pulses per second at the start of the wind-up.")]
        private float _pulseRateStart = 3f;
        [SerializeField, Min(0f), Tooltip("Pulses per second right before the strike (it speeds up).")]
        private float _pulseRateEnd = 10f;

        [Header("Ground zone")]
        [SerializeField, Range(0f, 1f)] private float _zoneWindupAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _zoneActiveAlpha = 0.6f;

        [Header("Weak point")]
        [SerializeField, Min(0f)] private float _weakPointPulseRate = 4f;
        [SerializeField, Min(0f)] private float _weakPointPulseScale = 0.15f;

        public Color TelegraphRed => _telegraphRed;
        public Color WeakPointGold => _weakPointGold;
        public float GlowMinAlpha => _glowMinAlpha;
        public float GlowMaxAlpha => _glowMaxAlpha;
        public float PulseRateStart => _pulseRateStart;
        public float PulseRateEnd => _pulseRateEnd;
        public float ZoneWindupAlpha => _zoneWindupAlpha;
        public float ZoneActiveAlpha => _zoneActiveAlpha;
        public float WeakPointPulseRate => _weakPointPulseRate;
        public float WeakPointPulseScale => _weakPointPulseScale;
    }
}
