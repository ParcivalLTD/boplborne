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

        [Header("Crits (a player hit while a boss's gold weak point shows)")]
        [SerializeField, Min(0f)] private float _critHitstop = 0.2f;
        [SerializeField, Min(0f), Tooltip("Your own crit shakes the screen this hard.")] private float _critShake = 1.1f;
        [SerializeField] private Color _critTextColor = new(1f, 0.79f, 0.29f);
        [SerializeField, Tooltip("Played on top of the normal sparks; falls back to playing the sparks twice.")]
        private ParticleSystem _critSparks;
        [SerializeField] private AudioCue _critCue;
        [SerializeField] private string _critText = "CRIT!";
        [SerializeField, Min(1)] private int _critTextSize = 64;
        [SerializeField, Min(0f)] private float _critTextLifetime = 0.8f;
        [SerializeField, Min(0f), Tooltip("Units the text rises over its lifetime.")] private float _critTextRise = 1.6f;

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
        public float CritHitstop => _critHitstop;
        public float CritShake => _critShake;
        public Color CritTextColor => _critTextColor;
        public ParticleSystem CritSparks => _critSparks != null ? _critSparks : _sparks;
        public AudioCue CritCue => _critCue != null ? _critCue : _heavyHitCue;
        public string CritText => _critText;
        public int CritTextSize => _critTextSize;
        public float CritTextLifetime => _critTextLifetime;
        public float CritTextRise => _critTextRise;
        public ParticleSystem Sparks => _sparks;
        public AudioCue HitCue => _hitCue;
        public AudioCue HeavyHitCue => _heavyHitCue;
        public AudioCue PlayerHurtCue => _playerHurtCue;
    }
}
