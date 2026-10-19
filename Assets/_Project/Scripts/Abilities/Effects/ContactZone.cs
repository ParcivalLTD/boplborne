using System.Collections.Generic;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Server: every tick interval, hits each target inside the ability's radius for the ability's
    /// damage and pushes it away (spike traps, fire pools, blade rings). Teammates only as an
    /// Area → Teammate combo rule allows. As a mine it waits for the first target, hits everything
    /// in range once, plays the burst and is gone.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class ContactZone : NetworkBehaviour
    {
        [SerializeField, Tooltip("Mine: the first target in range sets it off; it hits once and despawns.")]
        private bool _detonateOnce;
        [SerializeField, Tooltip("Each target is hit at most once for the whole lifetime (a dash strike).")]
        private bool _hitEachTargetOnce;
        [SerializeField, Tooltip("Optional burst visual for a mine going off.")]
        private BurstFx _fxPrefab;

        private readonly EffectTargets _targets = new();
        private readonly HashSet<Health> _alreadyHit = new();
        private AbilityEffect _effect;
        private float _nextTick;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        public override void OnNetworkSpawn()
        {
            _alreadyHit.Clear();
            _nextTick = _hitEachTargetOnce ? 0f : _effect.Ability.TickInterval;
        }

        private void FixedUpdate()
        {
            if (!IsServer || !IsSpawned || _effect.Age < _nextTick) return;
            var ability = _effect.Ability;
            _nextTick = _hitEachTargetOnce ? _effect.Age : _effect.Age + ability.TickInterval;

            var centre = (Vector2)transform.position;
            var found = _targets.Find(_effect, centre, ability.EffectRadius);
            if (found.Count == 0) return;

            var power = _effect.PowerMultiplier;
            var damage = Mathf.RoundToInt(ability.Damage * power);
            for (var i = 0; i < found.Count; i++)
            {
                var target = found[i];
                if (_hitEachTargetOnce && !_alreadyHit.Add(target)) continue;
                var push = AbilityMath.PushAway(centre, target.transform.position, ability.Knockback * power, ability.KnockbackLift * power);
                target.ReportHit(new DamageInfo(damage, push * _targets.Strengths[i], _effect.CasterId, Team.Player));
            }

            if (!_detonateOnce) return;
            BurstRpc(centre);
            NetworkObject.Despawn();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void BurstRpc(Vector2 point)
        {
            if (_fxPrefab == null || Application.isBatchMode) return;
            var ability = _effect.Ability;
            Instantiate(_fxPrefab, point, Quaternion.identity).Play(ability.EffectRadius, ability.VisualTime);
        }
    }
}
