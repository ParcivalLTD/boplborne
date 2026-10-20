using GraceRun.Core.Audio;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// BossAttackBehaviour that fires Cuphead-style bullet-hell spreads, rings, or aimed arcs
    /// during a specified phase of an attack.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Behaviours/Bullet Pattern", fileName = "BulletPatternBehaviour")]
    public class BulletPatternBehaviour : BossAttackBehaviour
    {
        [Header("Trigger Phase")]
        [SerializeField] private AttackPhase _triggerPhase = AttackPhase.Active;

        [Header("Formation")]
        [SerializeField] private BulletPatternType _patternType = BulletPatternType.Spread;
        [SerializeField, Min(1)] private int _count = 3;
        [SerializeField, Range(0f, 360f)] private float _spreadAngle = 40f;
        [SerializeField] private float _baseAngleOffset;

        [Header("Projectile Specs")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField, Min(1f)] private float _speed = 12f;
        [SerializeField, Min(0)] private int _damage = 15;
        [SerializeField] private Vector2 _knockback = new(6f, 4f);
        [SerializeField, Min(0.1f)] private float _lifetime = 4f;
        [SerializeField, Min(0)] private int _bounces;
        [SerializeField, Min(0f)] private float _gravity;
        [SerializeField] private Vector2 _spawnOffset = new(0f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioCue _fireCue;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != _triggerPhase || !runner.IsServer || _projectilePrefab == null) return;

            var facing = runner.Facing;
            var origin = (Vector2)runner.transform.position + new Vector2(_spawnOffset.x * facing, _spawnOffset.y);
            float[] angles;

            switch (_patternType)
            {
                case BulletPatternType.Ring:
                    angles = BulletPatternMath.ComputeRingAngles(_count, _baseAngleOffset);
                    break;
                case BulletPatternType.Spread:
                default:
                    var centerAngle = BulletPatternMath.FacingAngle(facing, _baseAngleOffset);
                    angles = BulletPatternMath.ComputeSpreadAngles(_count, _spreadAngle, centerAngle);
                    break;
            }

            for (var i = 0; i < angles.Length; i++)
            {
                var vel = BulletPatternMath.AngleToVelocity(angles[i], _speed);
                var instance = Instantiate(_projectilePrefab, origin, Quaternion.identity);
                if (instance.TryGetComponent(out BossProjectile projectile))
                {
                    var kb = new Vector2(_knockback.x * Mathf.Sign(vel.x != 0 ? vel.x : facing), _knockback.y);
                    projectile.ServerConfigure(vel, _damage, kb, _lifetime, _bounces, _gravity);
                }

                if (instance.TryGetComponent(out NetworkObject netObj))
                {
                    netObj.Spawn();
                }
            }

            if (_fireCue != null)
            {
                AudioCuePlayer.Play2D(_fireCue);
            }
        }
    }
}
