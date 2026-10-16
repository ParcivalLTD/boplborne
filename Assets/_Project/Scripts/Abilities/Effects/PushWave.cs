using System.Collections.Generic;
using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Gust: a wave that travels the way the caster faced at the ability's launch speed and pushes
    /// each body it touches once (teammates too, not the caster). Every PC moves its own copy from
    /// the spawn point and pushes only the bodies it simulates.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class PushWave : MonoBehaviour
    {
        private readonly EffectBodies _bodies = new();
        private readonly HashSet<Knockback> _pushed = new();
        private AbilityEffect _effect;
        private Vector2 _origin;
        private bool _hasOrigin;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void FixedUpdate()
        {
            if (!_effect.IsSpawned) return;
            if (!_hasOrigin)
            {
                _origin = transform.position;
                _hasOrigin = true;
            }
            var ability = _effect.Ability;
            var side = _effect.Aim.x < 0f ? -1f : 1f;
            var centre = _origin + new Vector2(side * ability.LaunchSpeed * _effect.Age, 0f);
            transform.position = centre;

            foreach (var body in _bodies.Find(centre, ability.EffectRadius, _effect.CasterId))
                if (_pushed.Add(body)) body.ApplyLocal(new Vector2(side * ability.Knockback, ability.KnockbackLift));
        }
    }
}
