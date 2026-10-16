using System.Collections.Generic;

namespace GraceRun.Run
{
    /// <summary>Pure rules for picks and rerolls in the draft.</summary>
    public static class DraftRules
    {
        /// <summary>Slot index meaning "no valid slot".</summary>
        public const int NoSlot = -1;

        /// <summary>
        /// Where a picked card goes: the first empty slot if there is one (a pick never replaces an
        /// ability while a slot is free), otherwise <paramref name="requested"/>, the slot the player
        /// chose to replace. <see cref="NoSlot"/> when the request is out of range and every slot is full.
        /// </summary>
        public static int ResolveSlot(int requested, IReadOnlyList<bool> slotEmpty)
        {
            for (var i = 0; i < slotEmpty.Count; i++)
                if (slotEmpty[i]) return i;
            return requested >= 0 && requested < slotEmpty.Count ? requested : NoSlot;
        }

        /// <summary>Rerolls a player still has this run.</summary>
        public static int RerollsLeft(int perRun, int used) => perRun - used > 0 ? perRun - used : 0;
    }
}
