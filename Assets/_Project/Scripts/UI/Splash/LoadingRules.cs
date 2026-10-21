using UnityEngine;

namespace GraceRun.UI.Splash
{
    /// <summary>Pure rules for the loading screen: when it shows, how it fades and which tip it reads.</summary>
    public static class LoadingRules
    {
        /// <summary>The overlay covers the menu while a host or join is working and the game scene is not up yet.</summary>
        public static bool ShouldShow(bool busy, bool inGameScene) => busy && !inGameScene;

        /// <summary>Once shown, it stays at least <paramref name="minSeconds"/> so a quick connect does not flash.</summary>
        public static bool CanHide(float shownFor, float minSeconds) => shownFor >= minSeconds;

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> (0 or 1) over <paramref name="fadeSeconds"/>.</summary>
        public static float NextAlpha(float current, float target, float deltaTime, float fadeSeconds) =>
            fadeSeconds <= 0f ? target : Mathf.MoveTowards(current, target, deltaTime / fadeSeconds);

        /// <summary>Which of <paramref name="count"/> tips to show after <paramref name="elapsed"/> seconds, changing every <paramref name="tipSeconds"/>.</summary>
        public static int TipIndex(float elapsed, int count, float tipSeconds)
        {
            if (count <= 0 || tipSeconds <= 0f) return 0;
            return (int)(Mathf.Max(0f, elapsed) / tipSeconds) % count;
        }

        private const float DotSeconds = 0.4f;

        /// <summary>"Opening your room" plus one to three dots, cycling every <see cref="DotSeconds"/>.</summary>
        public static string Dots(string message, float elapsed)
        {
            var count = 1 + (int)(Mathf.Max(0f, elapsed) / DotSeconds) % 3;
            return message.TrimEnd('.', ' ', '…') + new string('.', count);
        }
    }
}
