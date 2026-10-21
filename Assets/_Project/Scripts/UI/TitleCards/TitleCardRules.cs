using UnityEngine;

namespace GraceRun.UI.TitleCards
{
    /// <summary>Pure rules of the title cards: the heading text and the stepped slide-in / hold / slide-out animation.</summary>
    public static class TitleCardRules
    {
        /// <summary>"Floor 1: The Gatekeeper", or "Final Floor: ..." on the last floor; just the name when there is no floor.</summary>
        public static string IntroTitle(int floor, bool finalFloor, string bossName)
        {
            if (string.IsNullOrEmpty(bossName)) bossName = "???";
            if (floor <= 0) return bossName;
            return finalFloor ? $"Final Floor: {bossName}" : $"Floor {floor}: {bossName}";
        }

        /// <summary>Snaps <paramref name="seconds"/> down to a whole frame of <paramref name="framesPerSecond"/> (held frames).</summary>
        public static float Quantize(float seconds, float framesPerSecond)
        {
            if (framesPerSecond <= 0f) return seconds;
            return Mathf.Floor(seconds * framesPerSecond) / framesPerSecond;
        }

        /// <summary>
        /// Where the card is at <paramref name="time"/>: <c>enter</c> 0 → 1 while it slides in (eased, with a
        /// small overshoot) and stays 1, <c>exit</c> 0 → 1 while it leaves, <c>done</c> once it is gone. Time is
        /// snapped to <paramref name="fps"/>.
        /// </summary>
        public static (float enter, float exit, bool done) Progress(float time, float slideIn, float hold, float slideOut, float fps)
        {
            var t = Quantize(Mathf.Max(0f, time), fps);
            var enter = Mathf.Clamp01(t / Mathf.Max(0.0001f, slideIn));
            var exitStart = slideIn + hold;
            var exit = Mathf.Clamp01((t - exitStart) / Mathf.Max(0.0001f, slideOut));
            return (EaseOutBack(enter), exit * exit, time >= exitStart + slideOut);
        }

        /// <summary>Starts at 0, overshoots 1 a little, settles at 1.</summary>
        public static float EaseOutBack(float x)
        {
            if (x <= 0f) return 0f;
            if (x >= 1f) return 1f;
            const float c1 = 1.2f;
            const float c3 = c1 + 1f;
            var p = x - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }

        /// <summary>The time a skip press jumps to: the start of the exit (never backwards).</summary>
        public static float SkipTime(float time, float slideIn, float hold) => Mathf.Max(time, slideIn + hold);
    }
}
