using UnityEngine;

namespace GraceRun.UI.Toon
{
    /// <summary>A damped spring for squash, stretch and wobble. Pure maths, so it is tested in EditMode.</summary>
    public struct Spring
    {
        public float Value;
        public float Velocity;

        public Spring(float value)
        {
            Value = value;
            Velocity = 0f;
        }

        /// <summary>Moves toward <paramref name="target"/>. A low damping ratio overshoots (cartoon bounce), 1 does not.</summary>
        public void Step(float target, float dt, float frequency = 9f, float damping = 0.38f)
        {
            // Substep so a long frame (a hitch while the menu is open) cannot blow the spring up.
            const float maxStep = 1f / 120f;
            var remaining = Mathf.Min(dt, 0.1f);
            while (remaining > 0f)
            {
                var h = Mathf.Min(remaining, maxStep);
                var omega = frequency * 2f * Mathf.PI;
                var accel = -omega * omega * (Value - target) - 2f * damping * omega * Velocity;
                Velocity += accel * h;
                Value += Velocity * h;
                remaining -= h;
            }
        }

        public bool IsAt(float target, float tolerance = 0.002f) =>
            Mathf.Abs(Value - target) < tolerance && Mathf.Abs(Velocity) < tolerance;
    }

    public static class ToonMotion
    {
        /// <summary>Holds each value for 1/fps seconds, so motion looks drawn "on twos" instead of smooth.</summary>
        public static float Stepped(float time, float fps) => Mathf.Floor(time * fps) / fps;

        /// <summary>A bouncy pop-in: 0 at t = 0, overshoots to about 1.15, settles at 1 at t = 1.</summary>
        public static float PopCurve(float t)
        {
            t = Mathf.Clamp01(t);
            if (t >= 1f) return 1f;
            // easeOutBack
            const float c1 = 2.2f;
            const float c3 = c1 + 1f;
            var x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
