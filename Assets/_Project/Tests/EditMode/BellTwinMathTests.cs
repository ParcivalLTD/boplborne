using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class BellTwinMathTests
    {
        [TestCase(720, 360, 360)]
        [TestCase(701, 351, 350)]
        [TestCase(1, 1, 1)]
        public void SplitEncounterHP_DividesTotalAcrossTwoTwins(int total, int expectedPrimary, int expectedPartner)
        {
            BellTwinMath.SplitEncounterHP(total, out var primary, out var partner);
            Assert.AreEqual(expectedPrimary, primary);
            Assert.AreEqual(expectedPartner, partner);
        }

        [TestCase(360, 360, 720, 1.0f)]
        [TestCase(0, 360, 720, 0.5f)]
        [TestCase(180, 180, 720, 0.5f)]
        [TestCase(0, 180, 720, 0.25f)]
        [TestCase(0, 0, 720, 0.0f)]
        public void NormalizedEncounterHP_SumsBothBodiesAgainstTotalMax(
            int primary, int partner, int encounterMax, float expected)
        {
            Assert.AreEqual(expected, BellTwinMath.NormalizedEncounterHP(primary, partner, encounterMax), 0.001f);
        }

        [Test]
        public void ShouldTriggerEnrage_FiresOnOneTwinDeadOrAtHalfCombinedHP()
        {
            Assert.IsFalse(BellTwinMath.ShouldTriggerEnrage(250, 250, 720, 0.5f));
            Assert.IsTrue(BellTwinMath.ShouldTriggerEnrage(0, 360, 720, 0.5f));
            Assert.IsTrue(BellTwinMath.ShouldTriggerEnrage(360, 0, 720, 0.5f));
            Assert.IsTrue(BellTwinMath.ShouldTriggerEnrage(160, 200, 720, 0.5f));
            Assert.IsFalse(BellTwinMath.ShouldTriggerEnrage(0, 0, 720, 0.5f));
        }

        [Test]
        public void ResolveTwinDeath_KillsWeakerTwinAndConservesCombinedHPInSurvivor()
        {
            BellTwinMath.ResolveTwinDeath(160, 200, 360, out var p1, out var p2, out var primarySurvives);
            Assert.IsFalse(primarySurvives);
            Assert.AreEqual(0, p1);
            Assert.AreEqual(360, p2);

            BellTwinMath.ResolveTwinDeath(360, 0, 360, out p1, out p2, out primarySurvives);
            Assert.IsTrue(primarySurvives);
            Assert.AreEqual(360, p1);
            Assert.AreEqual(0, p2);
        }

        [Test]
        public void SplitDebugHP_AboveThresholdSplitsAndAtOrBelowKillsOneTwin()
        {
            BellTwinMath.SplitDebugHP(600, 720, 360, 0.5f, true, true, out var p1, out var p2);
            Assert.AreEqual(300, p1);
            Assert.AreEqual(300, p2);

            BellTwinMath.SplitDebugHP(360, 720, 360, 0.5f, true, true, out p1, out p2);
            Assert.AreEqual(360, p1);
            Assert.AreEqual(0, p2);

            BellTwinMath.SplitDebugHP(0, 720, 360, 0.5f, false, true, out p1, out p2);
            Assert.AreEqual(0, p1);
            Assert.AreEqual(0, p2);
        }

        [Test]
        public void ChooseSplitTarget_AvoidsPartnerTargetWhenMultiplePlayersAlive()
        {
            var twoPlayers = new ulong[] { 1UL, 2UL };
            Assert.AreEqual(2UL, BellTwinMath.ChooseSplitTarget(BossTargeting.NoTarget, 1UL, twoPlayers, 0));
            Assert.AreEqual(1UL, BellTwinMath.ChooseSplitTarget(BossTargeting.NoTarget, 2UL, twoPlayers, 0));

            var soloPlayer = new ulong[] { 1UL };
            Assert.AreEqual(1UL, BellTwinMath.ChooseSplitTarget(BossTargeting.NoTarget, 1UL, soloPlayer, 0));
        }
    }
}
