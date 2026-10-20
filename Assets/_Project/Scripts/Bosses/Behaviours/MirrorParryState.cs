using GraceRun.Combat;
using GraceRun.Core.Audio;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Manages The Mirror Duelist's defensive parry stance. While active, incoming hits
    /// trigger an immediate crystalline deflect sound and a retaliatory wave of mirror projectiles.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class MirrorParryState : MonoBehaviour
    {
        [SerializeField] private GameObject _counterProjectilePrefab;
        [SerializeField] private AudioCue _parryCue;
        [SerializeField] private SpriteRenderer _shieldRenderer;

        private Health _health;
        private BossAttackRunner _runner;
        private bool _isParrying;
        private float _counterCooldown;

        public bool IsParrying => _isParrying;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _runner = GetComponent<BossAttackRunner>();
            if (_shieldRenderer != null) _shieldRenderer.enabled = false;
        }

        private void OnEnable()
        {
            if (_health != null) _health.HitTaken += OnHitTaken;
        }

        private void OnDisable()
        {
            if (_health != null) _health.HitTaken -= OnHitTaken;
            SetParry(false);
        }

        public void SetCounterPrefab(GameObject prefab) => _counterProjectilePrefab = prefab;

        public void SetParry(bool on)
        {
            _isParrying = on;
            if (_shieldRenderer != null)
            {
                _shieldRenderer.enabled = on;
                _shieldRenderer.color = new Color(0.73f, 0.65f, 1f, on ? 0.65f : 0f);
            }
        }

        private void OnHitTaken(DamageInfo info)
        {
            if (!_isParrying || _runner == null || !_runner.IsServer) return;
            if (Time.time < _counterCooldown) return;
            _counterCooldown = Time.time + 0.4f;

            if (_parryCue != null) AudioCuePlayer.Play2D(_parryCue);

            if (_counterProjectilePrefab != null)
            {
                var origin = (Vector2)transform.position + new Vector2(0f, 1f);
                var facing = _runner.Facing;
                var angles = BulletPatternMath.ComputeSpreadAngles(3, 30f, facing > 0 ? 0f : 180f);
                for (var i = 0; i < angles.Length; i++)
                {
                    var vel = BulletPatternMath.AngleToVelocity(angles[i], 14f);
                    var inst = Instantiate(_counterProjectilePrefab, origin, Quaternion.identity);
                    if (inst.TryGetComponent(out BossProjectile proj))
                    {
                        proj.ServerConfigure(vel, 18, new Vector2(8f * facing, 4f), 3.0f, 0);
                    }
                    if (inst.TryGetComponent(out NetworkObject netObj))
                    {
                        netObj.Spawn();
                    }
                }
            }
        }
    }
}
