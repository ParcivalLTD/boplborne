using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Pure math helpers for The Brood Mother's attacks: crawler spawn caps, sticky web placement
    /// and overlap checks, arena clamping and hazard fade-out.
    /// </summary>
    public static class BroodMotherMath
    {
        /// <summary>Clamps <paramref name="x"/> to <c>[-halfWidth, halfWidth]</c>.</summary>
        public static float ClampArenaX(float x, float halfWidth)
        {
            var limit = Mathf.Max(0f, halfWidth);
            return Mathf.Clamp(x, -limit, limit);
        }

        /// <summary>How many crawlers to spawn so the total alive never exceeds <paramref name="maxAlive"/>.</summary>
        public static int CrawlersToSpawn(int requested, int currentAlive, int maxAlive)
        {
            if (requested <= 0 || maxAlive <= 0) return 0;
            return Mathf.Clamp(maxAlive - Mathf.Max(0, currentAlive), 0, requested);
        }

        /// <summary>True when <paramref name="playerPosition"/> is inside the sticky web box at <paramref name="webCentre"/>.</summary>
        public static bool IsInWebZone(Vector2 playerPosition, Vector2 webCentre, float radius, float height) =>
            Mathf.Abs(playerPosition.x - webCentre.x) <= radius &&
            Mathf.Abs(playerPosition.y - webCentre.y) <= height;

        /// <summary>
        /// Fills <paramref name="output"/> with clamped floor X positions for sticky web zones: one per
        /// spread-out player (merged within <paramref name="mergeDistance"/>), plus a flanking zone in
        /// solo play so a single player still has to watch their footing.
        /// </summary>
        public static void CollectWebZoneX(
            IReadOnlyList<float> playerX,
            sbyte facing,
            float soloOffset,
            float mergeDistance,
            float halfWidth,
            int maxZones,
            List<float> output)
        {
            output.Clear();
            if (playerX == null || playerX.Count == 0 || maxZones <= 0) return;

            for (var i = 0; i < playerX.Count && output.Count < maxZones; i++)
            {
                var clamped = ClampArenaX(playerX[i], halfWidth);
                if (!IsTooClose(output, clamped, mergeDistance)) output.Add(clamped);
            }

            if (playerX.Count == 1 && output.Count < maxZones && Mathf.Abs(soloOffset) > 0.01f)
            {
                var dir = facing < 0 ? -1f : 1f;
                var second = ClampArenaX(output[0] + dir * soloOffset, halfWidth);
                if (!IsTooClose(output, second, mergeDistance)) output.Add(second);
            }
        }

        /// <summary>Alpha of a hazard zone that fades out over the last <paramref name="fadeOutTime"/> seconds.</summary>
        public static float FadeAlpha(float age, float lifetime, float fadeOutTime, float baseAlpha)
        {
            if (age >= lifetime || lifetime <= 0f) return 0f;
            if (fadeOutTime <= 0f) return baseAlpha;
            var remaining = lifetime - Mathf.Max(0f, age);
            return remaining >= fadeOutTime ? baseAlpha : baseAlpha * (remaining / fadeOutTime);
        }

        private static bool IsTooClose(List<float> existing, float candidate, float minDistance)
        {
            for (var i = 0; i < existing.Count; i++)
                if (Mathf.Abs(existing[i] - candidate) < minDistance) return true;
            return false;
        }
    }
}
