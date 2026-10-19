using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: every tick interval, fires the ability's child effect (a bolt) at the nearest enemy
    /// body within the ability's range (Sentry). Bolts belong to the caster and start at the ability's spawn offset above the base.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class Turret : MonoBehaviour
    {
        private readonly EnemyFinder _enemies = new();
        private AbilityEffect _effect;
        private float _nextShot;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void FixedUpdate()
        {
            if (!_effect.IsServer || !_effect.IsSpawned || _effect.Age < _nextShot) return;
            var ability = _effect.Ability;
            if (ability.ChildEffect == null) return;
            var muzzle = (Vector2)transform.position + ability.SpawnOffset;
            if (!_enemies.TryNearest(muzzle, ability.Range, out var target)) return;

            _nextShot = _effect.Age + ability.TickInterval;
            var bolt = Instantiate(ability.ChildEffect, muzzle, Quaternion.identity);
            bolt.ServerSetup(_effect.CasterId, AbilityMath.SafeAim(target - muzzle), _effect.PowerMultiplier);
            bolt.NetworkObject.Spawn(true);
        }
    }
}
