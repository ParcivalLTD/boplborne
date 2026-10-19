using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Core.Framing
{
    /// <summary>
    /// Pure camera math: where a follow camera of a given half-size should sit so the group is
    /// framed, the local player stays on screen, and the view never leaves the arena.
    /// </summary>
    public static class ArenaFraming
    {
        /// <summary>
        /// Centre of the bounding box of <paramref name="focus"/>, pulled so <paramref name="local"/> stays
        /// at least <paramref name="localMargin"/> inside the view, then clamped to <paramref name="bounds"/>.
        /// With no focus points the camera stays on <paramref name="local"/>, or on <paramref name="fallback"/>.
        /// </summary>
        public static Vector2 Solve(IReadOnlyList<Vector2> focus, Vector2? local, Vector2 half, Rect bounds, float localMargin, Vector2 fallback)
        {
            var center = BoxCenter(focus, local ?? fallback);
            if (local.HasValue) center = KeepInside(center, local.Value, half, localMargin);
            return ClampToBounds(center, half, bounds);
        }

        /// <summary>Centre of the box around <paramref name="points"/>; <paramref name="fallback"/> when empty.</summary>
        public static Vector2 BoxCenter(IReadOnlyList<Vector2> points, Vector2 fallback)
        {
            if (points == null || points.Count == 0) return fallback;
            var min = points[0];
            var max = points[0];
            for (var i = 1; i < points.Count; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }
            return (min + max) * 0.5f;
        }

        /// <summary>Moves <paramref name="center"/> the least needed so <paramref name="point"/> is inside the view shrunk by <paramref name="margin"/>.</summary>
        public static Vector2 KeepInside(Vector2 center, Vector2 point, Vector2 half, float margin)
        {
            var reach = new Vector2(Mathf.Max(0f, half.x - margin), Mathf.Max(0f, half.y - margin));
            return new Vector2(
                Mathf.Clamp(center.x, point.x - reach.x, point.x + reach.x),
                Mathf.Clamp(center.y, point.y - reach.y, point.y + reach.y));
        }

        /// <summary>Keeps the whole view inside <paramref name="bounds"/>; an axis the view is wider than is centred on the bounds.</summary>
        public static Vector2 ClampToBounds(Vector2 center, Vector2 half, Rect bounds) =>
            new Vector2(ClampAxis(center.x, half.x, bounds.xMin, bounds.xMax), ClampAxis(center.y, half.y, bounds.yMin, bounds.yMax));

        private static float ClampAxis(float value, float half, float min, float max)
        {
            if (max - min <= half * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min + half, max - half);
        }
    }
}
