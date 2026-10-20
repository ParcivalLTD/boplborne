using System;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Pure logic and calculations for The Crowned Hollow (Final Boss):
    /// ability mimicry selection, Cataclysm ring bullet spreads,
    /// encounter HP scaling, and punish window timing.
    /// </summary>
    public static class CrownedHollowRules
    {
        public const int DefaultFallbackAbilityId = 2; // Black Hole / Void Singularity

        /// <summary>Deterministically selects an ability ID from players' equipped loadouts, or returns <paramref name="fallbackId"/>.</summary>
        public static int SelectMimickedAbility(int[] availableAbilityIds, int seed, int fallbackId = DefaultFallbackAbilityId)
        {
            if (availableAbilityIds == null || availableAbilityIds.Length == 0) return fallbackId;

            // Count valid non-zero ability IDs
            var validCount = 0;
            for (var i = 0; i < availableAbilityIds.Length; i++)
            {
                if (availableAbilityIds[i] > 0) validCount++;
            }

            if (validCount == 0) return fallbackId;

            var valid = new int[validCount];
            var idx = 0;
            for (var i = 0; i < availableAbilityIds.Length; i++)
            {
                if (availableAbilityIds[i] > 0) valid[idx++] = availableAbilityIds[i];
            }

            var rng = new System.Random(seed);
            return valid[rng.Next(0, valid.Length)];
        }

        /// <summary>Computes radial projectile angles for a dual-ring Cataclysm supernova.</summary>
        public static float[] ComputeCataclysmRingAngles(int count, float phaseAngleDeg)
        {
            if (count <= 0) return new float[0];
            var angles = new float[count];
            var step = 360f / count;

            for (var i = 0; i < count; i++)
            {
                angles[i] = (phaseAngleDeg + (i * step)) % 360f;
            }

            return angles;
        }

        /// <summary>Calculates scaled final boss HP for <paramref name="playerCount"/> participants.</summary>
        public static int ScaleEncounterHP(int baseHP, int playerCount, float scalePerPlayer = 0.6f)
        {
            if (baseHP <= 0) return 0;
            var extra = Mathf.Max(0, playerCount - 1);
            return Mathf.RoundToInt(baseHP * (1f + (scalePerPlayer * extra)));
        }

        /// <summary>True when <paramref name="elapsedSinceActive"/> is within the attack's punish window.</summary>
        public static bool IsStaggerPunishActive(float recoveryTime, float punishWindow, float elapsedSinceActive)
        {
            if (elapsedSinceActive < 0f) return false;
            var window = Mathf.Min(punishWindow, recoveryTime);
            return elapsedSinceActive < window;
        }
    }
}
