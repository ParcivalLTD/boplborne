namespace GraceRun.Run
{
    /// <summary>Pure rules for the hub ready-up.</summary>
    public static class ReadyRules
    {
        /// <summary>True when at least one player is connected and all of them are ready.</summary>
        public static bool AllReady(int connected, int ready) => connected > 0 && ready >= connected;
    }
}
