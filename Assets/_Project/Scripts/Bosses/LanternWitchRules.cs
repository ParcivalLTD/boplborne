using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Pure logic and geometric calculations for The Lantern Witch:
    /// fourfold lantern split positions, light radius queries, beam angles,
    /// and arena darkness gloom intensity.
    /// </summary>
    public static class LanternWitchRules
    {
        public const float DefaultLightRadius = 7.5f;
        public const float DefaultDarkAlpha = 0.72f;
        public const float DefaultLitAlpha = 0.05f;

        /// <summary>Computes symmetrical floating lantern positions around an arena center.</summary>
        public static Vector2[] ComputeWispPositions(Vector2 center, int count, float orbitRadius, float angleOffsetDeg = 45f)
        {
            if (count <= 0) return new Vector2[0];
            var positions = new Vector2[count];
            var step = 360f / count;

            for (var i = 0; i < count; i++)
            {
                var angle = (angleOffsetDeg + (i * step)) * Mathf.Deg2Rad;
                positions[i] = center + new Vector2(Mathf.Cos(angle) * orbitRadius, Mathf.Sin(angle) * orbitRadius * 0.45f);
            }

            return positions;
        }

        /// <summary>True when <paramref name="point"/> falls within <paramref name="lightRadius"/> of any light source.</summary>
        public static bool IsInsideLight(Vector2 point, Vector2[] lightCenters, float lightRadius = DefaultLightRadius)
        {
            if (lightCenters == null || lightCenters.Length == 0 || lightRadius <= 0f) return false;
            var radiusSqr = lightRadius * lightRadius;

            for (var i = 0; i < lightCenters.Length; i++)
            {
                if ((point - lightCenters[i]).sqrMagnitude <= radiusSqr)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Calculates the end point of a directional phantom lantern beam.</summary>
        public static Vector2 ComputeBeamEnd(Vector2 origin, float angleDeg, float length)
        {
            var rad = angleDeg * Mathf.Deg2Rad;
            return origin + new Vector2(Mathf.Cos(rad) * length, Mathf.Sin(rad) * length);
        }

        /// <summary>Calculates darkness gloom alpha based on whether a target is illuminated.</summary>
        public static float GloomAlpha(bool isInsideLight, float darkAlpha = DefaultDarkAlpha, float litAlpha = DefaultLitAlpha)
        {
            return isInsideLight ? litAlpha : darkAlpha;
        }
    }
}
