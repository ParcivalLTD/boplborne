using GraceRun.Abilities.Effects;
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
        [SerializeField, Min(0f), Tooltip("Size of the effect in world units (radius for round effects).")]
        private float _effectRadius = 1f;

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
        public float Level2CooldownMultiplier => _level2CooldownMultiplier;
        public float Level2PowerMultiplier => _level2PowerMultiplier;
    }
}
