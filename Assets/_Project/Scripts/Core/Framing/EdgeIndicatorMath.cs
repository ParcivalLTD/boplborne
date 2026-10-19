using UnityEngine;

namespace GraceRun.Core.Framing
{
    /// <summary>Pure placement of an off-screen marker on the edge of the camera view.</summary>
    public static class EdgeIndicatorMath
    {
        /// <summary>True when <paramref name="world"/> is inside the view shrunk by <paramref name="hideMargin"/>.</summary>
        public static bool IsOnScreen(Vector2 world, Vector2 cameraCenter, Vector2 half, float hideMargin)
        {
            var d = world - cameraCenter;
            return Mathf.Abs(d.x) <= half.x - hideMargin && Mathf.Abs(d.y) <= half.y - hideMargin;
        }

        /// <summary>
        /// Off-screen target: returns false when it is on screen. Otherwise gives the point on the
        /// view rectangle (shrunk by <paramref name="inset"/>) on the line from the camera centre to the
        /// target, and the angle in degrees the marker points (0 = right, 90 = up).
        /// </summary>
        public static bool TryPlace(Vector2 world, Vector2 cameraCenter, Vector2 half, float inset, float hideMargin, out Vector2 edgePoint, out float angle)
        {
            edgePoint = world;
            angle = 0f;
            if (IsOnScreen(world, cameraCenter, half, hideMargin)) return false;

            var d = world - cameraCenter;
            var reach = new Vector2(Mathf.Max(0.01f, half.x - inset), Mathf.Max(0.01f, half.y - inset));
            var scale = Mathf.Min(
                Mathf.Abs(d.x) > 0.0001f ? reach.x / Mathf.Abs(d.x) : float.MaxValue,
                Mathf.Abs(d.y) > 0.0001f ? reach.y / Mathf.Abs(d.y) : float.MaxValue);
            edgePoint = cameraCenter + d * Mathf.Min(1f, scale);
            angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            return true;
        }
    }
}
