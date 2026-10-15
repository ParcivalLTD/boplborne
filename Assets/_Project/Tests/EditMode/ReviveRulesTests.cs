using GraceRun.Combat;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class ReviveRulesTests
    {
        [Test]
        public void Progress_IsZeroWhenNobodyRevives() =>
            Assert.AreEqual(0f, ReviveRules.Progress(ReviveRules.NotReviving, 10d, 3f));

        [Test]
        public void Progress_GrowsWithHoldTime() =>
            Assert.AreEqual(0.5f, ReviveRules.Progress(10d, 11.5d, 3f), 0.0001f);

        [Test]
        public void Progress_ClampsAtOne() =>
            Assert.AreEqual(1f, ReviveRules.Progress(10d, 20d, 3f));

        [Test]
        public void ReviveHp_IsThirtyPercentOfMax() =>
            Assert.AreEqual(30, ReviveRules.ReviveHp(100, 0.3f));

        [Test]
        public void ReviveHp_IsAtLeastOne() =>
            Assert.AreEqual(1, ReviveRules.ReviveHp(2, 0.1f));

        [Test]
        public void SoloLife_UsedWhenAloneWithLifeLeft() =>
            Assert.IsTrue(ReviveRules.UsesSoloLife(1, 0, 1));

        [Test]
        public void SoloLife_NotUsedTwicePerFloor() =>
            Assert.IsFalse(ReviveRules.UsesSoloLife(1, 1, 1));

        [Test]
        public void SoloLife_NotUsedWithTeammates() =>
            Assert.IsFalse(ReviveRules.UsesSoloLife(2, 0, 1));

        [Test]
        public void InRange_IncludesTheEdge()
        {
            Assert.IsTrue(ReviveRules.InRange(Vector2.zero, new Vector2(1.5f, 0f), 1.5f));
            Assert.IsFalse(ReviveRules.InRange(Vector2.zero, new Vector2(1.6f, 0f), 1.5f));
        }
    }
}
