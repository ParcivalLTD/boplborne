using System.Collections.Generic;

namespace GraceRun.Run
{
    /// <summary>Result of one Arena round.</summary>
    public enum ArenaRound : byte
    {
        /// <summary>Both teams still have someone standing.</summary>
        Undecided,
        TeamA,
        TeamB,
        /// <summary>Everyone fell in the same moment.</summary>
        Draw
    }

    /// <summary>
    /// Pure rules of the team Arena: two teams by player slot (P1 and P3 against P2 and P4), a round
    /// ends when a team has nobody standing, the first team to a number of round wins takes the match.
    /// </summary>
    public static class ArenaRules
    {
        /// <summary>0 or 1: slots 0 and 2 are team A, slots 1 and 3 are team B.</summary>
        public static int TeamOf(int slot) => slot & 1;

        public static bool IsOpposing(int slotA, int slotB) => TeamOf(slotA) != TeamOf(slotB);

        /// <summary>True when the players in <paramref name="slots"/> include both teams (an Arena match needs it).</summary>
        public static bool HasBothTeams(IEnumerable<int> slots)
        {
            var a = false;
            var b = false;
            foreach (var slot in slots)
            {
                if (TeamOf(slot) == 0) a = true;
                else b = true;
            }
            return a && b;
        }

        /// <summary>Who won the round given how many players of each team are still up.</summary>
        public static ArenaRound RoundResult(int livingA, int livingB)
        {
            if (livingA > 0 && livingB > 0) return ArenaRound.Undecided;
            if (livingA <= 0 && livingB <= 0) return ArenaRound.Draw;
            return livingA > 0 ? ArenaRound.TeamA : ArenaRound.TeamB;
        }

        /// <summary>0 or 1 when that team has <paramref name="winsNeeded"/> round wins (the higher score wins a tie-break), else -1.</summary>
        public static int MatchWinner(int scoreA, int scoreB, int winsNeeded)
        {
            if (winsNeeded <= 0) return -1;
            if (scoreA < winsNeeded && scoreB < winsNeeded) return -1;
            return scoreA >= scoreB ? 0 : 1;
        }

        /// <summary>Spawn x for a player: team A on the left, team B on the right, members spread by <paramref name="spacing"/>.</summary>
        public static float SpawnX(int slot, float edge, float spacing) =>
            (TeamOf(slot) == 0 ? -1f : 1f) * (edge + (slot / 2) * spacing);
    }
}
