using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>Server: a projectile that hits a target puts the ability's status on it (frost slows, the shrink ray shrinks).</summary>
    [RequireComponent(typeof(Projectile))]
    public class StatusOnHit : MonoBehaviour
    {
        private Projectile _projectile;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
            _projectile.Impacted += OnHit;
            _projectile.PiercedThrough += OnHit;
        }

        private void OnDestroy()
        {
            _projectile.Impacted -= OnHit;
            _projectile.PiercedThrough -= OnHit;
        }

        private void OnHit(Vector2 point, Vector2 velocity, Health target)
        {
            if (target == null) return;
            var effect = _projectile.Effect;
            if (DamageService.IsIgnored(target, new DamageInfo(0, Vector2.zero, effect.CasterId, Team.Player))) return;
            var ability = effect.Ability;
            if (StatusEffects.TryFind(target.NetworkObjectId, out var status))
                status.ServerApply(ability.Status, ability.StatusDuration, ability.StatusStrength);
        }
    }
}
