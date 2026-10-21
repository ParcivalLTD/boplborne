using UnityEngine;

namespace GraceRun.UI.Splash
{
    /// <summary>Timing of the start-up splash: a black cover that lifts, then the whole splash fading away.</summary>
    public static class SplashRules
    {
        /// <summary>Opacity of the black cover: fully black at 0 s, clear after <paramref name="fadeSeconds"/>.</summary>
        public static float CoverAlpha(float time, float fadeSeconds) =>
            fadeSeconds <= 0f ? 0f : 1f - Mathf.Clamp01(time / fadeSeconds);

        /// <summary>Opacity of the splash itself: 1 until the last <paramref name="fadeSeconds"/> of <paramref name="totalSeconds"/>, then down to 0.</summary>
        public static float PanelAlpha(float time, float totalSeconds, float fadeSeconds) =>
            fadeSeconds <= 0f ? (time < totalSeconds ? 1f : 0f) : Mathf.Clamp01((totalSeconds - time) / fadeSeconds);

        /// <summary>A key press is ignored for the first moments so the launch click does not skip the splash.</summary>
        public static bool CanSkip(float time, float graceSeconds) => time >= graceSeconds;

        /// <summary>True once the splash has nothing left to show.</summary>
        public static bool IsDone(float time, float totalSeconds) => time >= totalSeconds;
    }
}
