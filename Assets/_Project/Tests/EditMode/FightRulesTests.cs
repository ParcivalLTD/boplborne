using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class FightRulesTests
    {
        [Test]
        public void DeadBossIsVictory() =>
            Assert.AreEqual(FightPhase.Victory, FightRules.Outcome(true, 2, 1));

        [Test]
        public void DeadBossWinsEvenWhenEveryoneFellAtOnce() =>
            Assert.AreEqual(FightPhase.Victory, FightRules.Outcome(true, 2, 0));

        [Test]
        public void EveryPlayerDeadIsDefeat() =>
            Assert.AreEqual(FightPhase.Defeat, FightRules.Outcome(false, 3, 0));

        [Test]
        public void OneLivingPlayerKeepsTheFightGoing() =>
            Assert.AreEqual(FightPhase.Fighting, FightRules.Outcome(false, 4, 1));

        [Test]
        public void NoPlayersIsNotADefeat() =>
            Assert.AreEqual(FightPhase.Fighting, FightRules.Outcome(false, 0, 0));

        [TestCase(3.0, 3)]
        [TestCase(2.01, 3)]
        [TestCase(2.0, 2)]
        [TestCase(0.2, 1)]
        [TestCase(0.0, 0)]
        [TestCase(-0.5, 0)]
        public void CountdownShowsWholeSecondsRoundedUp(double remaining, int expected) =>
            Assert.AreEqual(expected, FightRules.CountdownNumber(remaining));

        [TestCase(FightPhase.Waiting, false)]
        [TestCase(FightPhase.Countdown, true)]
        [TestCase(FightPhase.Fighting, true)]
        [TestCase(FightPhase.Victory, false)]
        [TestCase(FightPhase.Defeat, false)]
        [TestCase(FightPhase.Draft, false)]
        [TestCase(FightPhase.RestStop, false)]
        public void OnlyCountdownAndFightBlockJoining(FightPhase phase, bool blocks) =>
            Assert.AreEqual(blocks, FightRules.BlocksJoining(phase));

        [TestCase(FightPhase.Waiting, true)]
        [TestCase(FightPhase.Defeat, true)]
        [TestCase(FightPhase.Victory, false)]
        [TestCase(FightPhase.Draft, false)]
        [TestCase(FightPhase.Countdown, false)]
        [TestCase(FightPhase.Fighting, false)]
        public void FightStartsOnlyFromWaitingOrDefeat(FightPhase phase, bool canStart) =>
            Assert.AreEqual(canStart, FightRules.CanStart(phase));

        [Test]
        public void EmptyFightIsAbandoned() =>
            Assert.IsTrue(FightRules.IsAbandoned(FightPhase.Fighting, 0));

        [Test]
        public void EmptyCountdownIsAbandoned() =>
            Assert.IsTrue(FightRules.IsAbandoned(FightPhase.Countdown, 0));

        [Test]
        public void FightWithAPlayerLeftIsNotAbandoned() =>
            Assert.IsFalse(FightRules.IsAbandoned(FightPhase.Fighting, 1));

        [Test]
        public void EmptyWaitingRoomIsNotAbandoned() =>
            Assert.IsFalse(FightRules.IsAbandoned(FightPhase.Waiting, 0));

        [Test]
        public void HostLeads() =>
            Assert.AreEqual(0ul, FightRules.Leader(true, 0, new ulong[] { 0, 1, 2 }));

        [Test]
        public void WithoutHostLowestClientLeads() =>
            Assert.AreEqual(2ul, FightRules.Leader(false, 0, new ulong[] { 5, 2, 3 }));

        [Test]
        public void NobodyConnectedHasNoLeader() =>
            Assert.AreEqual(FightRules.NoLeader, FightRules.Leader(false, 0, new ulong[0]));
    }
}
