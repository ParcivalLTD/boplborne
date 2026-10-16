using System;

namespace GraceRun.Abilities
{
    /// <summary>
    /// What an ability can meet in a combo rule: the same bits as <see cref="AbilityTags"/>, plus
    /// <see cref="Teammate"/> for an effect catching another player.
    /// </summary>
    [Flags]
    public enum ComboSubject
    {
        None = 0,
        Projectile = AbilityTags.Projectile,
        Movement = AbilityTags.Movement,
        Area = AbilityTags.Area,
        Summon = AbilityTags.Summon,
        Teammate = 1 << 4,
    }
}
