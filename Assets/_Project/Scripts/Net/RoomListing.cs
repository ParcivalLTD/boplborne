namespace GraceRun.Net
{
    /// <summary>One public room as the browser shows it.</summary>
    public readonly struct RoomListing
    {
        public readonly string Host;
        public readonly string JoinCode;
        public readonly int Players;
        public readonly int MaxPlayers;
        public readonly string Version;
        public readonly bool InFight;

        public RoomListing(string host, string joinCode, int players, int maxPlayers, string version, bool inFight)
        {
            Host = host;
            JoinCode = joinCode;
            Players = players;
            MaxPlayers = maxPlayers;
            Version = version;
            InFight = inFight;
        }
    }
}
