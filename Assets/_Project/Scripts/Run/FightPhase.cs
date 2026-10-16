namespace GraceRun.Run
{
    /// <summary>Where the run/fight loop is: waiting (hub), countdown, fighting, victory, defeat, draft, or rest stop. Synced from the server by <see cref="FightLoop"/>.</summary>
    public enum FightPhase : byte
    {
        Waiting,
        Countdown,
        Fighting,
        Victory,
        Defeat,
        Draft,
        RestStop
    }
}
