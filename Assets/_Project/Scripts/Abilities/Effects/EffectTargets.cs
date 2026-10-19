using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: finds the living, hittable targets (players, the dummy, bosses) inside a circle,
    /// skipping the caster and teammates that no Area → Teammate combo rule lets the effect reach.
    /// Each found target comes with its force multiplier from <see cref="ComboForce"/>.
    /// </summary>
    public class EffectTargets
    {
        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly List<Health> _found = new();
        private readonly List<float> _strengths = new();
        private readonly ContactFilter2D _filter = new()
        {
            useLayerMask = true,
            layerMask = Layers.Mask(Layers.PlayerIndex) | Layers.Mask(Layers.BossIndex),
            useTriggers = false,
        };

        /// <summary>Force multiplier of the target at the same index in the last <see cref="Find"/>.</summary>
        public IReadOnlyList<float> Strengths => _strengths;

        /// <summary>The targets within <paramref name="radius"/>; reused, do not keep.</summary>
        public IReadOnlyList<Health> Find(AbilityEffect effect, Vector2 centre, float radius)
        {
            _found.Clear();
            _strengths.Clear();
            var count = Physics2D.OverlapCircle(centre, radius, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                if (!Hurtbox.TryFind(_hits[i], out var hurtbox) || hurtbox.IsInvulnerable) continue;
                var target = hurtbox.Health;
                if (target == null || !target.IsSpawned || target.IsDead || target.NetworkObjectId == effect.CasterId) continue;
                if (_found.Contains(target) || !ComboForce.OnTarget(effect, target, out var strength)) continue;
                _found.Add(target);
                _strengths.Add(strength);
            }
            return _found;
        }
    }
}
