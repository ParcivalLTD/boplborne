using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Server side of the damage flow (docs/ARCHITECTURE.md): resolves a reported hit with the
    /// friendly-fire rules, writes the target's HP and sends knockback to the target's owner.
    /// </summary>
    public static class DamageService
    {
        /// <summary>Server only. Ignores hits on dead targets.</summary>
        public static void Apply(Health target, DamageInfo info)
        {
            if (target == null || !target.IsServer || target.IsDead) return;

            var combat = target.Combat;
            var friendly = DamageMath.IsFriendlyFire(info.AttackerTeam, info.AttackerId, target.Team, target.NetworkObjectId);
            if (friendly && !combat.FriendlyFireEnabled) return;

            var hp = DamageMath.Resolve(target.Current, info.Amount, friendly,
                combat.FriendlyFireDamageMultiplier, combat.FriendlyFireMinHP, target.GodMode);
            target.ServerSetCurrent(hp);

            if (info.Knockback != Vector2.zero && target.TryGetComponent(out Knockback knockback))
                knockback.PushRpc(info.Knockback);
        }
    }
}
