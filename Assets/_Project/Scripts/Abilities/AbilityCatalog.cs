using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Every ability in the game, in a fixed order. The position in this list is the ability's
    /// network id (an asset can't be sent over the network). Lives under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Ability Catalog", fileName = "AbilityCatalog")]
    public class AbilityCatalog : ScriptableObject
    {
        /// <summary>Network id of an empty slot.</summary>
        public const short None = -1;

        [SerializeField, Tooltip("Append new abilities at the end; reordering changes network ids.")]
        private AbilityData[] _abilities;
        [SerializeField, Min(0f), Tooltip("Seconds early the server still accepts a use; covers clock drift between PCs.")]
        private float _cooldownTolerance = 0.15f;

        public int Count => _abilities.Length;
        public float CooldownTolerance => _cooldownTolerance;

        /// <summary>The ability with network id <paramref name="id"/>, or null (empty or unknown).</summary>
        public AbilityData Get(int id) => id >= 0 && id < _abilities.Length ? _abilities[id] : null;

        /// <summary>Network id of <paramref name="ability"/>, or <see cref="None"/>.</summary>
        public short IdOf(AbilityData ability)
        {
            if (ability == null) return None;
            for (short i = 0; i < _abilities.Length; i++)
                if (_abilities[i] == ability) return i;
            return None;
        }
    }
}
