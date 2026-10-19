using GraceRun.Combat;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class HealthRulesTests
    {
        [Test]
        public void Max_UsesStatsWithoutOverride() =>
            Assert.AreEqual(100, HealthRules.Max(HealthRules.NoOverride, 100));

        [Test]
        public void Max_UsesOverrideWhenSet() =>
            Assert.AreEqual(125, HealthRules.Max(HealthRules.OverrideFor(125), 100));

        [Test]
        public void SetMaxZero_ClearsOverrideBackToStats() =>
            Assert.AreEqual(100, HealthRules.Max(HealthRules.OverrideFor(0), 100));

        [Test]
        public void SetMaxNegative_ClearsOverride() =>
            Assert.AreEqual(HealthRules.NoOverride, HealthRules.OverrideFor(-5));
    }
}
