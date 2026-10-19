using System.Collections.Generic;
using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Gust: a wave that travels the way the caster faced at the ability's launch speed and pushes
    /// each body it touches once (not the caster; teammates only as <c>ComboRules</c> allows). Every
    /// PC moves its own copy from the spawn point and pushes only the bodies it simulates; the server
    /// also turns projectiles the wave passes (when a projectile → area rule allows it).
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class PushWave : MonoBehaviour
    {
        private readonly EffectBodies _bodies = new();
        private readonly EffectProjectiles _projectiles = new();
        private readonly HashSet<Knockback> _pushed = new();
        private readonly HashSet<Projectile> _redirected = new();
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
            var power = _effect.PowerMultiplier;
            var push = new Vector2(side * ability.Knockback * power, ability.KnockbackLift * power);

            foreach (var body in _bodies.Find(centre, ability.EffectRadius, _effect.CasterId))
                if (!_pushed.Contains(body) && ComboForce.OnBody(_effect, body, out var strength) && _pushed.Add(body))
                    body.ApplyLocal(push * strength);

            if (!_effect.IsServer) return;
            foreach (var projectile in _projectiles.Find(centre, ability.EffectRadius))
                if (!_redirected.Contains(projectile) && ComboForce.OnProjectile(_effect, projectile, out var strength) && _redirected.Add(projectile))
                    projectile.ServerRedirect(push * strength);
        }
    }
}
