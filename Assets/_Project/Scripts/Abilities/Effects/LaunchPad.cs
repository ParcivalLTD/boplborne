using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Bounce pad: any player (the caster and teammates too) inside the ability's radius is thrown
    /// up at the ability's force speed, keeping their sideways speed. Every PC bounces only the
    /// bodies it simulates, so the bounce is instant for the player who lands on it.
    /// The tick interval stops the same body bouncing twice in a row.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class LaunchPad : MonoBehaviour
    {
        private const ulong NoCaster = ulong.MaxValue;

        private readonly EffectBodies _bodies = new();
        private readonly Dictionary<Knockback, float> _readyAt = new();
        private AbilityEffect _effect;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void FixedUpdate()
        {
            if (!_effect.IsSpawned) return;
            var ability = _effect.Ability;
            var launch = ability.ForceSpeed * _effect.PowerMultiplier;
            foreach (var body in _bodies.Find(transform.position, ability.EffectRadius, NoCaster))
            {
                if (body.gameObject.layer != Layers.PlayerIndex) continue;
                if (_readyAt.TryGetValue(body, out var readyAt) && _effect.Age < readyAt) continue;
                var velocity = body.Velocity;
                if (velocity.y > launch * 0.5f) continue;
                _readyAt[body] = _effect.Age + ability.TickInterval;
                body.ApplyLocal(new Vector2(velocity.x, launch));
            }
        }
    }
}
