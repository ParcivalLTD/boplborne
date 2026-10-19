using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Keeps the effect on its caster (blade ring, dash strike). Every PC follows the caster as it
    /// sees it, so it needs no NetworkTransform. If the caster is gone, the server ends the effect.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class FollowCaster : MonoBehaviour
    {
        private AbilityEffect _effect;
        private Transform _caster;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void FixedUpdate()
        {
            if (!_effect.IsSpawned) return;
            if (_caster == null && !TryFindCaster())
            {
                if (_effect.IsServer) _effect.NetworkObject.Despawn();
                return;
            }
            transform.position = _caster.position;
        }

        private void LateUpdate()
        {
            if (_caster != null) transform.position = _caster.position;
        }

        private bool TryFindCaster()
        {
            var spawned = _effect.NetworkManager.SpawnManager.SpawnedObjects;
            if (!spawned.TryGetValue(_effect.CasterId, out var caster)) return false;
            _caster = caster.transform;
            return true;
        }
    }
}
