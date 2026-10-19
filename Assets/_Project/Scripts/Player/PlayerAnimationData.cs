using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Thresholds and fallback timers that drive the player's <see cref="Animator"/> states
    /// (idle, run, jump, fall, dodge, attack1–3, hurt, ghost). Tune the asset under Data/Player.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Player Animation Data", fileName = "PlayerAnimationData")]
    public class PlayerAnimationData : ScriptableObject
    {
        [Header("Motion thresholds")]
        [SerializeField, Min(0.01f), Tooltip("Horizontal speed (u/s) above which a grounded player plays the run clip.")]
        private float _runSpeedThreshold = 0.25f;
        [SerializeField, Min(0.01f), Tooltip("Vertical speed (u/s) separating rising (jump) from falling (fall).")]
        private float _airSpeedThreshold = 0.15f;
        [SerializeField, Min(0.01f), Tooltip("Horizontal speed (u/s) needed for a remote player's sprite to flip facing.")]
        private float _facingFlipThreshold = 0.15f;

        [Header("Action clip durations")]
        [SerializeField, Min(0.05f), Tooltip("Seconds the hurt clip plays after taking a hit.")]
        private float _hurtDuration = 0.22f;
        [SerializeField, Min(0.05f), Tooltip("Seconds an attack clip plays on remote PCs after SwingStarted.")]
        private float _attackClipDuration = 0.28f;
        [SerializeField, Min(0.05f), Tooltip("Seconds the dodge clip plays after RollStarted.")]
        private float _dodgeClipDuration = 0.35f;

        public float RunSpeedThreshold => _runSpeedThreshold;
        public float AirSpeedThreshold => _airSpeedThreshold;
        public float FacingFlipThreshold => _facingFlipThreshold;
        public float HurtDuration => _hurtDuration;
        public float AttackClipDuration => _attackClipDuration;
        public float DodgeClipDuration => _dodgeClipDuration;
    }
}
