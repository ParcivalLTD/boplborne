using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Tunable stats for a <see cref="BroodCrawler"/> minion spawned by The Brood Mother.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Crawler Data", fileName = "CrawlerData")]
    public class CrawlerData : ScriptableObject, IHealthStats
    {
        [SerializeField, Min(1)] private int _maxHP = 24;
        [SerializeField, Min(0f)] private float _moveSpeed = 4.2f;
        [SerializeField, Min(0f), Tooltip("Horizontal distance within which the crawler lunges at its target.")]
        private float _hopRange = 2.6f;
        [SerializeField] private Vector2 _hopVelocity = new(6.5f, 6.5f);
        [SerializeField, Min(0.1f), Tooltip("Seconds between contact bites and hops.")]
        private float _biteCooldown = 1.1f;
        [SerializeField, Min(0)] private int _damage = 15;
        [SerializeField] private Vector2 _knockback = new(7f, 5f);
        [SerializeField, Min(0f), Tooltip("Seconds a push or pull stops the crawler from steering itself.")]
        private float _knockbackStunTime = 0.35f;
        [SerializeField, Min(0f)] private float _despawnDelay = 0.25f;

        public int MaxHP => _maxHP;
        public float MoveSpeed => _moveSpeed;
        public float HopRange => _hopRange;
        public Vector2 HopVelocity => _hopVelocity;
        public float BiteCooldown => _biteCooldown;
        public int Damage => _damage;
        public Vector2 Knockback => _knockback;
        public float KnockbackStunTime => _knockbackStunTime;
        public float DespawnDelay => _despawnDelay;
    }
}
