using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Pure rules for the Elden Ring boss posture and stagger system:
    /// posture builds up as players deal damage (especially heavy attacks and abilities),
    /// decays if the boss hasn't been hit for a recovery delay, and when full triggers a posture break
    /// stagger during which the boss is immobilized and incoming damage deals critical bonus damage.
    /// </summary>
    public static class BossPostureRules
    {
        public const float DefaultHeavyMultiplier = 2.5f;
        public const float DefaultCriticalMultiplier = 1.5f;

        /// <summary>Calculates the posture damage inflicted by a landed hit.</summary>
        public static float CalculatePostureDamage(int rawDamage, bool isHeavy, float heavyMultiplier = DefaultHeavyMultiplier, float postureScale = 0.5f)
        {
            if (rawDamage <= 0) return 0f;
            var mult = isHeavy ? Mathf.Max(1f, heavyMultiplier) : 1f;
            return rawDamage * mult * Mathf.Max(0.01f, postureScale);
        }

        /// <summary>Updates current posture over time, decaying if no hits have landed after <paramref name="recoveryDelay"/>.</summary>
        public static float UpdatePosture(float currentPosture, float secondsSinceHit, float recoveryDelay, float recoveryRate, float dt)
        {
            if (currentPosture <= 0f) return 0f;
            if (dt <= 0f || secondsSinceHit < recoveryDelay) return currentPosture;
            return Mathf.Max(0f, currentPosture - (recoveryRate * dt));
        }

        /// <summary>True when posture has reached or exceeded max posture threshold.</summary>
        public static bool IsBroken(float currentPosture, float maxPosture)
        {
            if (maxPosture <= 0f) return true;
            return currentPosture >= maxPosture;
        }

        /// <summary>Calculates incoming damage during stagger (critical hit multiplier).</summary>
        public static int StaggerDamage(int baseDamage, bool isStaggered, float criticalMultiplier = DefaultCriticalMultiplier)
        {
            if (baseDamage <= 0) return 0;
            if (!isStaggered) return baseDamage;
            return Mathf.RoundToInt(baseDamage * Mathf.Max(1f, criticalMultiplier));
        }

        /// <summary>Normalized 0..1 posture progress toward breaking.</summary>
        public static float Normalized(float current, float max)
        {
            if (max <= 0f) return 0f;
            return Mathf.Clamp01(current / max);
        }
    }
}
