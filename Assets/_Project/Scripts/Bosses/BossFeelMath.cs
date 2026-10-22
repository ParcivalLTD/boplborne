using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>Pure rules behind the boss fight feel (no Unity objects, covered by EditMode tests).</summary>
    public static class BossFeelMath
    {
        /// <summary>Idle pause scale from the player count: 1 at one player, shrinking per extra player, never below half.</summary>
        public static float IdleScale(int players, float shrinkPerExtraPlayer) =>
            Mathf.Clamp(1f - shrinkPerExtraPlayer * Mathf.Max(0, players - 1), 0.5f, 1f);

        /// <summary>Chance to skip the idle pause: 0 up to 2 players, then per extra player.</summary>
        public static float PressureChance(int players, float perPlayer) =>
            Mathf.Clamp01(Mathf.Max(0, players - 2) * perPlayer);

        /// <summary>A pool (poise, interrupt damage) scaled like boss HP.</summary>
        public static float ScaledPool(float basePool, float perExtraPlayer, int players) =>
            basePool * (1f + perExtraPlayer * Mathf.Max(0, players - 1));

        /// <summary>True while a hit counts as hitting the weak point: inside the punish window plus the grace.</summary>
        public static bool InWeakPointWindow(bool recovering, float sinceRecovery, float punishWindow, float grace) =>
            recovering && sinceRecovery >= 0f && sinceRecovery <= punishWindow + grace;

        /// <summary>The variant for a spawn: <paramref name="roll01"/> against the chance, then <paramref name="pick"/> chooses which.</summary>
        public static BossVariant PickVariant(float chance, double roll01, int pick)
        {
            if (roll01 >= chance) return BossVariant.None;
            return (BossVariant)(1 + Mathf.Abs(pick) % 4);
        }

        /// <summary>Damage multiplier for a variant's attacks.</summary>
        public static float DamageScale(BossVariant variant) => variant == BossVariant.Brutal ? 1.25f : 1f;

        /// <summary>Boss HP multiplier for a variant.</summary>
        public static float HpScale(BossVariant variant) => variant == BossVariant.Tough ? 1.25f : 1f;

        /// <summary>Idle pause multiplier for a variant.</summary>
        public static float IdleScale(BossVariant variant) =>
            variant == BossVariant.Swift ? 0.75f : variant == BossVariant.Relentless ? 0.85f : 1f;

        /// <summary>Walk speed multiplier for a variant.</summary>
        public static float SpeedScale(BossVariant variant) => variant == BossVariant.Swift ? 1.2f : 1f;
    }
}
