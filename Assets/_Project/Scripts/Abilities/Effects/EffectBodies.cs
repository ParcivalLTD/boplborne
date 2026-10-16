using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Finds the pushable bodies (players, the dummy) inside a circle that this PC simulates: its own
    /// player on a client, server bodies on the server. Forces from ability effects are applied
    /// there, so a pull or push always matches what the body's simulator sees. The caster is skipped.
    /// </summary>
    public class EffectBodies
    {
        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly List<Knockback> _found = new();
        private readonly ContactFilter2D _filter = new()
        {
            useLayerMask = true,
            layerMask = Layers.Mask(Layers.PlayerIndex) | Layers.Mask(Layers.BossIndex),
            useTriggers = false,
        };

        /// <summary>The bodies found by the last <see cref="Find"/>; reused, do not keep.</summary>
        public IReadOnlyList<Knockback> Found => _found;

        /// <summary>Fills <see cref="Found"/> with the local, pushable bodies within <paramref name="radius"/>.</summary>
        public IReadOnlyList<Knockback> Find(Vector2 centre, float radius, ulong casterId)
        {
            _found.Clear();
            var count = Physics2D.OverlapCircle(centre, radius, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                if (!Knockback.TryFind(_hits[i].attachedRigidbody, out var body)) continue;
                if (!body.IsSpawned || !body.IsOwner || body.NetworkObjectId == casterId) continue;
                if (!_found.Contains(body)) _found.Add(body);
            }
            return _found;
        }
    }
}
