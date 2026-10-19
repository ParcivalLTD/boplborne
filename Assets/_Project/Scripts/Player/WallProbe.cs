using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Looks for a wall right beside a body: solid ground and solid ability objects (a Stone Wall),
    /// never one-way ledges. Used for wall slides and wall jumps.
    /// </summary>
    public class WallProbe
    {
        private readonly Collider2D[] _hits = new Collider2D[4];
        private readonly ContactFilter2D _filter = new()
        {
            useLayerMask = true,
            layerMask = Layers.Mask(Layers.GroundIndex) | Layers.Mask(Layers.AbilityIndex),
            useTriggers = false,
        };

        /// <summary>+1 for a wall on the right, -1 on the left, 0 for none (right wins a tie).</summary>
        public float Probe(Bounds body, Vector2 size)
        {
            if (Touches(body, size, 1f)) return 1f;
            return Touches(body, size, -1f) ? -1f : 0f;
        }

        private bool Touches(Bounds body, Vector2 size, float side)
        {
            var edge = side > 0f ? body.max.x : body.min.x;
            var centre = new Vector2(edge + side * size.x * 0.5f, body.center.y);
            return Physics2D.OverlapBox(centre, size, 0f, _filter, _hits) > 0;
        }
    }
}
