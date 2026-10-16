using System;
using GraceRun.Combat;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// A server-simulated projectile (arrow, bomb): launched along the effect's aim at the ability's
    /// launch speed, falls with gravity, faces its flight. On the server it impacts on the first
    /// living hurtbox that is not the caster, on the ground or on an <see cref="AttackBlocker"/>, or
    /// on its own after the max flight time; then it raises <see cref="Impacted"/> and despawns.
    /// Everyone else sees it through a server-authority NetworkTransform.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect), typeof(Rigidbody2D))]
    public class Projectile : NetworkBehaviour
    {
        private AbilityEffect _effect;
        private Rigidbody2D _body;
        private float _flightTime;
        private bool _done;

        /// <summary>Server only: impact point, velocity at impact, and the target hit directly (null for ground, walls, time-out).</summary>
        public event Action<Vector2, Vector2, Health> Impacted;

        public AbilityEffect Effect => _effect;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
            _body = GetComponent<Rigidbody2D>();
        }

        public override void OnNetworkSpawn()
        {
            _done = false;
            _flightTime = 0f;
            if (!IsServer) return;
            _body.gravityScale = _effect.Ability.GravityScale;
            _body.linearVelocity = _effect.Aim * _effect.Ability.LaunchSpeed;
        }

        private void FixedUpdate()
        {
            if (!IsServer || !IsSpawned || _done) return;
            var velocity = _body.linearVelocity;
            if (velocity.sqrMagnitude > 0.01f) _body.rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            _flightTime += Time.fixedDeltaTime;
            if (_flightTime >= _effect.Ability.MaxFlightTime) Impact(null);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer || !IsSpawned || _done) return;

            var body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out NetworkObject owner) && owner.NetworkObjectId == _effect.CasterId) return;

            if (other.TryGetComponent(out Hurtbox hurtbox))
            {
                var target = hurtbox.Health;
                if (target != null && target.IsSpawned && !target.IsDead && !hurtbox.IsInvulnerable) Impact(target);
                return;
            }
            if (other.isTrigger) return;
            if (other.gameObject.layer == Layers.GroundIndex || other.TryGetComponent(out AttackBlocker _)) Impact(null);
        }

        private void Impact(Health target)
        {
            _done = true;
            Impacted?.Invoke(_body.position, _body.linearVelocity, target);
            NetworkObject.Despawn();
        }
    }
}
