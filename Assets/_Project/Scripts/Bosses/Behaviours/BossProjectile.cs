using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using GraceRun.Core.Framing;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Cuphead-style boss bullet projectile. Moves along its velocity, arms a victim-side Hitbox
    /// on the BossAttack layer, can bounce off walls and floors, and despawns on contact with
    /// an AttackBlocker (Stone Wall) or when lifetime expires.
    /// </summary>
    public class BossProjectile : NetworkBehaviour
    {
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private SpriteRenderer[] _renderers;
        [SerializeField] private TrailRenderer _trail;

        private static readonly List<BossProjectile> Active = new();
        private static readonly RaycastHit2D[] CastHits = new RaycastHit2D[4];

        private readonly NetworkVariable<Vector2> _velocity = new(
            Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _damage = new(
            15, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _lifetime = new(
            4f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _bounces = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Vector2 _seededVelocity;
        private int _seededDamage = 15;
        private Vector2 _seededKnockback = new(6f, 4f);
        private float _seededLifetime = 4f;
        private int _seededBounces;
        private float _seededGravity;
        private float _age;
        private ContactFilter2D _collisionFilter;

        public static int ActiveCount => Active.Count;

        private void Awake()
        {
            _collisionFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.GroundIndex) | Layers.Mask(Layers.AbilityIndex),
                useTriggers = false
            };
        }

        /// <summary>Server only, before Spawn: sets velocity, damage, knockback, bounces and physics.</summary>
        public void ServerConfigure(Vector2 velocity, int damage, Vector2 knockback, float lifetime = 4f, int bounces = 0, float gravity = 0f)
        {
            _seededVelocity = velocity;
            _seededDamage = damage;
            _seededKnockback = knockback;
            _seededLifetime = lifetime;
            _seededBounces = bounces;
            _seededGravity = gravity;
        }

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _age = 0f;

            if (IsServer)
            {
                _velocity.Value = _seededVelocity;
                _damage.Value = _seededDamage;
                _lifetime.Value = _seededLifetime;
                _bounces.Value = _seededBounces;
            }

            if (_hitbox != null)
            {
                _hitbox.Arm(_damage.Value, _seededKnockback);
            }
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            if (_hitbox != null) _hitbox.Disarm();
            if (_trail != null) _trail.Clear();
        }

        private void Update()
        {
            if (!IsSpawned) return;

            var vel = _velocity.Value;
            transform.position += (Vector3)(vel * Time.deltaTime);

            if (vel.sqrMagnitude > 0.01f)
            {
                var angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            if (!IsServer) return;

            _age += Time.deltaTime;
            if (_seededGravity > 0f)
            {
                vel.y -= _seededGravity * Time.deltaTime;
                _velocity.Value = vel;
            }

            if (_age >= _lifetime.Value || BossBrain.AreAllDefeated)
            {
                NetworkObject.Despawn();
                return;
            }

            CheckCollisions(vel);
        }

        private void CheckCollisions(Vector2 vel)
        {
            var origin = (Vector2)transform.position;
            var dist = Mathf.Max(0.15f, vel.magnitude * Time.deltaTime * 1.2f);
            var count = Physics2D.Raycast(origin, vel.normalized, _collisionFilter, CastHits, dist);

            for (var i = 0; i < count; i++)
            {
                var hit = CastHits[i];
                if (hit.collider == null) continue;

                if (hit.collider.TryGetComponent(out AttackBlocker _))
                {
                    NetworkObject.Despawn();
                    return;
                }

                if (_bounces.Value > 0)
                {
                    _bounces.Value--;
                    _velocity.Value = BulletPatternMath.Reflect(vel, hit.normal, 0.9f);
                    transform.position = hit.point + (hit.normal * 0.1f);
                    return;
                }

                NetworkObject.Despawn();
                return;
            }
        }

        /// <summary>Server only: despawns all active projectiles at once (arena swap, fight reset).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var p = Active[i];
                if (p != null && p.IsServer && p.IsSpawned)
                {
                    p.NetworkObject.Despawn();
                }
            }
            Active.Clear();
        }
    }
}
