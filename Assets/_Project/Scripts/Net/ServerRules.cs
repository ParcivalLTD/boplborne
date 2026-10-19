using System;

namespace GraceRun.Net
{
    /// <summary>Pure rules for a dedicated server that runs unattended: when to reset an abandoned run, and how status is worded.</summary>
    public static class ServerRules
    {
        /// <summary>
        /// True when the server should drop the unfinished run and go back to the hub: nobody has
        /// been connected for <paramref name="graceSeconds"/>, and it is not already sitting clean
        /// in the hub waiting for players.
        /// </summary>
        public static bool ShouldResetToHub(int connectedPlayers, double emptySeconds, double graceSeconds, bool alreadyInCleanHub) =>
            connectedPlayers <= 0 && !alreadyInCleanHub && emptySeconds >= graceSeconds;

        /// <summary>"3d 4h 05m", "4h 05m", "12m 30s" or "45s" for a running time.</summary>
        public static string FormatUptime(double seconds)
        {
            var total = (long)Math.Max(0d, seconds);
            var days = total / 86400;
            var hours = total % 86400 / 3600;
            var minutes = total % 3600 / 60;
            var secs = total % 60;
            if (days > 0) return $"{days}d {hours}h {minutes:00}m";
            if (hours > 0) return $"{hours}h {minutes:00}m";
            if (minutes > 0) return $"{minutes}m {secs:00}s";
            return $"{secs}s";
        }

        /// <summary>One log line: "[Server] players 2/4, Fighting, floor 3, up 1h 02m".</summary>
        public static string FormatStatus(int players, int maxPlayers, string phase, int floor, double uptimeSeconds) =>
            $"[Server] players {players}/{maxPlayers}, {phase}, {(floor == 0 ? "hub" : "floor " + floor)}, up {FormatUptime(uptimeSeconds)}";
    }
}
