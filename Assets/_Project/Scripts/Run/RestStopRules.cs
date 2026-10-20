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
        public const int MaxAbilityLevel = 2;

        /// <summary>True when ability in a slot can be upgraded to Level 2.</summary>
        public static bool CanUpgradeAbility(bool hasAbility, int currentLevel, int currentCurrency, int upgradeCost) =>
            hasAbility && currentLevel == 1 && CurrencyRules.CanAfford(currentCurrency, upgradeCost);

        /// <summary>True when a team reroll can be purchased.</summary>
        public static bool CanBuyReroll(int currentCurrency, int rerollCost) =>
            CurrencyRules.CanAfford(currentCurrency, rerollCost);

        /// <summary>True when a party buff can be purchased.</summary>
        public static bool CanBuyBuff(PartyBuff requestedBuff, PartyBuff currentBuff, int currentCurrency, int buffCost) =>
            requestedBuff != PartyBuff.None && requestedBuff != currentBuff && CurrencyRules.CanAfford(currentCurrency, buffCost);

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
