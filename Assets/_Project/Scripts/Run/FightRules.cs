using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Pure rules of the single-fight loop: when a fight is won or lost, what the countdown shows,
    /// and which player may press Start / Restart.
    /// </summary>
    public static class FightRules
    {
        /// <summary>Leader id when nobody is connected.</summary>
        public const ulong NoLeader = ulong.MaxValue;

        /// <summary>
        /// Outcome of a running fight. A dead boss wins even if the last player fell in the same
        /// moment. Defeat needs at least one player and none of them alive. Otherwise the fight goes on.
        /// </summary>
        public static FightPhase Outcome(bool bossDefeated, int players, int livingPlayers)
        {
            if (bossDefeated) return FightPhase.Victory;
            if (players > 0 && livingPlayers <= 0) return FightPhase.Defeat;
            return FightPhase.Fighting;
        }

        /// <summary>The number to show for <paramref name="remaining"/> seconds left: 3, 2, 1, then 0.</summary>
        public static int CountdownNumber(double remaining) =>
            remaining <= 0d ? 0 : Mathf.CeilToInt((float)remaining);

        /// <summary>
        /// Who controls the fight: the host's own player when there is a host, otherwise the
        /// connected client with the lowest id (server-only builds), or <see cref="NoLeader"/>.
        /// </summary>
        public static ulong Leader(bool isHost, ulong hostClientId, IReadOnlyList<ulong> connected)
        {
            if (isHost) return hostClientId;
            var leader = NoLeader;
            foreach (var id in connected)
                if (id < leader) leader = id;
            return leader;
        }
    }
}
