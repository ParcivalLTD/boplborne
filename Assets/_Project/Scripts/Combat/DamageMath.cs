using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// The pure damage rules, kept free of Unity objects so they can be tested in EditMode.
    /// Friendly fire (docs/GDD.md): a teammate's hit does a fraction of its damage and can never
    /// take you below a minimum HP.
    /// </summary>
    public static class DamageMath
    {
        /// <summary>True when one player hits another player. Hitting yourself is not friendly fire.</summary>
        public static bool IsFriendlyFire(Team attackerTeam, ulong attackerId, Team targetTeam, ulong targetId) =>
            attackerTeam == Team.Player && targetTeam == Team.Player && attackerId != targetId;

        /// <summary>True when the rules drop a hit completely: friendly fire while it is switched off.</summary>
        public static bool IsDropped(bool friendlyFireEnabled, bool friendlyFire) => friendlyFire && !friendlyFireEnabled;

        /// <summary>Damage after the friendly-fire multiplier. A friendly hit still does at least 1 unless the multiplier is 0.</summary>
        public static int DamageDealt(int amount, bool friendlyFire, float friendlyFireMultiplier)
        {
            if (amount <= 0) return 0;
            if (!friendlyFire) return amount;
            if (friendlyFireMultiplier <= 0f) return 0;
            return Mathf.Max(1, Mathf.RoundToInt(amount * friendlyFireMultiplier));
        }

        /// <summary>HP after taking <paramref name="damage"/>, never below <paramref name="floor"/>. A hit never heals.</summary>
        public static int HpAfterHit(int current, int damage, int floor)
        {
            if (damage <= 0 || current <= floor) return current;
            return Mathf.Max(floor, current - damage);
        }

        /// <summary>The whole rule set: god mode, friendly-fire scaling and the friendly-fire HP floor.</summary>
        public static int Resolve(int current, int amount, bool friendlyFire, float friendlyFireMultiplier,
            int friendlyFireMinHP, bool godMode)
        {
            if (godMode) return current;
            var damage = DamageDealt(amount, friendlyFire, friendlyFireMultiplier);
            return HpAfterHit(current, damage, friendlyFire ? friendlyFireMinHP : 0);
        }
    }
}
