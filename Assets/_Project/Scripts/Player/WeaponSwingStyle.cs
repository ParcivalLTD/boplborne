namespace GraceRun.Player
{
    /// <summary>How a weapon's art moves during a swing (visual only, the hitbox comes from the hit data).</summary>
    public enum WeaponSwingStyle : byte
    {
        /// <summary>Raised behind the head, then swept down through the aim (sword, hammer).</summary>
        Arc,

        /// <summary>Pulled back, then pushed straight out along the aim (spear, fists).</summary>
        Thrust,
    }
}
