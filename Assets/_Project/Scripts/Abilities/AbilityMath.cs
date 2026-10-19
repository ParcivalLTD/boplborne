using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Pure aiming and force math for abilities, kept free of Unity objects so it can be tested
    /// in EditMode.
    /// </summary>
    public static class AbilityMath
    {
        /// <summary>
        /// Direction of effect <paramref name="index"/> out of <paramref name="count"/>, fanned
        /// <paramref name="spread"/> degrees apart around <paramref name="launchAngle"/> degrees above
        /// the facing. <paramref name="side"/> is +1 (right) or -1 (left).
        /// </summary>
        public static Vector2 LaunchDirection(float side, float launchAngle, float spread, int index, int count)
        {
            var offset = count > 1 ? spread * (index - (count - 1) * 0.5f) : 0f;
            var radians = (launchAngle + offset) * Mathf.Deg2Rad;
            var sign = side < 0f ? -1f : 1f;
            return new Vector2(Mathf.Cos(radians) * sign, Mathf.Sin(radians));
        }

        /// <summary>The caster's aim: always the facing side, tilted 45° by holding up or down.</summary>
        public static Vector2 CasterAim(float facing, float vertical)
        {
            var side = facing < 0f ? -1f : 1f;
            var y = Mathf.Abs(vertical) > 0.5f ? Mathf.Sign(vertical) : 0f;
            return new Vector2(side, y).normalized;
        }

        /// <summary>
        /// Aim from <paramref name="from"/> toward the mouse cursor. A cursor on top of the caster
        /// falls back to the facing side.
        /// </summary>
        public static Vector2 CursorAim(Vector2 from, Vector2 cursor, float facing)
        {
            var toCursor = cursor - from;
            return toCursor.sqrMagnitude > 0.0001f ? toCursor.normalized : new Vector2(SideOf(facing), 0f);
        }

        /// <summary>A unit aim from any vector the network handed us; zero or NaN becomes straight right.</summary>
        public static Vector2 SafeAim(Vector2 aim)
        {
            if (float.IsNaN(aim.x) || float.IsNaN(aim.y) || aim.sqrMagnitude < 0.0001f) return Vector2.right;
            return aim.normalized;
        }

        /// <summary>+1 when <paramref name="x"/> points right or straight up/down, -1 when it points left.</summary>
        public static float SideOf(float x) => x < 0f ? -1f : 1f;

        /// <summary>Degrees above the horizontal on the aim's own side: -90 (down) to 90 (up).</summary>
        public static float Elevation(Vector2 aim) =>
            Mathf.Asin(Mathf.Clamp(SafeAim(aim).y, -1f, 1f)) * Mathf.Rad2Deg;

        /// <summary>
        /// A spawn offset authored for aiming right, turned up by <paramref name="elevation"/> degrees
        /// and mirrored to <paramref name="side"/>. Elevation 0 just mirrors x, as before cursor aim.
        /// </summary>
        public static Vector2 AimedOffset(Vector2 offset, float side, float elevation)
        {
            var radians = elevation * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            var turned = new Vector2(offset.x * cos - offset.y * sin, offset.x * sin + offset.y * cos);
            return new Vector2(turned.x * SideOf(side), turned.y);
        }

        /// <summary>
        /// Velocity a pull field wants a body to have: toward the centre at <paramref name="speed"/>,
        /// easing to 0 inside <paramref name="calmRadius"/> so bodies settle instead of overshooting.
        /// </summary>
        public static Vector2 PullVelocity(Vector2 body, Vector2 centre, float speed, float calmRadius)
        {
            var toCentre = centre - body;
            var distance = toCentre.magnitude;
            if (distance < 0.0001f) return Vector2.zero;
            var ease = calmRadius > 0f ? Mathf.Clamp01(distance / calmRadius) : 1f;
            return toCentre / distance * (speed * ease);
        }

        /// <summary>
        /// Push velocity away from <paramref name="origin"/>, plus lift. Never points down: a blast
        /// above a body throws it sideways and up instead of into the floor.
        /// </summary>
        public static Vector2 PushAway(Vector2 origin, Vector2 body, float speed, float lift)
        {
            var away = body - origin;
            away.y = Mathf.Max(0f, away.y);
            if (away.sqrMagnitude < 0.0001f) away = Vector2.up;
            return away.normalized * speed + Vector2.up * lift;
        }
    }
}
