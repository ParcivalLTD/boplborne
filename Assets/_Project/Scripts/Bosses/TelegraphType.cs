namespace GraceRun.Bosses
{
    /// <summary>How an attack warns the players during its wind-up (docs/ARCHITECTURE.md).</summary>
    public enum TelegraphType : byte
    {
        /// <summary>Only the red glow on the boss part.</summary>
        Glow,

        /// <summary>Red glow plus a red zone over the area the hitbox will cover.</summary>
        GroundZone,

        /// <summary>Not built yet (later bosses).</summary>
        Line,

        /// <summary>Not built yet (later bosses).</summary>
        Projectile
    }
}
