using System.Collections.Generic;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: finds the live projectiles inside a circle, so area effects can hand them their force
    /// when <c>ComboRules</c> says so. Projectiles fly only on the server, so only the server looks.
    /// </summary>
    public class EffectProjectiles
    {
        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly List<Projectile> _found = new();
        private readonly ContactFilter2D _filter = new()
        {
            useLayerMask = true,
            layerMask = Layers.Mask(Layers.AbilityIndex),
            useTriggers = true,
        };

        /// <summary>The live projectiles within <paramref name="radius"/>; reused, do not keep.</summary>
        public IReadOnlyList<Projectile> Find(Vector2 centre, float radius)
        {
            _found.Clear();
            var count = Physics2D.OverlapCircle(centre, radius, _filter, _hits);
            for (var i = 0; i < count; i++)
                if (Projectile.TryFind(_hits[i].attachedRigidbody, out var projectile) && !_found.Contains(projectile))
                    _found.Add(projectile);
            return _found;
        }
    }
}
