using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: bends a projectile toward the nearest enemy body within the ability's range,
    /// turning at the ability's steer rate (wisps).
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public class Homing : MonoBehaviour
    {
        private readonly EnemyFinder _enemies = new();
        private Projectile _projectile;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
        }

        private void FixedUpdate()
        {
            if (!_projectile.IsServer || !_projectile.IsSpawned) return;
            var ability = _projectile.Effect.Ability;
            var from = (Vector2)transform.position;
            if (!_enemies.TryNearest(from, ability.Range, out var target)) return;
            var wanted = (target - from).normalized * ability.LaunchSpeed;
            _projectile.ServerSteer(wanted, ability.SteerRate * Time.fixedDeltaTime);
        }
    }
}
