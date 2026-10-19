using UnityEngine;

namespace GraceRun.Net
{
    /// <summary>
    /// Tuning for a dedicated (server-only) build that runs around the clock. Hosts that play
    /// themselves are not affected. Tune the asset under Data/Config, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Server Data", fileName = "ServerData")]
    public class ServerData : ScriptableObject
    {
        [SerializeField, Min(0f), Tooltip("Seconds with nobody connected before the server throws away the unfinished run and goes back to the hub. Long enough to survive a quick reconnect after a crash. 0 resets at once.")]
        private float _idleResetSeconds = 30f;
        [SerializeField, Min(0f), Tooltip("Seconds between status lines in the log (players, phase, floor, uptime). 0 turns them off.")]
        private float _statusLogSeconds = 300f;
        [SerializeField, Min(0f), Tooltip("Seconds between writes of the status file when -statusfile is given.")]
        private float _statusFileSeconds = 15f;
        [SerializeField, Range(10, 240), Tooltip("Frame rate cap for the server. A headless build has no vsync and would otherwise use a whole CPU core.")]
        private int _targetFrameRate = 60;

        public float IdleResetSeconds => _idleResetSeconds;
        public float StatusLogSeconds => _statusLogSeconds;
        public float StatusFileSeconds => _statusFileSeconds;
        public int TargetFrameRate => _targetFrameRate;
    }
}
