namespace GraceRun.Run
{
    /// <summary>Where the single-fight loop is. After a victory comes the draft, then waiting again. Synced from the server by <see cref="FightLoop"/>.</summary>
    public enum FightPhase : byte
    {
        Waiting,
        Countdown,
        Fighting,
        Victory,
        Defeat,
        Draft
    }
}
