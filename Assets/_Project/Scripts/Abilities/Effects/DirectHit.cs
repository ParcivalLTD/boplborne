using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: a projectile that hits a target deals the ability's damage and pushes it along the
    /// flight direction (friendly-fire rules apply through the damage pipeline).
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public class DirectHit : MonoBehaviour
    {
        private Projectile _projectile;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
            _projectile.Impacted += OnImpacted;
            _projectile.PiercedThrough += OnImpacted;
        }

        private void OnDestroy()
        {
            _projectile.Impacted -= OnImpacted;
            _projectile.PiercedThrough -= OnImpacted;
        }

        private void OnImpacted(Vector2 point, Vector2 velocity, Health target)
        {
            if (target == null) return;
            var effect = _projectile.Effect;
            var ability = effect.Ability;
            var power = effect.PowerMultiplier;
            var side = velocity.x < 0f ? -1f : 1f;
            var damage = Mathf.RoundToInt(ability.Damage * power);
            var push = new Vector2(side * ability.Knockback * power, ability.KnockbackLift * power);
            target.ReportHit(new DamageInfo(damage, push, effect.CasterId, Team.Player));
        }
    }
}
