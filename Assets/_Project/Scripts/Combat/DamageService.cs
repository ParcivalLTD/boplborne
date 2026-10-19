using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Server side of the damage flow (docs/ARCHITECTURE.md): resolves a reported hit with the
    /// friendly-fire rules, writes the target's HP, sends knockback to the target's owner and
    /// tells the other PCs to play the hit feedback.
    /// </summary>
    public static class DamageService
    {
        /// <summary>True when the rules drop this hit completely (friendly fire while it is off). Any PC.
        /// A friendly hit that gets through keeps its full knockback; only the damage is scaled.</summary>
        public static bool IsIgnored(Health target, DamageInfo info) =>
            DamageMath.IsDropped(target.Combat.FriendlyFireEnabled,
                DamageMath.IsFriendlyFire(info.AttackerTeam, info.AttackerId, target.Team, target.NetworkObjectId));

        /// <summary>Server only. Ignores hits on dead targets. <paramref name="reporterClientId"/> already played the feedback.</summary>
        public static void Apply(Health target, DamageInfo info, ulong reporterClientId)
        {
            if (target == null || !target.IsServer || target.IsDead || IsIgnored(target, info)) return;

            var combat = target.Combat;
            var friendly = DamageMath.IsFriendlyFire(info.AttackerTeam, info.AttackerId, target.Team, target.NetworkObjectId);
            var buff = Run.RunManager.Instance != null ? Run.RunManager.Instance.ActiveBuff : Run.PartyBuff.None;
            var amount = Run.RestStopRules.BuffedDamage(info.Amount, friendly, info.AttackerTeam, buff);
            if (amount > 0 && StatusEffects.TryFind(info.AttackerId, out var attackerStatus))
                amount = DamageMath.Scaled(amount, attackerStatus.OutgoingDamageMultiplier);

            var hp = DamageMath.Resolve(target.Current, amount, friendly,
                combat.FriendlyFireDamageMultiplier, combat.FriendlyFireMinHP, target.GodMode);
            var dealt = target.Current - hp;
            target.ServerSetCurrent(hp);

            if (info.Knockback != Vector2.zero && target.TryGetComponent(out Knockback knockback))
                knockback.PushRpc(info.Knockback);

            target.ServerBroadcastHit(info, reporterClientId);

            var run = Run.RunManager.Instance;
            if (run != null && run.IsSpawned)
            {
                if (friendly)
                {
                    if (info.Knockback != Vector2.zero) run.ServerRecordFriendlyKnockback(info.AttackerId);
                }
                else if (info.AttackerTeam == Team.Player && dealt > 0)
                {
                    run.ServerRecordDamage(info.AttackerId, dealt);
                }
            }
        }
    }
}
