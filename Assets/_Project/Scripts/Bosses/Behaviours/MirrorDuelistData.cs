using GraceRun.Abilities;
using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Tunable values of The Mirror Duelist: which ability catalog it reads, the dash of its echo and its
    /// phase 2 reflections. Lives in Data/Bosses/MirrorDuelist/MirrorDuelistData.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Mirror Duelist Data", fileName = "MirrorDuelistData")]
    public class MirrorDuelistData : ScriptableObject
    {
        [Header("Memory")]
        [SerializeField, Tooltip("Needed to tell which ability a hit came from.")]
        private AbilityCatalog _catalog;
        [SerializeField] private Color _slashColour = new(0.93f, 0.9f, 0.96f, 1f);
        [SerializeField] private Color _shardColour = new(0.55f, 0.85f, 1f, 1f);
        [SerializeField] private Color _burstColour = new(1f, 0.75f, 0.4f, 1f);
        [SerializeField] private Color _dashColour = new(0.5f, 1f, 0.7f, 1f);

        [Header("Memory icons")]
        [SerializeField, Tooltip("Glyph shown above the Duelist for each remembered kind (slash, shard, burst, dash).")]
        private Sprite _slashIcon;
        [SerializeField] private Sprite _shardIcon;
        [SerializeField] private Sprite _burstIcon;
        [SerializeField] private Sprite _dashIcon;

        [Header("Echo")]
        [SerializeField, Min(1f)] private float _dashSpeed = 22f;
        [SerializeField, Min(0.05f)] private float _dashTime = 0.25f;
        [SerializeField, Tooltip("Shard height above the floor: lowest and highest.")]
        private Vector2 _shardHeight = new(0.8f, 3.5f);
        [SerializeField, Tooltip("Where a shard or wave leaves the Duelist, in front of it.")]
        private float _muzzleOffset = 1.8f;

        [Header("Phase 2 reflections")]
        [SerializeField, Range(0.1f, 1f), Tooltip("Reflections hit for this share of the attack's damage.")]
        private float _reflectionDamageScale = 0.6f;
        [SerializeField, Tooltip("Seconds after the Duelist's own strike that reflection 1 and 2 strike.")]
        private Vector2 _reflectionDelays = new(0.55f, 0.95f);
        [SerializeField, Range(0f, 1f)] private float _reflectionAlpha = 0.5f;
        [SerializeField, Min(0f)] private float _reflectionFade = 0.25f;
        [SerializeField, Min(0f), Tooltip("Reflections never stand closer than this; the second is pushed away.")]
        private float _reflectionMinGap = 3f;
        [SerializeField, Min(0f), Tooltip("Seconds the squash-and-flash lasts when a reflection arms its strike.")]
        private float _reflectionFlashTime = 0.14f;
        [SerializeField, Range(0f, 0.5f), Tooltip("How much a reflection squashes (wider, shorter) when it strikes.")]
        private float _reflectionSquash = 0.18f;

        [Header("Sounds (local only, empty = silent)")]
        [SerializeField, Tooltip("A pair of reflections appears (phase 2 wind-up).")] private AudioCue _reflectionAppearCue;
        [SerializeField, Tooltip("A reflection's strike goes live.")] private AudioCue _reflectionStrikeCue;
        [SerializeField, Tooltip("The remembered ability kind changed.")] private AudioCue _memoryChangeCue;

        public AudioCue ReflectionAppearCue => _reflectionAppearCue;
        public AudioCue ReflectionStrikeCue => _reflectionStrikeCue;
        public AudioCue MemoryChangeCue => _memoryChangeCue;
        public AbilityCatalog Catalog => _catalog;
        public float DashSpeed => _dashSpeed;
        public float DashTime => _dashTime;
        public Vector2 ShardHeight => _shardHeight;
        public float MuzzleOffset => _muzzleOffset;
        public float ReflectionDamageScale => _reflectionDamageScale;
        public Vector2 ReflectionDelays => _reflectionDelays;
        public float ReflectionAlpha => _reflectionAlpha;
        public float ReflectionFade => _reflectionFade;
        public float ReflectionMinGap => _reflectionMinGap;
        public float ReflectionFlashTime => _reflectionFlashTime;
        public float ReflectionSquash => _reflectionSquash;

        /// <summary>The colour the Duelist's mirror shows while it holds <paramref name="kind"/>.</summary>
        public Color ColourOf(MirrorKind kind) => kind switch
        {
            MirrorKind.Shard => _shardColour,
            MirrorKind.Burst => _burstColour,
            MirrorKind.Dash => _dashColour,
            _ => _slashColour,
        };

        /// <summary>The glyph that shows the Duelist's remembered <paramref name="kind"/> besides the mirror tint.</summary>
        public Sprite IconOf(MirrorKind kind) => kind switch
        {
            MirrorKind.Shard => _shardIcon,
            MirrorKind.Burst => _burstIcon,
            MirrorKind.Dash => _dashIcon,
            _ => _slashIcon,
        };
    }
}
