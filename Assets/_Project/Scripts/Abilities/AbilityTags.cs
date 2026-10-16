using System;

namespace GraceRun.Abilities
{
    /// <summary>
    /// What kind of thing an ability is. Combos come only from tags meeting tags
    /// (looked up in <c>ComboRules</c>), never from two named abilities. An ability can have several.
    /// </summary>
    [Flags]
    public enum AbilityTags
    {
        None = 0,
        Projectile = 1 << 0,
        Movement = 1 << 1,
        Area = 1 << 2,
        Summon = 1 << 3,
    }
}
