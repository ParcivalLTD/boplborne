using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Black Hole: for its lifetime, pulls every pushable body within the ability's radius toward
    /// its centre (teammates too, not the caster). Runs on every PC for the bodies that PC simulates.
    /// The ability's Size.x is the calm radius where the pull eases off.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class PullField : MonoBehaviour
    {
        private readonly EffectBodies _bodies = new();
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
                var target = AbilityMath.PullVelocity(body.transform.position, centre, ability.ForceSpeed, ability.Size.x);
                body.Steer(target, maxChange);
            }
        }
    }
}
