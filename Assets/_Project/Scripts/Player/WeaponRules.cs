using GraceRun.Run;

namespace GraceRun.Player
{
    /// <summary>Pure rule: when a player may change weapon.</summary>
    public static class WeaponRules
    {
        /// <summary>Anywhere in the hub (starter draft, then waiting to start the run) and at a rest stop. Never during a fight or a later draft.</summary>
        public static bool CanChange(FightPhase phase, bool inHub) =>
            phase == FightPhase.RestStop || (inHub && (phase == FightPhase.Draft || phase == FightPhase.Waiting));
    }
}
