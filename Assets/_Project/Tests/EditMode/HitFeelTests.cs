using GraceRun.Combat;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class HitFeelTests
    {
        [Test]
        public void HitWithoutKnockbackIsLight()
        {
            Assert.IsFalse(HitFeel.IsHeavy(Vector2.zero, 4f));
        }

        [Test]
        public void KnockbackAtThresholdIsHeavy()
        {
            Assert.IsTrue(HitFeel.IsHeavy(new Vector2(4f, 0f), 4f));
            Assert.IsTrue(HitFeel.IsHeavy(new Vector2(-10f, 5f), 4f));
            Assert.IsFalse(HitFeel.IsHeavy(new Vector2(2f, 2f), 4f));
        }

        [Test]
        public void ZeroThresholdNeverCountsAsHeavy()
        {
            Assert.IsFalse(HitFeel.IsHeavy(new Vector2(10f, 0f), 0f));
        }

        [Test]
        public void DirectionFollowsKnockbackFirst()
        {
            Assert.AreEqual(-1f, HitFeel.Direction(new Vector2(-8f, 2f), 0f, 5f, true));
        }

        [Test]
        public void DirectionPointsAwayFromAttackerWithoutKnockback()
        {
            Assert.AreEqual(1f, HitFeel.Direction(Vector2.zero, 0f, 5f, true));
            Assert.AreEqual(-1f, HitFeel.Direction(Vector2.zero, 5f, 0f, true));
        }

        [Test]
        public void DirectionDefaultsToRight()
        {
            Assert.AreEqual(1f, HitFeel.Direction(Vector2.zero, 9f, 0f, false));
        }
    }
}
