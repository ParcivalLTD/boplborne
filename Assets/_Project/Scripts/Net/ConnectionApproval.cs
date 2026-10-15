namespace GraceRun.Net
{
    /// <summary>Outcome of checking a connection request; anything but Approved is refused.</summary>
    public enum ApprovalResult
    {
        Approved,
        BadPayload,
        VersionMismatch,
        ServerFull,
        FightInProgress
    }

    /// <summary>
    /// The connection rules from docs/NETWORKING.md: same game version, at most MaxPlayers,
    /// no joining while a fight is running. Pure logic, covered by EditMode tests.
    /// </summary>
    public static class ConnectionApproval
    {
        public static ApprovalResult Evaluate(
            string clientVersion, string serverVersion, int connectedClients, int maxPlayers, bool fightInProgress)
        {
            if (string.IsNullOrEmpty(clientVersion)) return ApprovalResult.BadPayload;
            if (clientVersion != serverVersion) return ApprovalResult.VersionMismatch;
            if (connectedClients >= maxPlayers) return ApprovalResult.ServerFull;
            if (fightInProgress) return ApprovalResult.FightInProgress;
            return ApprovalResult.Approved;
        }

        /// <summary>Message the refused client sees in the main menu.</summary>
        public static string ReasonText(ApprovalResult result, string serverVersion, string clientVersion) => result switch
        {
            ApprovalResult.Approved => null,
            ApprovalResult.BadPayload => "The host could not read your connection request.",
            ApprovalResult.VersionMismatch => $"Host is on version {serverVersion}, you are on {clientVersion}.",
            ApprovalResult.ServerFull => "The game is full.",
            ApprovalResult.FightInProgress => "Wait for the fight to end.",
            _ => "Connection refused."
        };
    }
}
