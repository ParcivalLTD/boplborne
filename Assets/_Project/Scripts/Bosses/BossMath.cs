using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>Pure boss numbers: HP scaling by player count (docs/GDD.md) and the phase 2 check.</summary>
    public static class BossMath
    {
        /// <summary>Walk speed multiplier: <paramref name="chaseMultiplier"/> when farther than <paramref name="chaseDistance"/> from the target, else 1.</summary>
        public static float ChaseSpeedMultiplier(float distance, float chaseDistance, float chaseMultiplier) =>
            distance > chaseDistance ? Mathf.Max(1f, chaseMultiplier) : 1f;

        /// <summary>BossHP = BaseHP x (1 + perExtraPlayer x (players - 1)), rounded; at least 1 player counts.</summary>
        public static int ScaledMaxHP(int baseHP, float perExtraPlayer, int players)
        {
            var extra = Mathf.Max(0, players - 1);
            return Mathf.Max(1, Mathf.RoundToInt(baseHP * (1f + perExtraPlayer * extra)));
        }

        /// <summary>True once HP has dropped to or below <paramref name="threshold"/> of max (but the boss is not dead).</summary>
        public static bool IsInPhase2(int current, int max, float threshold) =>
            current > 0 && max > 0 && current <= max * threshold;
    }
}
