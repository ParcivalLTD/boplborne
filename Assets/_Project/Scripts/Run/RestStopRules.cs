using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Pure validation rules for rest stop purchases: rerolls, ability upgrades, and party buffs.
    /// Also computes buffed player damage during boss encounters.
    /// </summary>
    public static class RestStopRules
    {
        public const int MaxAbilityLevel = 3;

        /// <summary>True when the ability in a slot can go up a level (to level 2 or 3) and the group can pay <paramref name="upgradeCost"/>.</summary>
        public static bool CanUpgradeAbility(bool hasAbility, int currentLevel, int currentCurrency, int upgradeCost) =>
            hasAbility && currentLevel >= 1 && currentLevel < MaxAbilityLevel && CurrencyRules.CanAfford(currentCurrency, upgradeCost);

        /// <summary>Price of the next level: the base cost times the level the ability is at now (100 for level 2, 200 for level 3).</summary>
        public static int UpgradeCost(int baseCost, int currentLevel) => baseCost * Mathf.Max(1, currentLevel);

        /// <summary>True when a team reroll can be purchased.</summary>
        public static bool CanBuyReroll(int currentCurrency, int rerollCost) =>
            CurrencyRules.CanAfford(currentCurrency, rerollCost);

        /// <summary>True when a party buff can be purchased.</summary>
        public static bool CanBuyBuff(PartyBuff requestedBuff, PartyBuff currentBuff, int currentCurrency, int buffCost) =>
            requestedBuff != PartyBuff.None && requestedBuff != currentBuff && CurrencyRules.CanAfford(currentCurrency, buffCost);

        /// <summary>True when one more <paramref name="requestedBuff"/> can be bought: under the stack cap and affordable at <paramref name="cost"/>.</summary>
        public static bool CanBuyBuff(PartyBuff requestedBuff, BuffStacks owned, int maxStacks, int currentCurrency, int cost) =>
            requestedBuff != PartyBuff.None && owned.Of(requestedBuff) < maxStacks && CurrencyRules.CanAfford(currentCurrency, cost);

        /// <summary>Applies the active party buff's damage multiplier to player hits on enemies; teammates and bosses are unaffected.</summary>
        public static int BuffedDamage(int baseDamage, bool isFriendly, Team attackerTeam, float buffDamageMultiplier)
        {
            if (!isFriendly && attackerTeam == Team.Player && buffDamageMultiplier != 1f)
            {
                return Mathf.RoundToInt(baseDamage * buffDamageMultiplier);
            }
            return baseDamage;
        }
    }
}
