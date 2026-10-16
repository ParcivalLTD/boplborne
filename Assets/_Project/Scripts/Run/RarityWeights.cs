using System;
using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>How likely each rarity is when a draft card is dealt. 0 = never dealt.</summary>
    [Serializable]
    public struct RarityWeights
    {
        [Min(0f)] public float Common;
        [Min(0f)] public float Rare;
        [Min(0f)] public float Legendary;

        public RarityWeights(float common, float rare, float legendary)
        {
            Common = common;
            Rare = rare;
            Legendary = legendary;
        }

        public float For(AbilityRarity rarity) => rarity switch
        {
            AbilityRarity.Common => Common,
            AbilityRarity.Rare => Rare,
            AbilityRarity.Legendary => Legendary,
            _ => 0f,
        };
    }
}
