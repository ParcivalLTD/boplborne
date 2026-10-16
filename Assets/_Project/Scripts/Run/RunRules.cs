using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Pure rules for a run across floors (docs/GDD.md, Run structure): which boss a floor picks,
    /// when a floor is the final one, which floors lead to a rest stop, and when the leader may
    /// advance the run.
    /// </summary>
    public static class RunRules
    {
        /// <summary>Floor number used while the party is in the hub before floor 1.</summary>
        public const int HubFloor = 0;

        /// <summary>True when <paramref name="floor"/> is the last floor of the run.</summary>
        public static bool IsFinalFloor(int floor, int totalFloors) =>
            totalFloors > 0 && floor >= totalFloors;

        /// <summary>
        /// True when a rest stop comes after <paramref name="floor"/> (and its draft). There is
        /// never a rest stop in the hub or after the final floor.
        /// </summary>
        public static bool HasRestStopAfter(int floor, int totalFloors, IReadOnlyList<int> restAfterFloors)
        {
            if (floor <= HubFloor || IsFinalFloor(floor, totalFloors) || restAfterFloors == null) return false;
            for (var i = 0; i < restAfterFloors.Count; i++)
                if (restAfterFloors[i] == floor) return true;
            return false;
        }

        /// <summary>Picks an index in <c>0..poolSize-1</c> from a <c>[0, 1)</c> roll, or -1 when the pool is empty.</summary>
        public static int PickBossIndex(int poolSize, double roll01)
        {
            if (poolSize <= 0) return -1;
            var clamped = roll01 < 0d ? 0d : roll01 >= 1d ? 0.999999d : roll01;
            return Mathf.Clamp((int)(clamped * poolSize), 0, poolSize - 1);
        }

        /// <summary>Deterministic seed for <paramref name="floor"/> derived from the run seed.</summary>
        public static int FloorSeed(int runSeed, int floor) => unchecked(runSeed * 397 ^ floor);

        /// <summary>
        /// True when the leader button may advance the run: starting floor 1 from the hub, leaving
        /// a rest stop for the next floor, or returning to the hub after a defeat or final victory.
        /// </summary>
        public static bool CanAdvance(FightPhase phase, int floor, int totalFloors) =>
            phase == FightPhase.Waiting ||
            phase == FightPhase.RestStop ||
            phase == FightPhase.Defeat ||
            (phase == FightPhase.Victory && IsFinalFloor(floor, totalFloors));
    }
}
