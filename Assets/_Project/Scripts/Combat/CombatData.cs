using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Damage rules shared by every target, plus the numbers the debug commands use.
    /// Tune the asset under Data/Config, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Combat Data", fileName = "CombatData")]
    public class CombatData : ScriptableObject
    {
        [Header("Friendly fire")]
        [SerializeField, Tooltip("GDD: on. When off, hits from a teammate are ignored completely.")]
        private bool _friendlyFireEnabled = true;
        [SerializeField, Range(0f, 1f)] private float _friendlyFireDamageMultiplier = 0.1f;
        [SerializeField, Min(0)] private int _friendlyFireMinHP = 1;

        [Header("Status effects")]
        [SerializeField, Range(0f, 1f), Tooltip("A shrunk attacker's hits deal this share of their damage.")]
        private float _shrunkDamageMultiplier = 0.5f;
        [SerializeField, Min(0.01f), Tooltip("How fast a shrinking or growing body changes size (scale per second).")]
        private float _statusScaleSpeed = 4f;

        [Header("Debug commands")]
        [SerializeField, Min(0)] private int _debugDamage = 20;
        [SerializeField] private Vector2 _debugKnockback = new(8f, 6f);

        /// <summary>When off, a player's hit on another player does nothing (no damage, no knockback).</summary>
        public bool FriendlyFireEnabled => _friendlyFireEnabled;

        /// <summary>Share of the damage a teammate's hit deals (GDD: 10%).</summary>
        public float FriendlyFireDamageMultiplier => _friendlyFireDamageMultiplier;

        /// <summary>A teammate's hit can never take you below this HP.</summary>
        public int FriendlyFireMinHP => _friendlyFireMinHP;

        /// <summary>Share of the damage a shrunk attacker deals.</summary>
        public float ShrunkDamageMultiplier => _shrunkDamageMultiplier;

        /// <summary>Scale change per second while a shrink starts or wears off.</summary>
        public float StatusScaleSpeed => _statusScaleSpeed;

        public int DebugDamage => _debugDamage;

        /// <summary>Knockback velocity of the debug "hit dummy" command, x pointing away from the player.</summary>
        public Vector2 DebugKnockback => _debugKnockback;
    }
}
