using GraceRun.Bosses;
using NUnit.Framework;

namespace GraceRun.Tests.EditMode
{
    [TestFixture]
    public class ClockworkColossusRulesTests
    {
        [Test]
        public void IsBackHit_WhenFacingRightAndAttackerBehind_ReturnsTrue()
        {
            // Boss at x = 0 facing right (+1); attacker at x = -2 (behind)
            Assert.IsTrue(ClockworkColossusRules.IsBackHit(-2f, 0f, 1f));
        }

        [Test]
        public void IsBackHit_WhenFacingRightAndAttackerInFront_ReturnsFalse()
        {
            // Boss at x = 0 facing right (+1); attacker at x = 2 (in front)
            Assert.IsFalse(ClockworkColossusRules.IsBackHit(2f, 0f, 1f));
        }

        [Test]
        public void IsBackHit_WhenFacingLeftAndAttackerBehind_ReturnsTrue()
        {
            // Boss at x = 0 facing left (-1); attacker at x = 2 (behind)
            Assert.IsTrue(ClockworkColossusRules.IsBackHit(2f, 0f, -1f));
        }

        [Test]
        public void IsBackHit_WhenFacingLeftAndAttackerInFront_ReturnsFalse()
        {
            // Boss at x = 0 facing left (-1); attacker at x = -2 (in front)
            Assert.IsFalse(ClockworkColossusRules.IsBackHit(-2f, 0f, -1f));
        }

        [Test]
        public void CalculateBackHitPostureDamage_NormalHit_ReturnsUnamplified()
        {
            var dmg = ClockworkColossusRules.CalculateBackHitPostureDamage(20, false, 2.0f);
            Assert.AreEqual(20f, dmg);
        }

        [Test]
        public void CalculateBackHitPostureDamage_BackHit_AppliesMultiplier()
        {
            var dmg = ClockworkColossusRules.CalculateBackHitPostureDamage(20, true, 2.0f);
            Assert.AreEqual(40f, dmg);
        }

        [Test]
        public void ConveyorVelocity_AlternatesWithInterval()
        {
            var baseSpeed = 4f;
            var interval = 5f;

            // At t = 1s, cycle 0 -> +4f
            Assert.AreEqual(4f, ClockworkColossusRules.ConveyorVelocity(baseSpeed, 1f, interval));

            // At t = 6s, cycle 1 -> -4f
            Assert.AreEqual(-4f, ClockworkColossusRules.ConveyorVelocity(baseSpeed, 6f, interval));

            // At t = 11s, cycle 2 -> +4f
            Assert.AreEqual(4f, ClockworkColossusRules.ConveyorVelocity(baseSpeed, 11f, interval));
        }

        [Test]
        public void IsOnFloor_DetectsFloorVsPlatforms()
        {
            var floorTop = -5f;
            var tolerance = 0.5f;

            // Player at y = -4.8f is grounded on the floor
            Assert.IsTrue(ClockworkColossusRules.IsOnFloor(-4.8f, floorTop, tolerance));

            // Player on platform at y = -2.5f is elevated
            Assert.IsFalse(ClockworkColossusRules.IsOnFloor(-2.5f, floorTop, tolerance));
        }
    }
}
