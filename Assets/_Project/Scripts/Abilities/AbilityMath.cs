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
