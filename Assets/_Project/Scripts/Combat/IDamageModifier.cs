namespace GraceRun.Combat
{
    /// <summary>
    /// Server only: a target that changes the damage it takes (a boss's weak point, stagger, adaptation).
    /// <see cref="DamageService"/> calls it for every non-friendly hit on a target that has one.
    /// </summary>
    public interface IDamageModifier
    {
        /// <summary>The damage to apply instead of <paramref name="amount"/> (after party buffs).</summary>
        int ModifyIncoming(int amount, DamageInfo info);
    }
}
