namespace GraceRun.Player
{
    /// <summary>The kinds of attack a weapon can have.</summary>
    public enum WeaponMove : byte
    {
        /// <summary>The chained light combo (neutral, on the ground).</summary>
        Light,
        Heavy,
        /// <summary>Light attack while airborne.</summary>
        Aerial,
        /// <summary>Light attack while holding up.</summary>
        Up,
        /// <summary>Light attack while holding down.</summary>
        Down,
    }
}
