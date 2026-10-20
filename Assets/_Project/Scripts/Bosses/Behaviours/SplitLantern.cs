using System.Collections.Generic;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// One of the four floating spectral lanterns spawned during The Lantern Witch's Phase 2.
    /// Hovers in the arena, illuminates a pocket of light in the darkness, and periodically
    /// emits radial spirit projectile bursts.
    /// </summary>
    public class SplitLantern : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer _coreRenderer;
        [SerializeField] private SpriteRenderer _haloRenderer;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField, Min(1f)] private float _pulseInterval = 4.5f;

        private static readonly List<SplitLantern> Active = new();

        private Vector3 _anchor;
        private float _seed;
        private float _pulseTimer;

        public static int ActiveCount => Active.Count;

        public static Vector2[] ActivePositions()
        {
            var result = new Vector2[Active.Count];
            for (var i = 0; i < Active.Count; i++)
            {
                result[i] = Active[i] != null ? (Vector2)Active[i].transform.position : Vector2.zero;
            }
            return result;
        }

        public void ServerConfigure(Vector2 position, GameObject projectilePrefab = null)
        {
            _anchor = position;
            transform.position = position;
            if (projectilePrefab != null) _projectilePrefab = projectilePrefab;
        }

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _anchor = transform.position;
            _seed = NetworkObjectId * 0.77f;
            _pulseTimer = 1.5f + (_seed % 2f);
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
        }

        private void Update()
        {
            // Gentle hovering float
            transform.position = _anchor + new Vector3(0f, Mathf.Sin((Time.time + _seed) * 2.2f) * 0.45f, 0f);

            if (_haloRenderer != null)
            {
                var pulse = 0.35f + (0.15f * Mathf.Sin((Time.time + _seed) * 3.5f));
                _haloRenderer.color = new Color(0.95f, 0.88f, 0.72f, pulse);
            }

            if (!IsServer || !IsSpawned || BossBrain.AreAllDefeated) return;

            _pulseTimer -= Time.deltaTime;
            if (_pulseTimer <= 0f)
            {
                _pulseTimer = _pulseInterval;
                FireRadialPulse();
            }
        }

        private void FireRadialPulse()
        {
            if (_projectilePrefab == null) return;

            var origin = (Vector2)transform.position;
            var angles = BulletPatternMath.ComputeRingAngles(6, (Time.time * 25f) % 360f);

            for (var i = 0; i < angles.Length; i++)
            {
                var vel = BulletPatternMath.AngleToVelocity(angles[i], 9.5f);
                var inst = Instantiate(_projectilePrefab, origin, Quaternion.identity);

                if (inst.TryGetComponent(out BossProjectile proj))
                {
                    proj.ServerConfigure(vel, 14, new Vector2(vel.x * 0.5f, 4f), lifetime: 3.5f, bounces: 1);
                }

                if (inst.TryGetComponent(out NetworkObject netObj))
                {
                    netObj.Spawn();
                }
            }
        }

        /// <summary>Server only: despawns all active split lanterns at once (fight reset, arena swap).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var lantern = Active[i];
                if (lantern != null && lantern.IsServer && lantern.IsSpawned)
                {
                    lantern.NetworkObject.Despawn();
                }
            }
            Active.Clear();
        }
    }
}
