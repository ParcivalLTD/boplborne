using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: after the ability's turn time, steers the projectile back to its caster at the
    /// ability's steer rate, and ends it quietly once it is back in the caster's hand.
    /// The ability's effect radius is how close counts as caught. Pair with a piercing ability so
    /// it cuts on the way out and on the way back.
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public class Boomerang : MonoBehaviour
    {
        private Projectile _projectile;
        private Transform _caster;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
        }

        private void FixedUpdate()
        {
            if (!_projectile.IsServer || !_projectile.IsSpawned) return;
            var effect = _projectile.Effect;
            if (effect.Age < effect.Ability.TurnTime) return;
            if (_caster == null)
            {
                if (!effect.NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(effect.CasterId, out var caster)) return;
                _caster = caster.transform;
            }

            var toCaster = (Vector2)_caster.position - (Vector2)transform.position;
            if (toCaster.magnitude <= effect.Ability.EffectRadius)
            {
                _projectile.ServerFinish();
                return;
            }
            var ability = effect.Ability;
            _projectile.ServerSteer(toCaster.normalized * ability.LaunchSpeed, ability.SteerRate * Time.fixedDeltaTime);
        }
    }
}
