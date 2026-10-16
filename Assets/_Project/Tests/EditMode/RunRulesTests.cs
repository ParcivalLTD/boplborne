using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class RunRulesTests
    {
        private static readonly int[] StandardRestFloors = { 2, 4, 6 };

        [TestCase(0, 3, false)]
        [TestCase(1, 3, false)]
        [TestCase(2, 3, false)]
        [TestCase(3, 3, true)]
        [TestCase(1, 0, false)]
        public void FinalFloorIsDetectedCorrectly(int floor, int totalFloors, bool expected) =>
            Assert.AreEqual(expected, RunRules.IsFinalFloor(floor, totalFloors));

        [TestCase(0, 3, false)]
        [TestCase(1, 3, false)]
        [TestCase(2, 3, true)]
        [TestCase(3, 3, false)]
        [TestCase(2, 2, false)]
        [TestCase(4, 8, true)]
        [TestCase(6, 8, true)]
        [TestCase(8, 8, false)]
        public void RestStopOnlyAfterConfiguredNonFinalFloors(int floor, int totalFloors, bool expected) =>
            Assert.AreEqual(expected, RunRules.HasRestStopAfter(floor, totalFloors, StandardRestFloors));

        [Test]
        public void EmptyPoolPicksMinusOne() =>
            Assert.AreEqual(-1, RunRules.PickBossIndex(0, 0.5));

        [TestCase(1, 0.0, 0)]
        [TestCase(1, 0.99, 0)]
        [TestCase(3, 0.0, 0)]
        [TestCase(3, 0.34, 1)]
        [TestCase(3, 0.99, 2)]
        public void PickBossIndexMapsRollAcrossPool(int poolSize, double roll, int expected) =>
            Assert.AreEqual(expected, RunRules.PickBossIndex(poolSize, roll));

        [Test]
        public void FloorSeedDiffersByFloorAndRunSeed()
        {
            Assert.AreNotEqual(RunRules.FloorSeed(12345, 1), RunRules.FloorSeed(12345, 2));
            Assert.AreNotEqual(RunRules.FloorSeed(12345, 1), RunRules.FloorSeed(54321, 1));
            Assert.AreEqual(RunRules.FloorSeed(12345, 2), RunRules.FloorSeed(12345, 2));
        }

        [TestCase(FightPhase.Waiting, 0, 3, true)]
        [TestCase(FightPhase.RestStop, 2, 3, true)]
        [TestCase(FightPhase.Defeat, 1, 3, true)]
        [TestCase(FightPhase.Victory, 3, 3, true)]
        [TestCase(FightPhase.Victory, 1, 3, false)]
        [TestCase(FightPhase.Victory, 2, 3, false)]
        [TestCase(FightPhase.Countdown, 1, 3, false)]
        [TestCase(FightPhase.Fighting, 1, 3, false)]
        [TestCase(FightPhase.Draft, 1, 3, false)]
        public void CanAdvanceMatchesRunPhases(FightPhase phase, int floor, int totalFloors, bool expected) =>
            Assert.AreEqual(expected, RunRules.CanAdvance(phase, floor, totalFloors));
    }
}
