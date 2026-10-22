using UnityEngine;

namespace GraceRun.UI
{
    /// <summary>Pure maths for the HP bar's trailing "chip" and its low-HP colour.</summary>
    public static class HealthBarRules
    {
        /// <summary>
        /// Next chip fraction: jumps up with the HP, and after <paramref name="hold"/> seconds
        /// without a new hit drains toward the HP at <paramref name="drainPerSecond"/>.
        /// </summary>
        public static float Chip(float chip, float hp, float hold, float drainPerSecond, float deltaTime)
        {
            if (hp >= chip) return hp;
            if (hold > 0f) return chip;
            return Mathf.Max(hp, chip - drainPerSecond * deltaTime);
        }

        /// <summary>The hold timer after this frame: restarts when the HP dropped, otherwise counts down.</summary>
        public static float Hold(float hold, float previousHp, float hp, float holdSeconds, float deltaTime) =>
            hp < previousHp ? holdSeconds : Mathf.Max(0f, hold - deltaTime);

        /// <summary>Fill colour: <paramref name="full"/> above <paramref name="warn"/>, then blends to <paramref name="low"/> at <paramref name="critical"/>.</summary>
        public static Color FillColor(float hp, float warn, float critical, Color full, Color low)
        {
            if (hp >= warn) return full;
            var t = Mathf.InverseLerp(warn, critical, hp);
            return Color.Lerp(full, low, t);
        }
    }
}
