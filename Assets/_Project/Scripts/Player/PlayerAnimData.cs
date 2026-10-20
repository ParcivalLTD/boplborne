using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Numbers for the player's animation: when it counts as running, how long a hurt pose
    /// holds, and each clip's frame rate (read by Grace Run → Build Player Animations).
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Player Anim Data", fileName = "PlayerAnimData")]
    public class PlayerAnimData : ScriptableObject
    {
        [Header("State")]
        [SerializeField, Min(0f), Tooltip("Horizontal speed (units/s) above which a grounded player plays Run.")]
        private float _runSpeedThreshold = 0.5f;
        [SerializeField, Min(0f), Tooltip("How long the Hurt clip holds after losing HP, in seconds.")]
        private float _hurtTime = 0.25f;

        [Header("Frame rates (rebuild the clips after changing)")]
        [SerializeField, Min(1f)] private float _idleFps = 12f;
        [SerializeField, Min(1f)] private float _runFps = 16f;
        [SerializeField, Min(1f)] private float _airFps = 12f;
        [SerializeField, Min(1f)] private float _dodgeFps = 20f;
        [SerializeField, Min(1f)] private float _attackFps = 20f;
        [SerializeField, Min(1f)] private float _hurtFps = 12f;
        [SerializeField, Min(1f)] private float _ghostFps = 10f;

        public float RunSpeedThreshold => _runSpeedThreshold;
        public float HurtTime => _hurtTime;
        public float IdleFps => _idleFps;
        public float RunFps => _runFps;
        public float AirFps => _airFps;
        public float DodgeFps => _dodgeFps;
        public float AttackFps => _attackFps;
        public float HurtFps => _hurtFps;
        public float GhostFps => _ghostFps;
    }
}
