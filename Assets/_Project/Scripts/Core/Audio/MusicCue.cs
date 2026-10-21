namespace GraceRun.Core.Audio
{
    /// <summary>Which music the game wants right now (the hub and main menu share one track).</summary>
    public enum MusicCue : byte
    {
        Hub,
        Boss,
        Rest,
        Victory,
        Defeat
    }
}
