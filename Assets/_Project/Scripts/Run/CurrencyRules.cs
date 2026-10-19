namespace GraceRun.Run
{
    /// <summary>
    /// Pure rules for currency awards and purchases (docs/ARCHITECTURE.md, docs/GDD.md).
    /// </summary>
    public static class CurrencyRules
    {
        /// <summary>Calculates the currency awarded when a boss is defeated, including bonuses.</summary>
        public static int CalculateBossReward(int baseReward, bool noDeaths, int bonusNoDeaths, bool fastKill, int bonusFastKill)
        {
            var total = baseReward;
            if (noDeaths) total += bonusNoDeaths;
            if (fastKill) total += bonusFastKill;
            return total > 0 ? total : 0;
        }

        /// <summary>True when <paramref name="current"/> is at least <paramref name="cost"/>.</summary>
        public static bool CanAfford(int current, int cost) => cost >= 0 && current >= cost;

        /// <summary>Spends <paramref name="cost"/> from <paramref name="current"/>, clamped to zero.</summary>
        public static int Spend(int current, int cost)
        {
            if (!CanAfford(current, cost)) return current;
            return current - cost;
        }
    }
}
