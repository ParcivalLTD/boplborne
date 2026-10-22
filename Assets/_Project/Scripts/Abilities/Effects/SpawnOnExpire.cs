using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: when the effect runs out its lifetime, spawns the ability's child effect where it stood
    /// (a black hole collapsing into a blast, a tornado's last gust). The child belongs to the same caster
    /// and keeps the level's power; the child's own ability data holds its damage and radius.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class SpawnOnExpire : MonoBehaviour
    {
        private AbilityEffect _effect;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
            _effect.Expired += OnExpired;
        }

        private void OnDestroy()
        {
            if (_effect != null) _effect.Expired -= OnExpired;
        }

        private void OnExpired(AbilityEffect effect)
        {
            var child = effect.Ability.ChildEffect;
            if (child == null) return;
            var spawned = Instantiate(child, transform.position, Quaternion.identity);
            spawned.ServerSetup(effect.CasterId, Vector2.up, effect.PowerMultiplier);
            spawned.NetworkObject.Spawn(true);
        }
    }
}
