namespace GraceRun.Abilities
{
    /// <summary>What happens when a combo rule matches. The effect that meets the other thing carries it out.</summary>
    public enum ComboInteraction
    {
        /// <summary>No combo: the two ignore each other.</summary>
        None,
        /// <summary>The other thing takes this area's force (pulled by a Black Hole, pushed by a Gust, launched by a blast).</summary>
        TakeForce,
        /// <summary>A movement ability can anchor on it (grapple onto a summon).</summary>
        Latch,
        /// <summary>A projectile bounces off it.</summary>
        Ricochet,
        /// <summary>A projectile is swallowed without an impact.</summary>
        Absorb,
    }
}
