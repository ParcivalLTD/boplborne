using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>What the leader starts from the hub.</summary>
    public enum GameMode : byte
    {
        /// <summary>The normal run through the floors.</summary>
        Run,

        /// <summary>One chosen boss, unlimited tries, no draft and no run progress.</summary>
        Practice,

        /// <summary>Team versus team in the arena, first to a number of rounds.</summary>
        Arena
    }

    /// <summary>
    /// Pure rules for the hub mode choice. The leader cycles one list: Run, then Practice for each
    /// boss in turn, then Arena. A selection is an index into that list.
    /// </summary>
    public static class ModeRules
    {
        /// <summary>Number of entries: Run, every practice boss, Arena.</summary>
        public static int OptionCount(int bossCount) => 2 + Mathf.Max(0, bossCount);

        public static GameMode ModeOf(int selection, int bossCount)
        {
            if (selection <= 0) return GameMode.Run;
            return selection > Mathf.Max(0, bossCount) ? GameMode.Arena : GameMode.Practice;
        }

        /// <summary>Index of the practice boss for <paramref name="selection"/>, or -1 outside Practice.</summary>
        public static int BossIndexOf(int selection, int bossCount) =>
            ModeOf(selection, bossCount) == GameMode.Practice ? selection - 1 : -1;

        /// <summary>The selection <paramref name="delta"/> steps away, wrapping around the list.</summary>
        public static int Step(int selection, int delta, int bossCount)
        {
            var count = OptionCount(bossCount);
            return ((selection + delta) % count + count) % count;
        }
    }
}
