namespace GraceRun.Bosses
{
    /// <summary>The part of an attack the <see cref="AttackPicker"/> needs: how likely it is and at what range it works.</summary>
    public readonly struct AttackOption
    {
        public readonly float Weight;
        public readonly float MinRange;
        public readonly float MaxRange;

        public AttackOption(float weight, float minRange, float maxRange)
        {
            Weight = weight;
            MinRange = minRange;
            MaxRange = maxRange;
        }

        public static AttackOption From(AttackData data) => new(data.Weight, data.MinRange, data.MaxRange);

        public bool InRange(float distance) => distance >= MinRange && distance <= MaxRange;
    }
}
