using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Pure aiming from the move input (keys or stick) for abilities fired without a mouse cursor:
    /// holding up or down tilts the aim, holding left or right only picks the side (the ability's own
    /// launch angle still applies, so a run-and-throw lob stays a lob), and an optional snap onto
    /// the enemy nearest to the aim line.
    /// </summary>
    public static class AbilityAim
    {
        /// <summary>A stick or key axis below this counts as not held.</summary>
        public const float Deadzone = 0.5f;

        /// <summary>
        /// The aim for the move input at the moment of firing. <paramref name="tilted"/> is true when up
        /// or down is held: the aim is then the exact direction (8-way on keys, free on a stick).
        /// Otherwise the aim is horizontal, on the held side or the <paramref name="facing"/> side.
        /// </summary>
        public static Vector2 FromMove(Vector2 move, float facing, out bool tilted)
        {
            tilted = Mathf.Abs(move.y) >= Deadzone;
            if (tilted) return move.normalized;
            var side = Mathf.Abs(move.x) >= Deadzone ? AbilityMath.SideOf(move.x) : AbilityMath.SideOf(facing);
            return new Vector2(side, 0f);
        }

        /// <summary>
        /// Index of the target within <paramref name="coneDegrees"/> of <paramref name="aim"/> and
        /// <paramref name="maxRange"/> of <paramref name="origin"/> that lies closest to the aim line
        /// (the nearer one on a tie), or -1.
        /// </summary>
        public static int PickTarget(Vector2 origin, Vector2 aim, IReadOnlyList<Vector2> targets, float coneDegrees, float maxRange)
        {
            var best = -1;
            var bestAngle = float.MaxValue;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < targets.Count; i++)
            {
                var offset = targets[i] - origin;
                var distance = offset.magnitude;
                if (distance < 0.3f || distance > maxRange) continue;
                var angle = Vector2.Angle(aim, offset);
                if (angle > coneDegrees) continue;
                var better = angle < bestAngle - 0.01f || (Mathf.Abs(angle - bestAngle) <= 0.01f && distance < bestDistance);
                if (!better) continue;
                best = i;
                bestAngle = angle;
                bestDistance = distance;
            }
            return best;
        }
    }
}
