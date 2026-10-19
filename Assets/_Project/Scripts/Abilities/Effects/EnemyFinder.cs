using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>Server: finds the centre of the nearest enemy body (boss, minion, dummy) within a range. Used by homing shots and turrets.</summary>
    public class EnemyFinder
    {
        private readonly Collider2D[] _hits = new Collider2D[8];
        private readonly ContactFilter2D _filter = new()
        {
            useLayerMask = true,
            layerMask = Layers.Mask(Layers.BossIndex),
            useTriggers = false,
        };

        /// <summary>True when an enemy is within <paramref name="range"/> of <paramref name="from"/>; <paramref name="nearest"/> is its centre.</summary>
        public bool TryNearest(Vector2 from, float range, out Vector2 nearest)
        {
            nearest = default;
            var best = float.MaxValue;
            var count = Physics2D.OverlapCircle(from, range, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                var centre = (Vector2)_hits[i].bounds.center;
                var distance = (centre - from).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                nearest = centre;
            }
            return count > 0;
        }
    }
}
