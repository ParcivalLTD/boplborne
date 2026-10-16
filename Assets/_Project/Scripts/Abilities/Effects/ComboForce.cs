using GraceRun.Combat;
using GraceRun.Core;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Asks <c>ComboRules</c> whether an area effect's force reaches something that is not its plain
    /// target: a projectile flying through it, or a teammate it catches. The answer is a force
    /// multiplier (the rule's strength); no TakeForce rule means the force does not reach it.
    /// </summary>
    public static class ComboForce
    {
        /// <summary>
        /// Force multiplier for <paramref name="body"/>: 1 for non-players (the dummy, enemies);
        /// for a teammate (another player) only if an Area → Teammate rule allows it.
        /// </summary>
        public static bool OnBody(AbilityEffect area, Knockback body, out float strength)
        {
            strength = 1f;
            if (body.gameObject.layer != Layers.PlayerIndex) return true;
            return OnTeammate(area, out strength);
        }

        /// <summary>Force multiplier for <paramref name="target"/> (a blast): teammates need a rule, others always.</summary>
        public static bool OnTarget(AbilityEffect area, Health target, out float strength)
        {
            strength = 1f;
            if (target.Team != Team.Player) return true;
            return OnTeammate(area, out strength);
        }

        /// <summary>Force multiplier for a projectile passing through the area (projectile → area rule).</summary>
        public static bool OnProjectile(AbilityEffect area, Projectile projectile, out float strength)
        {
            strength = 0f;
            if (projectile.Effect == area) return false;
            var source = projectile.Effect;
            if (!ComboRules.Match(source.Combos, source.Tags, area.Tags, ComboInteraction.TakeForce, out var rule)) return false;
            strength = rule.Strength;
            return true;
        }

        private static bool OnTeammate(AbilityEffect area, out float strength)
        {
            strength = 0f;
            if (!ComboRules.Match(area.Combos, area.Tags, ComboSubject.Teammate, ComboInteraction.TakeForce, out var rule)) return false;
            strength = rule.Strength;
            return true;
        }
    }
}
