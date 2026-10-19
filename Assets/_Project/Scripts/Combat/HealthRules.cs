namespace GraceRun.Combat
{
    /// <summary>Pure max-HP rules for <see cref="Health"/>: an optional override on top of the stats asset.</summary>
    public static class HealthRules
    {
        /// <summary>Override value meaning "use the stats asset".</summary>
        public const int NoOverride = 0;

        /// <summary>The override to store for a requested max HP; 0 or less clears it.</summary>
        public static int OverrideFor(int requestedMax) => requestedMax > 0 ? requestedMax : NoOverride;

        /// <summary>Max HP: the override when one is set, otherwise the stats asset's value.</summary>
        public static int Max(int overrideValue, int statsMax) => overrideValue > 0 ? overrideValue : statsMax;
    }
}
