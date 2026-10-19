namespace GraceRun.Player
{
    /// <summary>Pure rule: which <see cref="WeaponMove"/> a press asks for.</summary>
    public static class WeaponMoves
    {
        /// <summary>
        /// The Heavy button always gives Heavy. The light button gives Up / Down when the move
        /// stick points that way past <paramref name="threshold"/>, Aerial while airborne, else the Light combo.
        /// </summary>
        public static WeaponMove Select(bool heavy, bool grounded, float moveY, float threshold)
        {
            if (heavy) return WeaponMove.Heavy;
            if (moveY >= threshold) return WeaponMove.Up;
            if (moveY <= -threshold) return WeaponMove.Down;
            return grounded ? WeaponMove.Light : WeaponMove.Aerial;
        }
    }
}
