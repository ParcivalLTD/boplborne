using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Tunable parameters for The Bell Twins' resonance link and Phase 2 survivor enrage.
    /// Lives in Data/Bosses/BellTwins/BellTwinData.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Bell Twin Data", fileName = "BellTwinData")]
    public class BellTwinData : ScriptableObject
    {
        [Header("Twin Spawn & Tether")]
        [SerializeField, Min(1f), Tooltip("Horizontal distance from the arena centre where each twin spawns (-X and +X).")]
        private float _spawnSeparationX = 6.5f;
        [SerializeField, Tooltip("Local offset on each twin where the bronze resonance tether attaches.")]
        private Vector2 _tetherOffset = new(0f, 0.8f);
        [SerializeField, Min(0.02f)] private float _tetherWidth = 0.14f;
        [SerializeField] private Color _tetherColor = new(0.88f, 0.72f, 0.42f, 0.75f);
        [SerializeField, Min(0f)] private float _tetherPulseAmplitude = 0.04f;
        [SerializeField, Min(0f)] private float _tetherPulseRate = 4f;

        [Header("Phase 2 Survivor Enrage")]
        [SerializeField, Min(1f), Tooltip("Walk speed multiplier applied to the surviving twin when the other dies.")]
        private float _enrageSpeedMultiplier = 1.35f;
        [SerializeField, Range(0.2f, 1f), Tooltip("Multiplier on IdleTime between attacks when enraged (lower = faster attacks).")]
        private float _enrageIdleMultiplier = 0.65f;
        [SerializeField, Tooltip("Bright bronze-gold tint applied to the surviving twin's crown and clapper when enraged.")]
        private Color _enrageTint = new(0.95f, 0.76f, 0.36f, 1f);

        public float SpawnSeparationX => _spawnSeparationX;
        public Vector2 TetherOffset => _tetherOffset;
        public float TetherWidth => _tetherWidth;
        public Color TetherColor => _tetherColor;
        public float TetherPulseAmplitude => _tetherPulseAmplitude;
        public float TetherPulseRate => _tetherPulseRate;
        public float EnrageSpeedMultiplier => _enrageSpeedMultiplier;
        public float EnrageIdleMultiplier => _enrageIdleMultiplier;
        public Color EnrageTint => _enrageTint;
    }
}
