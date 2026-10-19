using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Tunable stats for The Bell Twins' ringing ground shockwaves (speed, lifetime, damage, knockback,
    /// arena bounds and fade-out). Lives in Data/Bosses/BellTwins/ShockwaveData.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Shockwave Data", fileName = "ShockwaveData")]
    public class ShockwaveData : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _speed = 11.5f;
        [SerializeField, Min(0.2f)] private float _lifetime = 2.2f;
        [SerializeField, Min(1)] private int _damage = 25;
        [SerializeField] private Vector2 _knockback = new(9f, 7f);
        [SerializeField, Min(1f)] private float _arenaHalfWidth = 15.2f;
        [SerializeField, Min(0.1f)] private float _blockerCheckDistance = 0.6f;
        [SerializeField, Min(0f)] private float _fadeOutTime = 0.35f;
        [SerializeField, Range(0f, 1f)] private float _baseAlpha = 0.85f;

        public float Speed => _speed;
        public float Lifetime => _lifetime;
        public int Damage => _damage;
        public Vector2 Knockback => _knockback;
        public float ArenaHalfWidth => _arenaHalfWidth;
        public float BlockerCheckDistance => _blockerCheckDistance;
        public float FadeOutTime => _fadeOutTime;
        public float BaseAlpha => _baseAlpha;
    }
}
