using System.Collections.Generic;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Pure attack choice for the boss brain: a weighted pick among attacks that are off cooldown
    /// and in range of the target.
    /// </summary>
    public static class AttackPicker
    {
        /// <summary>
        /// Index of the chosen attack, or -1 when none fits. <paramref name="roll01"/> is a random
        /// number in [0, 1) from the server's random source.
        /// </summary>
        public static int Pick(IReadOnlyList<AttackOption> options, IReadOnlyList<bool> ready, float distance, double roll01)
        {
            var total = 0d;
            for (var i = 0; i < options.Count; i++)
                if (Usable(options, ready, i, distance)) total += options[i].Weight;
            if (total <= 0d) return -1;

            var mark = roll01 * total;
            var last = -1;
            for (var i = 0; i < options.Count; i++)
            {
                if (!Usable(options, ready, i, distance)) continue;
                last = i;
                mark -= options[i].Weight;
                if (mark < 0d) return i;
            }
            return last;
        }

        /// <summary>True when some attack is off cooldown but the target is farther than all of them reach: walk closer.</summary>
        public static bool IsOutOfReach(IReadOnlyList<AttackOption> options, IReadOnlyList<bool> ready, float distance)
        {
            var anyReady = false;
            for (var i = 0; i < options.Count; i++)
            {
                if (!ready[i] || options[i].Weight <= 0f) continue;
                anyReady = true;
                if (distance <= options[i].MaxRange) return false;
            }
            return anyReady;
        }

        private static bool Usable(IReadOnlyList<AttackOption> options, IReadOnlyList<bool> ready, int i, float distance) =>
            ready[i] && options[i].Weight > 0f && options[i].InRange(distance);
    }
}
