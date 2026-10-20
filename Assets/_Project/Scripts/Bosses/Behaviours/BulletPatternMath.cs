using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Pure math for Cuphead-style bullet-hell projectile spreads, rings, and bounce reflections.
    /// </summary>
    public static class BulletPatternMath
    {
        /// <summary>
        /// Returns an array of angles (degrees) distributed across <paramref name="spreadAngleDegrees"/>,
        /// centered at <paramref name="centerAngleDegrees"/>.
        /// </summary>
        public static float[] ComputeSpreadAngles(int count, float spreadAngleDegrees, float centerAngleDegrees)
        {
            if (count <= 0) return new float[0];
            if (count == 1) return new[] { centerAngleDegrees };

            var angles = new float[count];
            var step = spreadAngleDegrees / (count - 1);
            var start = centerAngleDegrees - (spreadAngleDegrees * 0.5f);
            for (var i = 0; i < count; i++)
            {
                angles[i] = start + (step * i);
            }
            return angles;
        }

        /// <summary>
        /// Returns an array of angles (degrees) evenly dividing 360 degrees into <paramref name="count"/> spokes.
        /// </summary>
        public static float[] ComputeRingAngles(int count, float startAngleDegrees = 0f)
        {
            if (count <= 0) return new float[0];
            var angles = new float[count];
            var step = 360f / count;
            for (var i = 0; i < count; i++)
            {
                angles[i] = (startAngleDegrees + (step * i)) % 360f;
            }
            return angles;
        }

        /// <summary>
        /// Converts an angle in degrees and a speed into a 2D velocity vector.
        /// </summary>
        public static Vector2 AngleToVelocity(float angleDegrees, float speed)
        {
            var rad = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * speed;
        }

        /// <summary>
        /// Mirrors an angle based on horizontal facing (+1 = right, -1 = left).
        /// </summary>
        public static float FacingAngle(sbyte facing, float angleOffsetDegrees)
        {
            if (facing < 0)
            {
                var mirrored = 180f - angleOffsetDegrees;
                while (mirrored > 180f) mirrored -= 360f;
                while (mirrored < -180f) mirrored += 360f;
                return mirrored;
            }
            return angleOffsetDegrees;
        }

        /// <summary>
        /// Reflects a velocity vector against a collision normal with optional bounciness.
        /// </summary>
        public static Vector2 Reflect(Vector2 velocity, Vector2 normal, float bounciness = 1f)
        {
            if (normal == Vector2.zero) return -velocity;
            var norm = normal.normalized;
            var dot = Vector2.Dot(velocity, norm);
            return (velocity - (2f * dot * norm)) * Mathf.Max(0f, bounciness);
        }
    }
}
