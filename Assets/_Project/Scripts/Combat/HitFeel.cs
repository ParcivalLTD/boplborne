using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>Pure rules for how a hit should feel: heavy or light, and which way the sparks fly.</summary>
    public static class HitFeel
    {
        /// <summary>A hit is heavy when its knockback is at least <paramref name="threshold"/> units per second.</summary>
        public static bool IsHeavy(Vector2 knockback, float threshold) =>
            threshold > 0f && knockback.sqrMagnitude >= threshold * threshold;

        /// <summary>A crit is a player's hit on a boss while its gold weak point shows.</summary>
        public static bool IsCrit(Team attackerTeam, bool targetIsBoss, bool weakPointOpen) =>
            attackerTeam == Team.Player && targetIsBoss && weakPointOpen;

        /// <summary>
        /// +1 or -1: the knockback's x direction, else away from the attacker, else right.
        /// </summary>
        public static float Direction(Vector2 knockback, float attackerX, float targetX, bool hasAttacker)
        {
            if (Mathf.Abs(knockback.x) > 0.01f) return Mathf.Sign(knockback.x);
            if (hasAttacker && Mathf.Abs(targetX - attackerX) > 0.01f) return Mathf.Sign(targetX - attackerX);
            return 1f;
        }
    }
}
