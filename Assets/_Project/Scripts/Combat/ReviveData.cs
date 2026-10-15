using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Tuning for downing, ghosts and revives (docs/GDD.md co-op rules).
    /// Tune the asset under Data/Config, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Revive Data", fileName = "ReviveData")]
    public class ReviveData : ScriptableObject
    {
        [Header("Revive")]
        [SerializeField, Min(0.1f), Tooltip("GDD: hold Interact on the gravestone for 3 s.")]
        private float _holdSeconds = 3f;
        [SerializeField, Range(0.01f, 1f), Tooltip("GDD: back with 30% HP.")]
        private float _reviveHpFraction = 0.3f;
        [SerializeField, Min(0.1f), Tooltip("How close (units) the reviver must stand to the gravestone.")]
        private float _reviveRange = 1.5f;
        [SerializeField, Min(0f), Tooltip("Extra range the server allows, because it sees the reviver a little late.")]
        private float _serverRangeSlack = 0.75f;

        [Header("Solo")]
        [SerializeField, Min(0), Tooltip("GDD: a lone player gets one extra life per floor (auto-revive after the hold time).")]
        private int _soloExtraLivesPerFloor = 1;

        [Header("Gravestone")]
        [SerializeField, Min(0f), Tooltip("A gravestone dropped in the air falls at most this far to the ground.")]
        private float _graveSnapDistance = 12f;

        [Header("Ghost")]
        [SerializeField] private Color _ghostColor = new(0.659f, 0.784f, 1f, 0.5f);
        [SerializeField, Min(0f)] private float _ghostSpeed = 7f;
        [SerializeField, Min(0f)] private float _ghostAcceleration = 40f;
        [SerializeField, Min(0f), Tooltip("Ghosts stay this far inside the camera view.")]
        private float _ghostScreenMargin = 0.5f;

        [Header("Ghost poke")]
        [SerializeField, Min(0f)] private float _pokeRange = 1.5f;
        [SerializeField, Min(0f)] private float _pokeCooldown = 0.6f;
        [SerializeField, Min(0f), Tooltip("How long a poke holds an idle or walking boss in place. Attacks are never interrupted.")]
        private float _pokeStagger = 0.25f;

        public float HoldSeconds => _holdSeconds;
        public float ReviveHpFraction => _reviveHpFraction;
        public float ReviveRange => _reviveRange;
        public float ServerRangeSlack => _serverRangeSlack;
        public int SoloExtraLivesPerFloor => _soloExtraLivesPerFloor;
        public float GraveSnapDistance => _graveSnapDistance;
        public Color GhostColor => _ghostColor;
        public float GhostSpeed => _ghostSpeed;
        public float GhostAcceleration => _ghostAcceleration;
        public float GhostScreenMargin => _ghostScreenMargin;
        public float PokeRange => _pokeRange;
        public float PokeCooldown => _pokeCooldown;
        public float PokeStagger => _pokeStagger;
    }
}
