using UnityEngine;

namespace GraceRun.Core.Environment
{
    /// <summary>Pure rules for the ground shadow: how big and how dark it is at a height above the ground.</summary>
    public static class ShadowMath
    {
        /// <summary>1 on the ground, down to <paramref name="smallest"/> at <paramref name="fadeHeight"/> and above.</summary>
        public static float Scale(float height, float fadeHeight, float smallest) =>
            Mathf.Lerp(1f, smallest, Mathf.Clamp01(height / Mathf.Max(0.01f, fadeHeight)));

        /// <summary>1 on the ground, 0 at <paramref name="fadeHeight"/> and above.</summary>
        public static float Strength(float height, float fadeHeight) =>
            1f - Mathf.Clamp01(height / Mathf.Max(0.01f, fadeHeight));
    }
}
