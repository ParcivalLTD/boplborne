using System;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// One swing of the light attack combo: its timing, cost, damage and hitbox. Offsets and
    /// knockback are written for a player facing right and mirrored when facing left.
    /// </summary>
    [Serializable]
    public struct LightAttackHit
    {
        [SerializeField, Min(0)] private int _damage;
        [SerializeField, Tooltip("Velocity given to the target, x pointing away from the attacker.")]
        private Vector2 _knockback;
        [SerializeField] private Vector2 _hitboxSize;
        [SerializeField] private Vector2 _hitboxOffset;
        [SerializeField, Min(0f)] private float _startup;
        [SerializeField, Min(0.01f)] private float _active;
        [SerializeField, Min(0f)] private float _recovery;
        [SerializeField, Min(0f)] private float _staminaCost;

        public LightAttackHit(int damage, Vector2 knockback, Vector2 hitboxSize, Vector2 hitboxOffset,
            float startup, float active, float recovery, float staminaCost)
        {
            _damage = damage;
            _knockback = knockback;
            _hitboxSize = hitboxSize;
            _hitboxOffset = hitboxOffset;
            _startup = startup;
            _active = active;
            _recovery = recovery;
            _staminaCost = staminaCost;
        }

        public int Damage => _damage;
        public Vector2 Knockback => _knockback;
        public Vector2 HitboxSize => _hitboxSize;
        public Vector2 HitboxOffset => _hitboxOffset;

        /// <summary>Seconds from the press until the hitbox turns on.</summary>
        public float Startup => _startup;

        /// <summary>Seconds the hitbox stays on.</summary>
        public float Active => _active;

        /// <summary>Seconds after the hitbox turns off before the next swing can start.</summary>
        public float Recovery => _recovery;

        public float StaminaCost => _staminaCost;
    }
}
