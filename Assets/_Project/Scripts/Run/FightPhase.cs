namespace GraceRun.Run
{
    /// <summary>Where the single-fight loop is. Synced from the server by <see cref="FightLoop"/>.</summary>
    public enum FightPhase : byte
    {
        Waiting,
        Countdown,
        Fighting,
        Victory,
        Defeat
    }
}
