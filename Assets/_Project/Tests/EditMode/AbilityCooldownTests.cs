using GraceRun.Abilities;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class AbilityCooldownTests
    {
        private const float Cooldown = 4f;

        [Test]
        public void NeverUsedIsReady()
        {
            Assert.IsTrue(AbilityCooldown.IsReady(0d, 10d));
        }

        [Test]
        public void JustUsedIsNotReady()
        {
            var readyAt = AbilityCooldown.NextReadyAt(10d, Cooldown);
            Assert.IsFalse(AbilityCooldown.IsReady(readyAt, 10d));
        }

        [Test]
        public void ReadyOnceCooldownHasPassed()
        {
            var readyAt = AbilityCooldown.NextReadyAt(10d, Cooldown);
            Assert.IsTrue(AbilityCooldown.IsReady(readyAt, 14d));
        }

        [Test]
        public void ToleranceAcceptsASlightlyEarlyPress()
        {
            var readyAt = AbilityCooldown.NextReadyAt(10d, Cooldown);
            Assert.IsFalse(AbilityCooldown.IsReady(readyAt, 13.9d));
            Assert.IsTrue(AbilityCooldown.IsReady(readyAt, 13.9d, 0.15f));
        }

        [Test]
        public void RemainingGoesFromOneToZero()
        {
            var readyAt = AbilityCooldown.NextReadyAt(10d, Cooldown);
            Assert.AreEqual(1f, AbilityCooldown.Remaining01(readyAt, Cooldown, 10d), 1e-5f);
            Assert.AreEqual(0.5f, AbilityCooldown.Remaining01(readyAt, Cooldown, 12d), 1e-5f);
            Assert.AreEqual(0f, AbilityCooldown.Remaining01(readyAt, Cooldown, 20d), 1e-5f);
        }

        [Test]
        public void NoCooldownShowsNoShade()
        {
            Assert.AreEqual(0f, AbilityCooldown.Remaining01(5d, 0f, 1d));
        }
    }
}
