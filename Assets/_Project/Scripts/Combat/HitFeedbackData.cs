using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// How a landed hit looks, sounds and feels: hitstop, flash, sparks, shake and sounds.
    /// Shared by every target; tune the asset under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Hit Feedback Data", fileName = "HitFeedbackData")]
    public class HitFeedbackData : ScriptableObject
    {
        [Header("Heavy hits")]
        [SerializeField, Min(0f), Tooltip("Knockback speed (units/s) from which a hit counts as heavy.")]
        private float _heavyKnockback = 4f;

        [Header("Hitstop (seconds, attacker and victim)")]
        [SerializeField, Min(0f)] private float _lightHitstop = 0.05f;
        [SerializeField, Min(0f)] private float _heavyHitstop = 0.11f;
        [SerializeField, Min(0f), Tooltip("How far the victim's sprite shakes during hitstop (units).")]
        private float _victimJitter = 0.08f;

        [Header("Flash")]
        [SerializeField, Min(0f), Tooltip("The flash also lasts at least as long as the hitstop.")]
        private float _flashDuration = 0.08f;

        [Header("Screen shake (only when your own player is involved)")]
        [SerializeField, Min(0f), Tooltip("Your heavy hit lands on something.")] private float _heavyHitShake = 0.5f;
        [SerializeField, Min(0f), Tooltip("Your player takes a hit.")] private float _hurtShake = 0.7f;

        [Header("Particles and sounds")]
        [SerializeField] private ParticleSystem _sparks;
        [SerializeField] private AudioCue _hitCue;
        [SerializeField] private AudioCue _heavyHitCue;
        [SerializeField, Tooltip("Played instead of the hit cue when a player is the one hit.")]
        private AudioCue _playerHurtCue;

        public float HeavyKnockback => _heavyKnockback;
        public float LightHitstop => _lightHitstop;
        public float HeavyHitstop => _heavyHitstop;
        public float VictimJitter => _victimJitter;
        public float FlashDuration => _flashDuration;
        public float HeavyHitShake => _heavyHitShake;
        public float HurtShake => _hurtShake;
        public ParticleSystem Sparks => _sparks;
        public AudioCue HitCue => _hitCue;
        public AudioCue HeavyHitCue => _heavyHitCue;
        public AudioCue PlayerHurtCue => _playerHurtCue;
    }
}
