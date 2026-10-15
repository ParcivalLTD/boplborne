namespace GraceRun.Core
{
    /// <summary>
    /// Physics layer names and indices. Indices must match Project Settings > Tags and Layers;
    /// the EditMode test LayersTests checks that they do. Never type layer names as strings elsewhere.
    /// </summary>
    public static class Layers
    {
        public const string Ground = "Ground";
        public const string Player = "Player";
        public const string Ghost = "Ghost";
        public const string Boss = "Boss";
        public const string PlayerAttack = "PlayerAttack";
        public const string BossAttack = "BossAttack";
        public const string Ability = "Ability";

        public const int GroundIndex = 8;
        public const int PlayerIndex = 9;
        public const int GhostIndex = 10;
        public const int BossIndex = 11;
        public const int PlayerAttackIndex = 12;
        public const int BossAttackIndex = 13;
        public const int AbilityIndex = 14;

        /// <summary>Layer mask containing only the given layer index.</summary>
        public static int Mask(int layerIndex) => 1 << layerIndex;
    }
}
