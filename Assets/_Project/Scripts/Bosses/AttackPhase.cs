namespace GraceRun.Bosses
{
    /// <summary>Where a boss attack is on its timeline. Every PC computes it from the shared start tick.</summary>
    public enum AttackPhase : byte
    {
        None,

        /// <summary>Started on the server, but this PC has not reached the start tick yet.</summary>
        Pending,

        /// <summary>Telegraph: red glow (and zone). Dodge now.</summary>
        Windup,

        /// <summary>The hitbox is armed.</summary>
        Active,

        /// <summary>Punish window: the gold weak point shows.</summary>
        Recovery
    }
}
