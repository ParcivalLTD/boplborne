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

        [Header("Debug commands")]
        [SerializeField, Min(0)] private int _debugDamage = 20;
        [SerializeField] private Vector2 _debugKnockback = new(8f, 6f);

        /// <summary>When off, a player's hit on another player does nothing (no damage, no knockback).</summary>
        public bool FriendlyFireEnabled => _friendlyFireEnabled;

        /// <summary>Share of the damage a teammate's hit deals (GDD: 10%).</summary>
        public float FriendlyFireDamageMultiplier => _friendlyFireDamageMultiplier;

        /// <summary>A teammate's hit can never take you below this HP.</summary>
        public int FriendlyFireMinHP => _friendlyFireMinHP;

        public int DebugDamage => _debugDamage;

        /// <summary>Knockback velocity of the debug "hit dummy" command, x pointing away from the player.</summary>
        public Vector2 DebugKnockback => _debugKnockback;
    }
}
