using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Pure helpers and formatting logic for run results (docs/GDD.md, docs/PROMPTS.md 6.5):
    /// formatting run duration, floors reached summary, damage totals and funny friendly-fire stats.
    /// </summary>
    public static class RunResultRules
    {
        /// <summary>Formats duration into mm:ss (or hh:mm:ss if >= 1 hour).</summary>
        public static string FormatTime(float totalSeconds)
        {
            if (totalSeconds < 0f) totalSeconds = 0f;
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            return timeSpan.TotalHours >= 1
                ? $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}"
                : $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        /// <summary>Formats the floors reached/cleared summary banner.</summary>
        public static string FormatFloors(bool isVictory, int floorReached, int totalFloors)
        {
            if (isVictory)
            {
                return totalFloors > 0
                    ? $"All {totalFloors} Floors Cleared!"
                    : "All Floors Cleared!";
            }

            return totalFloors > 0
                ? $"Reached Floor {Mathf.Max(1, floorReached)} of {totalFloors}"
                : $"Reached Floor {Mathf.Max(1, floorReached)}";
        }

        /// <summary>Calculates the sum of damage dealt across all players.</summary>
        public static int CalculateTotalDamage(IReadOnlyList<int> damageBySlot)
        {
            if (damageBySlot == null) return 0;
            var total = 0;
            for (var i = 0; i < damageBySlot.Count; i++)
            {
                if (damageBySlot[i] > 0) total += damageBySlot[i];
            }
            return total;
        }

        /// <summary>Calculates the sum of friendly-fire knockbacks across all players.</summary>
        public static int CalculateTotalKnockbacks(IReadOnlyList<int> knockbacksBySlot)
        {
            if (knockbacksBySlot == null) return 0;
            var total = 0;
            for (var i = 0; i < knockbacksBySlot.Count; i++)
            {
                if (knockbacksBySlot[i] > 0) total += knockbacksBySlot[i];
            }
            return total;
        }

        /// <summary>
        /// Finds the player slot (0..3) with the highest friendly-fire knockbacks,
        /// or -1 if no friendly-fire knockbacks occurred.
        /// </summary>
        public static int FindTopKnockbackCulprit(IReadOnlyList<int> knockbacksBySlot)
        {
            if (knockbacksBySlot == null) return -1;
            var max = 0;
            var bestSlot = -1;
            for (var i = 0; i < knockbacksBySlot.Count; i++)
            {
                if (knockbacksBySlot[i] > max)
                {
                    max = knockbacksBySlot[i];
                    bestSlot = i;
                }
            }
            return max > 0 ? bestSlot : -1;
        }

        /// <summary>
        /// Finds the player slot (0..3) with the highest damage dealt,
        /// or -1 if no damage was dealt.
        /// </summary>
        public static int FindTopDamageDealer(IReadOnlyList<int> damageBySlot)
        {
            if (damageBySlot == null) return -1;
            var max = 0;
            var bestSlot = -1;
            for (var i = 0; i < damageBySlot.Count; i++)
            {
                if (damageBySlot[i] > max)
                {
                    max = damageBySlot[i];
                    bestSlot = i;
                }
            }
            return max > 0 ? bestSlot : -1;
        }
    }
}
