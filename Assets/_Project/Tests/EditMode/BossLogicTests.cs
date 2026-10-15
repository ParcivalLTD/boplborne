using GraceRun.Bosses;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class BossLogicTests
    {
        [TestCase(1, 1000)]
        [TestCase(2, 1600)]
        [TestCase(3, 2200)]
        [TestCase(4, 2800)]
        public void ScaledMaxHP_FollowsGddFormula(int players, int expected) =>
            Assert.AreEqual(expected, BossMath.ScaledMaxHP(1000, 0.6f, players));

        [Test]
        public void ScaledMaxHP_ZeroPlayersCountsAsOne() =>
            Assert.AreEqual(500, BossMath.ScaledMaxHP(500, 0.6f, 0));

        [Test]
        public void ScaledMaxHP_RoundsToNearest() =>
            Assert.AreEqual(1053, BossMath.ScaledMaxHP(658, 0.6f, 2)); // 1052.8

        [TestCase(51, false)]
        [TestCase(50, true)]
        [TestCase(1, true)]
        [TestCase(0, false)]
        public void IsInPhase2_AtOrBelowThresholdWhileAlive(int current, bool expected) =>
            Assert.AreEqual(expected, BossMath.IsInPhase2(current, 100, 0.5f));

        [Test]
        public void AttackData_PhaseAt_WalksTheTimeline()
        {
            var attack = UnityEngine.ScriptableObject.CreateInstance<AttackData>(); // windup 0.8, active 0.15, recovery 1
            Assert.AreEqual(AttackPhase.Pending, attack.PhaseAt(-0.01));
            Assert.AreEqual(AttackPhase.Windup, attack.PhaseAt(0.0));
            Assert.AreEqual(AttackPhase.Active, attack.PhaseAt(0.85));
            Assert.AreEqual(AttackPhase.Recovery, attack.PhaseAt(1.0));
            Assert.AreEqual(AttackPhase.None, attack.PhaseAt(2.0));
            UnityEngine.Object.DestroyImmediate(attack);
        }

        [Test]
        public void Pick_SkipsOutOfRangeAndCoolingDown()
        {
            var options = new[] { new AttackOption(1f, 0f, 3f), new AttackOption(1f, 0f, 10f), new AttackOption(1f, 0f, 10f) };
            var ready = new[] { true, false, true };
            Assert.AreEqual(2, AttackPicker.Pick(options, ready, 5f, 0.0));
            Assert.AreEqual(2, AttackPicker.Pick(options, ready, 5f, 0.99));
        }

        [Test]
        public void Pick_UsesWeights()
        {
            var options = new[] { new AttackOption(1f, 0f, 10f), new AttackOption(3f, 0f, 10f) };
            var ready = new[] { true, true };
            Assert.AreEqual(0, AttackPicker.Pick(options, ready, 1f, 0.2));
            Assert.AreEqual(1, AttackPicker.Pick(options, ready, 1f, 0.3));
        }

        [Test]
        public void Pick_NoneFits_ReturnsMinusOne()
        {
            var options = new[] { new AttackOption(1f, 2f, 4f) };
            Assert.AreEqual(-1, AttackPicker.Pick(options, new[] { true }, 1f, 0.5));
            Assert.AreEqual(-1, AttackPicker.Pick(options, new[] { false }, 3f, 0.5));
        }

        [Test]
        public void IsOutOfReach_OnlyWhenFartherThanEveryReadyAttack()
        {
            var options = new[] { new AttackOption(1f, 0f, 3f), new AttackOption(1f, 0f, 8f) };
            Assert.IsFalse(AttackPicker.IsOutOfReach(options, new[] { true, true }, 5f));
            Assert.IsTrue(AttackPicker.IsOutOfReach(options, new[] { true, false }, 5f));
            Assert.IsFalse(AttackPicker.IsOutOfReach(options, new[] { false, false }, 50f));
        }

        [Test]
        public void Choose_AvoidsCurrentTargetWhenOthersLive()
        {
            var ids = new ulong[] { 10, 20, 30 };
            for (var roll = 0; roll < 6; roll++)
                Assert.AreNotEqual(20UL, BossTargeting.Choose(20, ids, roll));
        }

        [Test]
        public void Choose_SingleOrNoCandidate()
        {
            Assert.AreEqual(7UL, BossTargeting.Choose(7, new ulong[] { 7 }, 3));
            Assert.AreEqual(BossTargeting.NoTarget, BossTargeting.Choose(7, new ulong[0], 3));
        }
    }
}
