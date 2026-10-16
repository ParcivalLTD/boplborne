using GraceRun.Abilities;

namespace GraceRun.Run
{
    /// <summary>One ability the draft may deal, as plain data: its catalog id, rarity and first floor.</summary>
    public readonly struct DraftCandidate
    {
        public readonly int Id;
        public readonly AbilityRarity Rarity;
        public readonly int MinFloor;

        public DraftCandidate(int id, AbilityRarity rarity, int minFloor = 1)
        {
            Id = id;
            Rarity = rarity;
            MinFloor = minFloor;
        }
    }
}
