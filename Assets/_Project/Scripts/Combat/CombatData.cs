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

        [Header("Damage roles (docs/GDD.md: abilities are the main way to fight)")]
        [SerializeField, Min(0f), Tooltip("Players' weapon swings deal this share of their authored damage to enemies. Weapons build the ability meter and chip poise; they are not the main damage.")]
        private float _weaponDamageMultiplier = 0.5f;
        [SerializeField, Min(0f), Tooltip("Ability hits deal this many times their authored damage to enemies.")]
        private float _abilityDamageMultiplier = 3f;

        [SerializeField, Min(0f), Tooltip("Arena: a hit on a player of the other team deals this share of its authored damage (weapons and abilities alike). 1 = the numbers in the asset.")]
        private float _pvpDamageMultiplier = 1f;
        [SerializeField, Min(0f), Tooltip("Seconds a weapon hit that lands on an enemy shaves off every ability cooldown of the attacker, per point of the hit's authored damage.")]
        private float _cooldownShavePerDamage = 0.025f;
        [SerializeField, Min(0f), Tooltip("Most seconds one weapon hit can shave off each ability cooldown.")]
        private float _maxCooldownShavePerHit = 0.6f;

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

        public float WeaponDamageMultiplier => _weaponDamageMultiplier;
        public float AbilityDamageMultiplier => _abilityDamageMultiplier;
        public float PvpDamageMultiplier => _pvpDamageMultiplier;

        /// <summary>Multiplier for a player hit from <paramref name="source"/> (<see cref="DamageSource"/>): weapon swings (unknown or melee) or abilities.</summary>
        public float DamageMultiplierFor(ushort source) =>
            source <= DamageSource.Melee ? _weaponDamageMultiplier : _abilityDamageMultiplier;

        /// <summary>Seconds a weapon hit of <paramref name="authoredDamage"/> takes off each ability cooldown (the weapon builds the abilities).</summary>
        public float CooldownShave(int authoredDamage) =>
            Mathf.Min(_maxCooldownShavePerHit, Mathf.Max(0, authoredDamage) * _cooldownShavePerDamage);

        /// <summary>Share of the damage a shrunk attacker deals.</summary>
        public float ShrunkDamageMultiplier => _shrunkDamageMultiplier;

        /// <summary>Scale change per second while a shrink starts or wears off.</summary>
        public float StatusScaleSpeed => _statusScaleSpeed;

        public int DebugDamage => _debugDamage;

        /// <summary>Knockback velocity of the debug "hit dummy" command, x pointing away from the player.</summary>
        public Vector2 DebugKnockback => _debugKnockback;
    }
}
