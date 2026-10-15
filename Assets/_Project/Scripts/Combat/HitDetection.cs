namespace GraceRun.Combat
{
    /// <summary>Which PC decides that a hitbox hit (docs/NETWORKING.md).</summary>
    public enum HitDetection : byte
    {
        /// <summary>Player attacks: the attacker's PC detects and reports.</summary>
        AttackerSide,

        /// <summary>Boss attacks: the victim's PC detects hits on its own player, unless invulnerable.</summary>
        VictimSide
    }
}
