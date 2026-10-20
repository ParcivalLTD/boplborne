using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Pure logic and rules for The Clockwork Colossus:
    /// back-hit detection, back weak-point posture damage calculation,
    /// conveyor belt velocity cycles, and floor detection.
    /// </summary>
    public static class ClockworkColossusRules
    {
        public const float DefaultBackMultiplier = 2.0f;
        public const float DefaultFloorTolerance = 0.5f;

        /// <summary>True when the attacker's X position is behind the boss's current facing direction.</summary>
        public static bool IsBackHit(float attackerX, float bossX, float facing)
        {
            if (facing == 0f) return false;
            var relativeX = attackerX - bossX;
            return (relativeX * facing) < -0.1f;
        }

        /// <summary>Calculates bonus posture damage inflicted on the Colossus when hitting its back weak point.</summary>
        public static float CalculateBackHitPostureDamage(int rawDamage, bool isBackHit, float multiplier = DefaultBackMultiplier)
        {
            if (rawDamage <= 0) return 0f;
            var mult = isBackHit ? Mathf.Max(1f, multiplier) : 1f;
            return rawDamage * mult;
        }

        /// <summary>Calculates alternating conveyor belt horizontal speed over time.</summary>
        public static float ConveyorVelocity(float baseSpeed, float time, float interval)
        {
            if (baseSpeed == 0f || interval <= 0f) return baseSpeed;
            var cycle = Mathf.FloorToInt(Mathf.Max(0f, time) / interval);
            return (cycle % 2 == 0) ? baseSpeed : -baseSpeed;
        }

        /// <summary>True when a player's vertical position is on the arena floor rather than elevated platforms.</summary>
        public static bool IsOnFloor(float playerY, float floorTop, float tolerance = DefaultFloorTolerance)
        {
            return playerY <= (floorTop + tolerance);
        }
    }
}
