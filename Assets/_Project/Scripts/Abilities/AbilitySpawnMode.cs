namespace GraceRun.Abilities
{
    /// <summary>Where an ability's effect appears when it is used.</summary>
    public enum AbilitySpawnMode
    {
        /// <summary>At the caster, plus the ability's spawn offset turned toward the aim.</summary>
        AtCaster,
        /// <summary>At the aim point: the cursor, at most the ability's range away (keys: the full range).</summary>
        AtAimPoint,
        /// <summary>Drop height above the aim point, launched straight down (meteors).</summary>
        AboveAimPoint,
    }
}
