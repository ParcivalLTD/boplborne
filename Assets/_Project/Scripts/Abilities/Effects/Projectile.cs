using System;
using System.Collections.Generic;
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
    /// A blocker that is a tagged effect (a summon) is asked through <c>ComboRules</c> first: the
    /// projectile may ricochet off it or be absorbed. Area effects steer it through
    /// <see cref="ServerSteer"/> and <see cref="ServerRedirect"/>.
    /// Everyone else sees it through a server-authority NetworkTransform.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect), typeof(Rigidbody2D))]
    public class Projectile : NetworkBehaviour
    {
        private static readonly Dictionary<Rigidbody2D, Projectile> ByBody = new();

        private AbilityEffect _effect;
        private Rigidbody2D _body;
        private float _flightTime;
        private int _bounces;
        private readonly HashSet<Health> _pierced = new();
        private bool _done;

        /// <summary>Server only: impact point, velocity at impact, and the target hit directly (null for ground, walls, time-out).</summary>
        public event Action<Vector2, Vector2, Health> Impacted;

        /// <summary>Server only, piercing projectiles: a target was hit and the projectile flies on (same arguments as <see cref="Impacted"/>).</summary>
        public event Action<Vector2, Vector2, Health> PiercedThrough;

        public AbilityEffect Effect => _effect;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
            _body = GetComponent<Rigidbody2D>();
            ByBody[_body] = this;
        }

        public override void OnDestroy()
        {
            ByBody.Remove(_body);
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            _done = false;
            _flightTime = 0f;
            _bounces = 0;
            _pierced.Clear();
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
                if (target == null || !target.IsSpawned || target.IsDead || hurtbox.IsInvulnerable) return;
                if (!_effect.Ability.Piercing) Impact(target);
                else if (_pierced.Add(target)) PiercedThrough?.Invoke(_body.position, _body.linearVelocity, target);
                return;
            }
            if (other.isTrigger) return;
            if (other.TryGetComponent(out AttackBlocker _))
            {
                if (!TryCombo(other)) Impact(null);
                return;
            }
            if (other.gameObject.layer == Layers.GroundIndex) Impact(null);
        }

        /// <summary>The live projectile on <paramref name="body"/>, without a GetComponent call (safe in FixedUpdate).</summary>
        public static bool TryFind(Rigidbody2D body, out Projectile projectile)
        {
            projectile = null;
            return body != null && ByBody.TryGetValue(body, out projectile) && projectile != null && projectile.IsSpawned && !projectile._done;
        }

        /// <summary>Server, every physics step a pull acts: turns the velocity toward <paramref name="target"/> by at most <paramref name="maxChange"/>.</summary>
        public void ServerSteer(Vector2 target, float maxChange)
        {
            if (!IsServer || _done) return;
            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity, target, maxChange);
        }

        /// <summary>Server: sends the projectile along <paramref name="push"/>, keeping its speed if that is faster.</summary>
        public void ServerRedirect(Vector2 push)
        {
            if (!IsServer || _done || push.sqrMagnitude < 0.0001f) return;
            var speed = Mathf.Max(_body.linearVelocity.magnitude, push.magnitude);
            _body.linearVelocity = push.normalized * speed;
        }

        /// <summary>Server: ends the flight without an impact (a boomerang back in the caster's hand).</summary>
        public void ServerFinish()
        {
            if (!IsServer || _done) return;
            _done = true;
            NetworkObject.Despawn();
        }

        private bool TryCombo(Collider2D blocker)
        {
            var summon = blocker.GetComponentInParent<AbilityEffect>();
            if (summon == null) return false;
            var combos = _effect.Combos;
            if (ComboRules.Match(combos, _effect.Tags, summon.Tags, ComboInteraction.Absorb, out _))
            {
                _done = true;
                NetworkObject.Despawn();
                return true;
            }
            if (!ComboRules.Match(combos, _effect.Tags, summon.Tags, ComboInteraction.Ricochet, out var rule) || _bounces >= rule.MaxRepeats)
                return false;

            _bounces++;
            Ricochet(blocker.bounds, rule.Strength);
            return true;
        }

        private void Ricochet(Bounds bounds, float keep)
        {
            var velocity = _body.linearVelocity;
            var previous = _body.position - velocity * Time.fixedDeltaTime;
            var cameFromSide = previous.x < bounds.min.x || previous.x > bounds.max.x;
            if (cameFromSide) velocity.x = -velocity.x;
            else velocity.y = -velocity.y;
            _body.position = previous;
            _body.linearVelocity = velocity * keep;
        }

        private void Impact(Health target)
        {
            _done = true;
            Impacted?.Invoke(_body.position, _body.linearVelocity, target);
            NetworkObject.Despawn();
        }
    }
}
