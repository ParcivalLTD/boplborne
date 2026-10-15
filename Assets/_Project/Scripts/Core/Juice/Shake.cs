using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Screen shake entry point: <c>Shake.Play(strength)</c>. Local cosmetic only; every PC
    /// triggers its own shake from synced state. Does nothing while no <see cref="ShakeSource"/>
    /// is in the scene (menus, server-only builds).
    /// </summary>
    public static class Shake
    {
        private static ShakeSource _source;

        /// <summary>True while a camera in the loaded scene can shake.</summary>
        public static bool IsAvailable => _source != null;

        /// <summary>Shakes the screen. Strength 1 is a normal heavy hit.</summary>
        public static void Play(float strength)
        {
            if (_source != null) _source.Play(strength);
        }

        /// <summary>Plays the debug test shake from the source's data.</summary>
        public static void PlayTest()
        {
            if (_source != null) _source.Play(_source.TestStrength);
        }

        /// <summary>Impulse force for a shake: strength × settings multiplier × base force, never negative.</summary>
        public static float ComputeForce(float strength, float multiplier, float baseForce) =>
            Mathf.Max(0f, strength) * Mathf.Max(0f, multiplier) * Mathf.Max(0f, baseForce);

        internal static void Register(ShakeSource source) => _source = source;

        internal static void Unregister(ShakeSource source)
        {
            if (_source == source) _source = null;
        }
    }
}
