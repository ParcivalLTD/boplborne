using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Pure rules that map a player's movement and combat state onto the ten animation clip tags
    /// defined in <c>docs/ART_PIPELINE.md</c> and horizontal sprite flip.
    /// </summary>
    public static class PlayerAnimationRules
    {
        public const string Idle = "idle";
        public const string Run = "run";
        public const string Jump = "jump";
        public const string Fall = "fall";
        public const string Dodge = "dodge";
        public const string Attack1 = "attack1";
        public const string Attack2 = "attack2";
        public const string Attack3 = "attack3";
        public const string Hurt = "hurt";
        public const string Ghost = "ghost";

        /// <summary>
        /// Picks the active animation clip name by priority:
        /// ghost &gt; hurt &gt; dodge &gt; attack1–3 &gt; jump / fall &gt; run &gt; idle.
        /// </summary>
        public static string ResolveState(
            bool isGhost,
            bool isHurt,
            bool isDodging,
            bool isSwinging,
            int attackIndex,
            bool isGrounded,
            float horizontalSpeed,
            float verticalVelocity,
            float runSpeedThreshold,
            float airSpeedThreshold)
        {
            if (isGhost) return Ghost;
            if (isHurt) return Hurt;
            if (isDodging) return Dodge;
            if (isSwinging) return AttackStateFor(attackIndex);
            if (!isGrounded) return verticalVelocity > airSpeedThreshold ? Jump : Fall;
            return Mathf.Abs(horizontalSpeed) >= runSpeedThreshold ? Run : Idle;
        }

        /// <summary>Maps combo hit index (0, 1, 2) to <c>attack1</c>, <c>attack2</c> or <c>attack3</c>.</summary>
        public static string AttackStateFor(int attackIndex) => attackIndex switch
        {
            1 => Attack2,
            2 => Attack3,
            _ => Attack1
        };

        /// <summary>Updates facing (+1 right, -1 left) when horizontal velocity exceeds <paramref name="flipThreshold"/>.</summary>
        public static float ResolveFacing(float currentFacing, float horizontalVelocity, float flipThreshold)
        {
            if (horizontalVelocity > flipThreshold) return 1f;
            if (horizontalVelocity < -flipThreshold) return -1f;
            return currentFacing < 0f ? -1f : 1f;
        }
    }
}
