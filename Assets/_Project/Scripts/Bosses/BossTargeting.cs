using System.Collections.Generic;

namespace GraceRun.Bosses
{
    /// <summary>Pure target choice: the boss switches to a different living player whenever it can.</summary>
    public static class BossTargeting
    {
        public const ulong NoTarget = ulong.MaxValue;

        /// <summary>
        /// Picks a target among <paramref name="candidates"/> (living players), avoiding
        /// <paramref name="current"/> when anyone else is available. <paramref name="roll"/> is any
        /// non-negative random integer from the server. Returns <see cref="NoTarget"/> when nobody is alive.
        /// </summary>
        public static ulong Choose(ulong current, IReadOnlyList<ulong> candidates, int roll)
        {
            if (candidates.Count == 0) return NoTarget;
            if (candidates.Count == 1) return candidates[0];

            var others = candidates.Count;
            for (var i = 0; i < candidates.Count; i++)
                if (candidates[i] == current) others--;
            if (others == 0) return current;

            var pick = roll % others;
            for (var i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] == current) continue;
                if (pick-- == 0) return candidates[i];
            }
            return candidates[0];
        }
    }
}
