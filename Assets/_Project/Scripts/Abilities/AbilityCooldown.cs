using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Pure cooldown math on server time. The owner checks with no tolerance for instant feedback;
    /// the server re-checks with a small tolerance so clock drift never eats a fair press.
    /// </summary>
    public static class AbilityCooldown
    {
        /// <summary>True when the ability may be used at <paramref name="now"/>.</summary>
        public static bool IsReady(double readyAt, double now, float tolerance = 0f) =>
            now + Mathf.Max(0f, tolerance) >= readyAt;

        /// <summary>Server time the ability is ready again after a use at <paramref name="now"/>.</summary>
        public static double NextReadyAt(double now, float cooldown) => now + Mathf.Max(0f, cooldown);

        /// <summary>Cooldown left as 1 (just used) to 0 (ready), for the HUD shade.</summary>
        public static float Remaining01(double readyAt, float cooldown, double now)
        {
            if (cooldown <= 0f) return 0f;
            return Mathf.Clamp01((float)((readyAt - now) / cooldown));
        }
    }
}
