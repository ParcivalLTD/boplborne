using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>Where a lightning column is in its life.</summary>
    public enum ColumnPhase : byte { Telegraph, Strike, Fade, Done }

    /// <summary>Where a breakable platform is in its life.</summary>
    public enum PlatformBreakState : byte { Intact, Cracking, Broken }

    /// <summary>
    /// Pure math of The Storm Wyrm: where lightning columns land, which platforms shatter and the
    /// timing of both. No Unity objects, so it is tested in EditMode.
    /// </summary>
    public static class StormWyrmMath
    {
        /// <summary>
        /// Column positions: one under every player (players closer than <paramref name="mergeDistance"/> share one),
        /// then, if <paramref name="flankOffset"/> is above 0, one beside each of those toward the arena centre.
        /// Never more than <paramref name="maxColumns"/>; players come first.
        /// </summary>
        public static void CollectColumnX(
            IReadOnlyList<float> playerX, float mergeDistance, float flankOffset, float halfWidth, int maxColumns, List<float> result)
        {
            result.Clear();
            if (playerX == null || maxColumns <= 0) return;

            var sorted = new List<float>(playerX.Count);
            for (var i = 0; i < playerX.Count; i++) sorted.Add(BroodMotherMath.ClampArenaX(playerX[i], halfWidth));
            sorted.Sort();
            foreach (var x in sorted)
            {
                if (result.Count >= maxColumns) return;
                if (!IsCrowded(result, x, mergeDistance)) result.Add(x);
            }

            if (flankOffset <= 0f) return;
            var playerColumns = result.Count;
            for (var i = 0; i < playerColumns && result.Count < maxColumns; i++)
            {
                var side = result[i] > 0f ? -1f : 1f;
                var x = BroodMotherMath.ClampArenaX(result[i] + side * flankOffset, halfWidth);
                if (!IsCrowded(result, x, mergeDistance)) result.Add(x);
            }
        }

        /// <summary>
        /// Which of <paramref name="count"/> platforms break: about <paramref name="fraction"/> of them, never all of them,
        /// the same picks on every PC for the same <paramref name="seed"/>.
        /// </summary>
        public static void PickBroken(int count, int seed, float fraction, List<int> result)
        {
            result.Clear();
            if (count <= 0) return;
            var take = count == 1 ? 1 : Mathf.Clamp(Mathf.CeilToInt(count * Mathf.Clamp01(fraction)), 1, count - 1);
            var order = new int[count];
            for (var i = 0; i < count; i++) order[i] = i;
            var random = new System.Random(seed);
            for (var i = count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            for (var i = 0; i < take; i++) result.Add(order[i]);
        }

        /// <summary>A platform <paramref name="elapsed"/> seconds after the attack started: cracking, then broken, then whole again.</summary>
        public static PlatformBreakState BreakStateAt(float elapsed, float crackSeconds, float brokenSeconds)
        {
            if (elapsed < 0f) return PlatformBreakState.Intact;
            if (elapsed < crackSeconds) return PlatformBreakState.Cracking;
            return elapsed < crackSeconds + brokenSeconds ? PlatformBreakState.Broken : PlatformBreakState.Intact;
        }

        /// <summary>A lightning column <paramref name="age"/> seconds after it appeared.</summary>
        public static ColumnPhase ColumnPhaseAt(float age, float delay, float strikeSeconds, float fadeSeconds)
        {
            if (age < delay) return ColumnPhase.Telegraph;
            if (age < delay + strikeSeconds) return ColumnPhase.Strike;
            return age < delay + strikeSeconds + fadeSeconds ? ColumnPhase.Fade : ColumnPhase.Done;
        }

        private static bool IsCrowded(List<float> placed, float x, float mergeDistance)
        {
            for (var i = 0; i < placed.Count; i++)
                if (Math.Abs(placed[i] - x) < mergeDistance) return true;
            return false;
        }
    }
}
