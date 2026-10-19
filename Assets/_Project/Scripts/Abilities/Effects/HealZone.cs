using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: every tick interval, heals every living player inside the ability's radius
    /// (the caster too) by the ability's damage value, scaled by its level (Grace Totem).
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class HealZone : MonoBehaviour
    {
        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly HashSet<Health> _healed = new();
        private readonly ContactFilter2D _filter = new()
        {
            useLayerMask = true,
            layerMask = Layers.Mask(Layers.PlayerIndex),
            useTriggers = false,
        };
        private AbilityEffect _effect;
        private float _nextTick;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void FixedUpdate()
        {
            if (!_effect.IsServer || !_effect.IsSpawned || _effect.Age < _nextTick) return;
            var ability = _effect.Ability;
            _nextTick = _effect.Age + ability.TickInterval;
            var heal = Mathf.RoundToInt(ability.Damage * _effect.PowerMultiplier);

            _healed.Clear();
            var count = Physics2D.OverlapCircle(transform.position, ability.EffectRadius, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                if (!Hurtbox.TryFind(_hits[i], out var hurtbox)) continue;
                var target = hurtbox.Health;
                if (target == null || !target.IsSpawned || target.IsDead || target.Team != Team.Player) continue;
                if (_healed.Add(target)) target.ServerSetCurrent(target.Current + heal);
            }
        }
    }
}
