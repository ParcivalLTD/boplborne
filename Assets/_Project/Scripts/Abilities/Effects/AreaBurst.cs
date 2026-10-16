using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Bomb: when the projectile impacts, the server hits every living target within the ability's
    /// radius (teammates too, at friendly-fire damage; not the caster) and pushes it away from the
    /// blast, then every PC plays the burst visual.
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public class AreaBurst : NetworkBehaviour
    {
        [SerializeField] private BurstFx _fxPrefab;

        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly HashSet<Health> _hitThisBurst = new();
        private Projectile _projectile;
        private ContactFilter2D _filter;

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
            _projectile.Impacted += OnImpacted;
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.PlayerIndex) | Layers.Mask(Layers.BossIndex),
                useTriggers = false,
            };
        }

        public override void OnDestroy()
        {
            _projectile.Impacted -= OnImpacted;
            base.OnDestroy();
        }

        private void OnImpacted(Vector2 point, Vector2 velocity, Health direct)
        {
            var effect = _projectile.Effect;
            var ability = effect.Ability;
            _hitThisBurst.Clear();

            var count = Physics2D.OverlapCircle(point, ability.EffectRadius, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                if (!_hits[i].TryGetComponent(out Hurtbox hurtbox) || hurtbox.IsInvulnerable) continue;
                var target = hurtbox.Health;
                if (target == null || !target.IsSpawned || target.IsDead || target.NetworkObjectId == effect.CasterId) continue;
                if (!_hitThisBurst.Add(target)) continue;

                var push = AbilityMath.PushAway(point, target.transform.position, ability.Knockback, ability.KnockbackLift);
                target.ReportHit(new DamageInfo(ability.Damage, push, effect.CasterId, Team.Player));
            }
            BurstRpc(point);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void BurstRpc(Vector2 point)
        {
            if (_fxPrefab == null || Application.isBatchMode) return;
            var ability = _projectile.Effect.Ability;
            Instantiate(_fxPrefab, point, Quaternion.identity).Play(ability.EffectRadius, ability.VisualTime);
        }
    }
}
