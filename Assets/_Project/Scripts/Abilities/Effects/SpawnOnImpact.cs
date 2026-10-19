using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: when the projectile impacts, spawns the ability's child effect (fire pool, mini black
    /// hole, bomblets) the ability's child count times, fanned around straight up. Children belong
    /// to the same caster and keep the level's power.
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public class SpawnOnImpact : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Children start this far above the impact point, so they don't spawn inside the floor.")]
        private float _lift = 0.3f;

        private Projectile _projectile;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
            _projectile.Impacted += OnImpacted;
        }

        private void OnDestroy() => _projectile.Impacted -= OnImpacted;

        private void OnImpacted(Vector2 point, Vector2 velocity, Health target)
        {
            var effect = _projectile.Effect;
            var ability = effect.Ability;
            if (ability.ChildEffect == null) return;
            var position = point + Vector2.up * _lift;
            for (var i = 0; i < ability.ChildCount; i++)
            {
                var direction = AbilityMath.FanDirection(Vector2.up, ability.ChildSpread, i, ability.ChildCount);
                var child = Instantiate(ability.ChildEffect, position, Quaternion.identity);
                child.ServerSetup(effect.CasterId, direction, effect.PowerMultiplier);
                child.NetworkObject.Spawn(true);
            }
        }
    }
}
