using GraceRun.Bosses;
using NUnit.Framework;

namespace GraceRun.Tests.EditMode
{
    [TestFixture]
    public class CrownedHollowRulesTests
    {
        [Test]
        public void SelectMimickedAbility_WithValidIds_ReturnsOneOfThem()
        {
            var ids = new[] { 1, 3, 5 };
            var selected = CrownedHollowRules.SelectMimickedAbility(ids, seed: 42, fallbackId: 2);
            Assert.Contains(selected, ids);
        }

        [Test]
        public void SelectMimickedAbility_EmptyIds_ReturnsFallback()
        {
            var empty = new int[0];
            var selected = CrownedHollowRules.SelectMimickedAbility(empty, seed: 42, fallbackId: 99);
            Assert.AreEqual(99, selected);
        }

        [Test]
        public void SelectMimickedAbility_ZeroIds_ReturnsFallback()
        {
            var zeros = new[] { 0, 0, 0 };
            var selected = CrownedHollowRules.SelectMimickedAbility(zeros, seed: 42, fallbackId: 77);
            Assert.AreEqual(77, selected);
        }

        [Test]
        public void ComputeCataclysmRingAngles_ReturnsCorrectCount()
        {
            var angles = CrownedHollowRules.ComputeCataclysmRingAngles(16, 0f);
            Assert.AreEqual(16, angles.Length);
        }

        [Test]
        public void ComputeCataclysmRingAngles_CalculatesUniformSpacing()
        {
            var angles = CrownedHollowRules.ComputeCataclysmRingAngles(4, 0f);
            Assert.AreEqual(0f, angles[0], 0.01f);
            Assert.AreEqual(90f, angles[1], 0.01f);
            Assert.AreEqual(180f, angles[2], 0.01f);
            Assert.AreEqual(270f, angles[3], 0.01f);
        }

        [Test]
        public void ScaleEncounterHP_SoloPlayer_ReturnsBaseHP()
        {
            var hp = CrownedHollowRules.ScaleEncounterHP(1350, 1, 0.6f);
            Assert.AreEqual(1350, hp);
        }

        [Test]
        public void ScaleEncounterHP_TwoPlayers_ReturnsScaledHP()
        {
            // 1350 * (1 + 0.6) = 2160
            var hp = CrownedHollowRules.ScaleEncounterHP(1350, 2, 0.6f);
            Assert.AreEqual(2160, hp);
        }

        [Test]
        public void IsStaggerPunishActive_ChecksPunishWindow()
        {
            var recovery = 1.2f;
            var punish = 0.8f;

            // 0.4s after active is inside 0.8s punish window
            Assert.IsTrue(CrownedHollowRules.IsStaggerPunishActive(recovery, punish, 0.4f));

            // 1.0s after active is outside 0.8s punish window
            Assert.IsFalse(CrownedHollowRules.IsStaggerPunishActive(recovery, punish, 1.0f));
        }
    }
}
