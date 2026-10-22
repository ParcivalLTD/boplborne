using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Everything about one boss: name, HP, phases, attack lists and pacing.
    /// Tune the asset in the boss folder under Data/Bosses, never the code.
    /// Attack ids on the network are indices: phase 1 attacks first, then phase 2 attacks.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Data", fileName = "BossData")]
    public class BossData : ScriptableObject, IHealthStats
    {
        [SerializeField] private string _id = "boss";
        [SerializeField] private string _displayName = "Boss";
        [SerializeField, Min(1)] private int _floor = 1;
        [SerializeField] private Color _accentColor = new(0.6f, 0.64f, 0.71f);

        [Header("Health")]
        [SerializeField, Min(1)] private int _baseHP = 600;
        [SerializeField, Min(0f), Tooltip("GDD: BossHP = BaseHP x (1 + this x (players - 1)).")]
        private float _hpScalePerExtraPlayer = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _phase2Threshold = 0.5f;

        [Header("Attacks")]
        [SerializeField] private AttackData[] _phase1Attacks;
        [SerializeField, Tooltip("Empty = phase 2 keeps using the phase 1 attacks.")]
        private AttackData[] _phase2Attacks;

        [Header("Pacing")]
        [SerializeField, Min(0f), Tooltip("Pause between the end of one attack and choosing the next.")]
        private float _idleTime = 0.8f;
        [SerializeField, Min(0f)] private float _targetSwitchInterval = 8f;
        [SerializeField, Min(0f)] private float _walkSpeed = 3f;
        [SerializeField, Min(0f), Tooltip("Farther than this from its target (units), the boss walks faster so a wide arena does not mean a long wait.")]
        private float _chaseDistance = 12f;
        [SerializeField, Min(1f), Tooltip("Walk speed multiplier beyond Chase Distance.")]
        private float _chaseSpeedMultiplier = 2f;
        [SerializeField, Min(0f), Tooltip("The server schedules each attack this many seconds ahead, so clients get it before it starts.")]
        private float _attackStartLead = 0.1f;
        [SerializeField, Min(0f), Tooltip("Seconds at 0 HP before the boss despawns.")]
        private float _despawnDelay = 1.5f;

        [Header("Fight feel")]
        [SerializeField] private BossFeelSettings _feel = new();

        [Header("Escorts")]
        [SerializeField, Tooltip("Minibosses that fight alongside this boss on its floor.")]
        private EscortSpec[] _escorts = System.Array.Empty<EscortSpec>();

        [Header("Prefabs")]
        [SerializeField] private GameObject _prefab;
        [SerializeField, Tooltip("Arena prefab; empty until arenas exist (M4/M6).")]
        private GameObject _arenaPrefab;

        public string Id => _id;
        public string DisplayName => _displayName;
        public int Floor => _floor;
        public Color AccentColor => _accentColor;
        public int MaxHP => _baseHP;
        public float HpScalePerExtraPlayer => _hpScalePerExtraPlayer;
        public float Phase2Threshold => _phase2Threshold;
        public float IdleTime => _idleTime;
        public float TargetSwitchInterval => _targetSwitchInterval;
        public float WalkSpeed => _walkSpeed;
        public float ChaseDistance => _chaseDistance;
        public float ChaseSpeedMultiplier => _chaseSpeedMultiplier;
        public float AttackStartLead => _attackStartLead;
        public float DespawnDelay => _despawnDelay;
        public BossFeelSettings Feel => _feel ??= new BossFeelSettings();
        public EscortSpec[] Escorts => _escorts ?? System.Array.Empty<EscortSpec>();
        public GameObject Prefab => _prefab;
        public GameObject ArenaPrefab => _arenaPrefab;

        private int Phase1Count => _phase1Attacks?.Length ?? 0;
        private bool HasPhase2List => _phase2Attacks != null && _phase2Attacks.Length > 0;

        /// <summary>The attacks the brain may pick from in <paramref name="phase"/> (1 or 2).</summary>
        public AttackData[] AttacksFor(int phase) => phase >= 2 && HasPhase2List ? _phase2Attacks : _phase1Attacks;

        /// <summary>Network id of the attack at <paramref name="index"/> in <see cref="AttacksFor"/>.</summary>
        public int AttackId(int phase, int index) => phase >= 2 && HasPhase2List ? Phase1Count + index : index;

        /// <summary>The attack behind a network id, or null.</summary>
        public AttackData AttackById(int id)
        {
            if (id < 0) return null;
            if (id < Phase1Count) return _phase1Attacks[id];
            id -= Phase1Count;
            return HasPhase2List && id < _phase2Attacks.Length ? _phase2Attacks[id] : null;
        }
    }
}
