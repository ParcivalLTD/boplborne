using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Pure geometry for the mouse slice: when a held press becomes a drag, which part of a cursor
    /// segment is inside the player's reach, and the push along the cut.
    /// </summary>
    public static class SliceMath
    {
        /// <summary>True once the cursor has moved at least <paramref name="startDistance"/> from where the press began.</summary>
        public static bool IsDrag(Vector2 pressPoint, Vector2 cursor, float startDistance) =>
            (cursor - pressPoint).sqrMagnitude >= startDistance * startDistance;

        /// <summary>
        /// Clips the segment <paramref name="a"/>–<paramref name="b"/> to the circle around
        /// <paramref name="centre"/>. False when no part of it is inside.
        /// </summary>
        public static bool ClipToCircle(Vector2 a, Vector2 b, Vector2 centre, float radius, out Vector2 clippedA, out Vector2 clippedB)
        {
            clippedA = a;
            clippedB = b;
            var d = b - a;
            var f = a - centre;
            var dd = Vector2.Dot(d, d);
            var r2 = radius * radius;
            if (dd < 0.000001f) return f.sqrMagnitude <= r2;

            var bTerm = 2f * Vector2.Dot(f, d);
            var c = Vector2.Dot(f, f) - r2;
            var discriminant = bTerm * bTerm - 4f * dd * c;
            if (discriminant < 0f) return false;

            var root = Mathf.Sqrt(discriminant);
            var t0 = Mathf.Max(0f, (-bTerm - root) / (2f * dd));
            var t1 = Mathf.Min(1f, (-bTerm + root) / (2f * dd));
            if (t0 > t1) return false;

            clippedA = a + d * t0;
            clippedB = a + d * t1;
            return true;
        }

        /// <summary>Push for a target cut by a stroke moving along <paramref name="cut"/>: along the cut, plus lift.</summary>
        public static Vector2 Knockback(Vector2 cut, float speed, float lift)
        {
            var along = cut.sqrMagnitude > 0.000001f ? cut.normalized : Vector2.zero;
            return along * speed + Vector2.up * lift;
        }
    }
}
