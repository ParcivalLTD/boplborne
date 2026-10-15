using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>Pure revive rules: hold progress, revive HP and the solo extra life.</summary>
    public static class ReviveRules
    {
        /// <summary>Start time meaning "nobody is reviving".</summary>
        public const double NotReviving = -1d;

        /// <summary>0..1 progress of a revive that started at <paramref name="start"/>; 0 when none runs.</summary>
        public static float Progress(double start, double now, float holdSeconds)
        {
            if (start < 0d) return 0f;
            if (holdSeconds <= 0f) return 1f;
            return Mathf.Clamp01((float)((now - start) / holdSeconds));
        }

        /// <summary>HP after a revive: a share of max HP, rounded, at least 1 and at most max.</summary>
        public static int ReviveHp(int maxHp, float fraction) =>
            Mathf.Clamp(Mathf.RoundToInt(maxHp * fraction), 1, Mathf.Max(1, maxHp));

        /// <summary>True when a player who just went down uses a solo extra life: they are alone and have one left.</summary>
        public static bool UsesSoloLife(int connectedPlayers, int livesUsed, int livesPerFloor) =>
            connectedPlayers == 1 && livesUsed < livesPerFloor;

        /// <summary>True when <paramref name="a"/> and <paramref name="b"/> are within <paramref name="range"/>.</summary>
        public static bool InRange(Vector2 a, Vector2 b, float range) => (a - b).sqrMagnitude <= range * range;
    }
}
