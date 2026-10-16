using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Black Hole: for its lifetime, pulls every pushable body within the ability's radius toward
    /// its centre (not the caster; teammates and projectiles only as <c>ComboRules</c> allows). Bodies
    /// are pulled on the PC that simulates them, projectiles on the server.
    /// The ability's Size.x is the calm radius where the pull eases off.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class PullField : MonoBehaviour
    {
        private readonly EffectBodies _bodies = new();
        private readonly EffectProjectiles _projectiles = new();
        private AbilityEffect _effect;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void FixedUpdate()
        {
            if (!_effect.IsSpawned) return;
            var ability = _effect.Ability;
            var centre = (Vector2)transform.position;
            var maxChange = ability.ForceRate * Time.fixedDeltaTime;

            foreach (var body in _bodies.Find(centre, ability.EffectRadius, _effect.CasterId))
            {
                if (!ComboForce.OnBody(_effect, body, out var strength)) continue;
                var target = AbilityMath.PullVelocity(body.transform.position, centre, ability.ForceSpeed * strength, ability.Size.x);
                body.Steer(target, maxChange * strength);
            }

            if (!_effect.IsServer) return;
            foreach (var projectile in _projectiles.Find(centre, ability.EffectRadius))
            {
                if (!ComboForce.OnProjectile(_effect, projectile, out var strength)) continue;
                var target = AbilityMath.PullVelocity(projectile.transform.position, centre, ability.ForceSpeed * strength, ability.Size.x);
                projectile.ServerSteer(target, maxChange * strength);
            }
        }
    }
}
