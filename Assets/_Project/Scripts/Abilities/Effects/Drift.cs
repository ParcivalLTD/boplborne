using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Moves the effect sideways, the way it was aimed, at the ability's launch speed (tornado).
    /// Every PC moves its own copy from the spawn point, like the Gust wave.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class Drift : MonoBehaviour
    {
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
            var side = AbilityMath.SideOf(_effect.Aim.x);
            transform.position = _origin + new Vector2(side * _effect.Ability.LaunchSpeed * _effect.Age, 0f);
        }
    }
}
