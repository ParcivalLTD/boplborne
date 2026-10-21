using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>Pure helpers between a 0..1 volume slider and the decibels an AudioMixer expects.</summary>
    public static class VolumeMath
    {
        /// <summary>The mixer's floor: what a slider at 0 maps to (silence).</summary>
        public const float MinDb = -80f;

        /// <summary>Slider 0..1 to decibels: 1 is 0 dB (full), 0.5 is about -6 dB, 0 is <see cref="MinDb"/>.</summary>
        public static float SliderToDb(float slider)
        {
            var value = Mathf.Clamp01(slider);
            if (value <= 0.0001f) return MinDb;
            return Mathf.Max(MinDb, 20f * Mathf.Log10(value));
        }

        /// <summary>Decibels back to a 0..1 slider value. Anything at or below <see cref="MinDb"/> is 0; 0 dB and louder is 1.</summary>
        public static float DbToSlider(float decibels)
        {
            if (decibels <= MinDb) return 0f;
            return Mathf.Clamp01(Mathf.Pow(10f, decibels / 20f));
        }
    }
}
