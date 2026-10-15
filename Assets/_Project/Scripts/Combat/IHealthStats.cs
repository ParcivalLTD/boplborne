namespace GraceRun.Combat
{
    /// <summary>A data asset that knows a max HP. <see cref="Health"/> reads its max HP through this.</summary>
    public interface IHealthStats
    {
        int MaxHP { get; }
    }
}
