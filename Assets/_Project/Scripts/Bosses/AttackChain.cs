namespace GraceRun.Bosses
{
    /// <summary>
    /// Follows an attack's <see cref="AttackData.FollowUp"/> chain: which link plays at a given
    /// time since the first link's start, and how far into that link it is. Every PC computes
    /// the same answer from the shared start tick, so chained strikes need no extra message.
    /// </summary>
    public static class AttackChain
    {
        /// <summary>Most links followed before giving up, so a follow-up loop can't hang the game.</summary>
        public const int MaxLinks = 8;

        /// <summary>
        /// Walks from <paramref name="first"/> past every finished link that has a follow-up.
        /// Returns the link playing at <paramref name="elapsed"/> (the last link once all are over)
        /// and sets <paramref name="linkStart"/> to its start, in seconds after the first link's start.
        /// </summary>
        public static AttackData LinkAt(AttackData first, double elapsed, out double linkStart)
        {
            linkStart = 0d;
            var link = first;
            for (var i = 0; link != null && i < MaxLinks; i++)
            {
                if (link.FollowUp == null || elapsed - linkStart < link.Duration) break;
                linkStart += link.Duration;
                link = link.FollowUp;
            }

            return link;
        }
    }
}
