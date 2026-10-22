using GraceRun.Abilities.Effects;
using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// One ability: its card text, tags, rarity, cooldown and the effect it spawns.
    /// One asset per ability under Data/Abilities; every ability must also be listed in the
    /// <see cref="AbilityCatalog"/> so it can be sent over the network.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Ability Data", fileName = "AbilityData")]
    public class AbilityData : ScriptableObject
    {
        [Header("Card")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField, TextArea, Tooltip("The combo line on the draft card.")]
        private string _comboHook;
        [SerializeField] private Sprite _icon;
        [SerializeField] private AbilityTags _tags;
        [SerializeField] private AbilityRarity _rarity;
        [SerializeField, Min(1), Tooltip("First floor this ability can be drafted on.")]
        private int _minFloor = 1;

        [Header("Use")]
        [SerializeField, Min(0f)] private float _cooldown = 6f;
        [SerializeField, Tooltip("Spawned by the server on use (a NetworkObject). Empty = no object.")]
        private AbilityEffect _effectPrefab;
        [SerializeField, Tooltip("Where the effect spawns, relative to the caster; x points the way the caster aims.")]
        private Vector2 _spawnOffset;
        [SerializeField, Min(0f), Tooltip("Seconds until the server despawns the effect. 0 = the effect ends itself.")]
        private float _effectLifetime = 1f;
        [SerializeField, Min(0f), Tooltip("Size of the effect in world units: radius for round effects and blasts; grapple: stops this close to the anchor.")]
        private float _effectRadius = 1f;
        [SerializeField, Tooltip("Optional. Runs on the caster's own PC first (e.g. the grapple); if it finds nothing to do, the ability is not used.")]
        private CasterAction _casterAction;
        [SerializeField, Tooltip("Where the effect appears: at the caster (plus the offset), at the aim point, or high above the aim point (falling).")]
        private AbilitySpawnMode _spawnMode = AbilitySpawnMode.AtCaster;
        [SerializeField, Min(0f), Tooltip("AboveAimPoint: how high above the aim point the effect starts.")]
        private float _dropHeight = 12f;

        [Header("Launch")]
        [SerializeField, Min(1), Tooltip("How many effects one use spawns (arrows in a volley).")]
        private int _count = 1;
        [SerializeField, Tooltip("Degrees between neighbouring effects when Count is above 1.")]
        private float _spread;
        [SerializeField, Tooltip("Degrees above the facing direction the effect is aimed (0 = straight ahead).")]
        private float _launchAngle;
        [SerializeField, Min(0f), Tooltip("Start speed of projectiles, or travel speed of waves, in units per second.")]
        private float _launchSpeed;
        [SerializeField, Min(0f), Tooltip("Projectiles: seconds until they impact on their own (bomb fuse). Grapple: longest pull.")]
        private float _maxFlightTime = 3f;
        [SerializeField, Min(0f), Tooltip("Projectiles: gravity scale while flying (0 = straight line).")]
        private float _gravityScale = 1f;

        [Header("Hit")]
        [SerializeField, Min(0)] private int _damage;
        [SerializeField, Min(0f), Tooltip("Push speed (units per second) given to whatever the effect hits. Grapple: yank speed on a teammate.")]
        private float _knockback;
        [SerializeField, Tooltip("Extra upward push speed on top of the knockback.")]
        private float _knockbackLift;

        [Header("Force")]
        [SerializeField, Min(0f), Tooltip("Pull fields: top pull speed. Grapple: pull speed toward the anchor.")]
        private float _forceSpeed;
        [SerializeField, Min(0f), Tooltip("Pull fields: how fast a body's velocity turns toward the pull (units per second squared).")]
        private float _forceRate;
        [SerializeField, Min(0f), Tooltip("Grapple: how far the hook reaches. Walls: how far down they look for the floor.")]
        private float _range;

        [Header("Extra")]
        [SerializeField, Min(0.01f), Tooltip("Zones, totems and turrets: seconds between ticks. Bounce pads: seconds before the same body bounces again.")]
        private float _tickInterval = 0.5f;
        [SerializeField, Min(0f), Tooltip("Homing / boomerang: how fast the flight turns (units per second squared).")]
        private float _steerRate;
        [SerializeField, Min(0f), Tooltip("Boomerang: seconds of flight before it turns back to the caster.")]
        private float _turnTime = 0.5f;
        [SerializeField, Tooltip("Projectiles: fly through targets, hitting each one once, instead of stopping on the first.")]
        private bool _piercing;
        [SerializeField, Tooltip("Status a hit puts on its target.")]
        private StatusKind _status;
        [SerializeField, Min(0f)] private float _statusDuration;
        [SerializeField, Min(0f), Tooltip("Slow: speed multiplier (0.5 = half speed). Shrink: size multiplier.")]
        private float _statusStrength = 1f;
        [SerializeField, Tooltip("Spawned on impact (cluster bomblets, fire pools) or fired (turret bolts). Has its own AbilityData.")]
        private AbilityEffect _childEffect;
        [SerializeField, Min(0)] private int _childCount = 1;
        [SerializeField, Tooltip("Degrees between neighbouring children, fanned around the launch direction.")]
        private float _childSpread;

        [Header("Look")]
        [SerializeField, Tooltip("Walls: width and height in units. Pull fields: x is the calm radius where the pull eases off. Dash: x is the share of the dash speed kept when it ends.")]
        private Vector2 _size = Vector2.one;
        [SerializeField, Min(0f), Tooltip("Seconds a visual takes: the wall rising, the explosion ring, the grapple rope after a yank.")]
        private float _visualTime = 0.25f;

        [Header("Cast animation")]
        [SerializeField, Tooltip("The body poses the caster shows: wind-up, release, recovery. Empty: no cast pose.")]
        private CastPoseSet _castPose;
        [SerializeField, Min(0f), Tooltip("Seconds between the button press and the effect appearing (the wind-up pose). 0 = at once; keep it under about 0.15 for quick abilities.")]
        private float _castWindup;
        [SerializeField, Min(0f), Tooltip("Seconds the recovery pose holds after the release.")]
        private float _castRecovery = 0.2f;

        [Header("Level 2")]
        [SerializeField, Min(0f)] private float _level2CooldownMultiplier = 0.8f;
        [SerializeField, Min(0f)] private float _level2PowerMultiplier = 1.25f;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public string ComboHook => _comboHook;
        public Sprite Icon => _icon;
        public AbilityTags Tags => _tags;
        public AbilityRarity Rarity => _rarity;
        public int MinFloor => _minFloor;
        public float Cooldown => _cooldown;
        public AbilityEffect EffectPrefab => _effectPrefab;
        public Vector2 SpawnOffset => _spawnOffset;
        public float EffectLifetime => _effectLifetime;
        public float EffectRadius => _effectRadius;
        public CasterAction CasterAction => _casterAction;
        public AbilitySpawnMode SpawnMode => _spawnMode;
        public float DropHeight => _dropHeight;
        public float TickInterval => _tickInterval;
        public float SteerRate => _steerRate;
        public float TurnTime => _turnTime;
        public bool Piercing => _piercing;
        public StatusKind Status => _status;
        public float StatusDuration => _statusDuration;
        public float StatusStrength => _statusStrength;
        public AbilityEffect ChildEffect => _childEffect;
        public int ChildCount => _childCount;
        public float ChildSpread => _childSpread;
        public int Count => _count;
        public float Spread => _spread;
        public float LaunchAngle => _launchAngle;
        public float LaunchSpeed => _launchSpeed;
        public float MaxFlightTime => _maxFlightTime;
        public float GravityScale => _gravityScale;
        public int Damage => _damage;
        public float Knockback => _knockback;
        public float KnockbackLift => _knockbackLift;
        public float ForceSpeed => _forceSpeed;
        public float ForceRate => _forceRate;
        public float Range => _range;
        public Vector2 Size => _size;
        public float VisualTime => _visualTime;
        public CastPoseSet CastPose => _castPose;
        public float CastWindup => _castWindup;
        public float CastRecovery => _castRecovery;
        public float Level2CooldownMultiplier => _level2CooldownMultiplier;
        public float Level2PowerMultiplier => _level2PowerMultiplier;
    }
}
