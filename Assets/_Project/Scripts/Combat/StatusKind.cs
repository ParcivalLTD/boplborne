namespace GraceRun.Combat
{
    /// <summary>A timed status an ability hit can put on a player, the dummy or a boss (see <see cref="StatusEffects"/>).</summary>
    public enum StatusKind
    {
        None,
        /// <summary>Moves slower.</summary>
        Slow,
        /// <summary>Smaller (harder to hit) and deals less damage.</summary>
        Shrink,
    }
}
