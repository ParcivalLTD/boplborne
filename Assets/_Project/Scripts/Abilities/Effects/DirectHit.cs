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
        }

        private void OnDestroy() => _projectile.Impacted -= OnImpacted;

        private void OnImpacted(Vector2 point, Vector2 velocity, Health target)
        {
            if (target == null) return;
            var ability = _projectile.Effect.Ability;
            var side = velocity.x < 0f ? -1f : 1f;
            var push = new Vector2(side * ability.Knockback, ability.KnockbackLift);
            target.ReportHit(new DamageInfo(ability.Damage, push, _projectile.Effect.CasterId, Team.Player));
        }
    }
}
