using UnityEngine;

namespace GraceRun.Core.Settings
{
    /// <summary>Pure helpers behind the settings screen: stepping values, cycling choices and the labels shown.</summary>
    public static class SettingsRules
    {
        /// <summary>
        /// <paramref name="value"/> moved by <paramref name="steps"/> of <paramref name="step"/>, kept in
        /// <paramref name="min"/>..<paramref name="max"/> and snapped to the step grid (no 0.30000004).
        /// </summary>
        public static float Step(float value, int steps, float step, float min, float max)
        {
            if (step <= 0f) return Mathf.Clamp(value, min, max);
            var snapped = Mathf.Round(value / step + steps) * step;
            return Mathf.Clamp((float)System.Math.Round(snapped, 4), min, max);
        }

        /// <summary>Next index after moving <paramref name="delta"/> through <paramref name="count"/> choices, wrapping around.</summary>
        public static int Cycle(int index, int delta, int count)
        {
            if (count <= 0) return 0;
            var next = (index + delta) % count;
            return next < 0 ? next + count : next;
        }

        /// <summary>"80%" for 0.8.</summary>
        public static string Percent(float value) => $"{Mathf.RoundToInt(value * 100f)}%";

        /// <summary>"On" or "Off".</summary>
        public static string OnOff(bool value) => value ? "On" : "Off";

        /// <summary>"1920 × 1080".</summary>
        public static string ResolutionLabel(int width, int height) => $"{width} × {height}";
    }
}
