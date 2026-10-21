using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>The Brood Crawler's sprites and walk speed (art faces right; the prefab mirrors it with the facing).</summary>
    [CreateAssetMenu(menuName = "Grace Run/Crawler Art Set", fileName = "CrawlerArt")]
    public class CrawlerArtSet : ScriptableObject
    {
        [SerializeField] private Sprite[] _walk;
        [SerializeField, Min(0.1f)] private float _walkFps = 10f;
        [SerializeField] private Sprite _idle;
        [SerializeField] private Sprite _hit;
        [SerializeField, Min(0f), Tooltip("Horizontal speed in units per second above which the walk cycle plays.")]
        private float _walkSpeedThreshold = 0.4f;

        public Sprite[] Walk => _walk;
        public float WalkFps => _walkFps;
        public Sprite Idle => _idle;
        public Sprite Hit => _hit;
        public float WalkSpeedThreshold => _walkSpeedThreshold;
    }
}
