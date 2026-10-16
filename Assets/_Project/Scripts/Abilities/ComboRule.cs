using System;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>One row of the <see cref="ComboRules"/> asset: when <see cref="Source"/> meets <see cref="Target"/>, do <see cref="Interaction"/>.</summary>
    [Serializable]
    public struct ComboRule
    {
        [SerializeField, Tooltip("The ability that meets something (any of these tags).")]
        private AbilityTags _source;
        [SerializeField, Tooltip("What it meets (any of these tags, or a teammate).")]
        private ComboSubject _target;
        [SerializeField] private ComboInteraction _interaction;
        [SerializeField, Min(0f), Tooltip("TakeForce: force multiplier. Ricochet: share of the speed kept per bounce.")]
        private float _strength;
        [SerializeField, Min(0), Tooltip("Ricochet: bounces per projectile before it impacts as usual.")]
        private int _maxRepeats;

        public ComboRule(AbilityTags source, ComboSubject target, ComboInteraction interaction, float strength = 1f, int maxRepeats = 0)
        {
            _source = source;
            _target = target;
            _interaction = interaction;
            _strength = strength;
            _maxRepeats = maxRepeats;
        }

        public AbilityTags Source => _source;
        public ComboSubject Target => _target;
        public ComboInteraction Interaction => _interaction;
        public float Strength => _strength;
        public int MaxRepeats => _maxRepeats;
    }
}
