namespace GraceRun.Player
{
    /// <summary>Which body animation a weapon swing uses. Every weapon has its own set of them (<see cref="WeaponBodyPoses"/>).</summary>
    public enum BodyMove : byte
    {
        LightA,
        LightB,

        /// <summary>The heavy attack and the last hit of the light combo.</summary>
        Heavy,
        Aerial,
        Up,
        Down,
    }

    /// <summary>Picks the <see cref="BodyMove"/> for a swing. Pure, so it is tested in EditMode.</summary>
    public static class BodyMoves
    {
        public static BodyMove Pick(WeaponMove move, bool finisher, int hitIndex)
        {
            switch (move)
            {
                case WeaponMove.Heavy: return BodyMove.Heavy;
                case WeaponMove.Aerial: return BodyMove.Aerial;
                case WeaponMove.Up: return BodyMove.Up;
                case WeaponMove.Down: return BodyMove.Down;
                default:
                    if (finisher) return BodyMove.Heavy;
                    return hitIndex % 2 == 0 ? BodyMove.LightA : BodyMove.LightB;
            }
        }
    }
}
