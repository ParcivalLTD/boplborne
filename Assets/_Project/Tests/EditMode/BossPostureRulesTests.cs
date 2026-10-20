using GraceRun.Bosses;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class BossPostureRulesTests
    {
        [Test]
        public void CalculatePostureDamage_ZeroOrNegative_ReturnsZero()
        {
            Assert.AreEqual(0f, BossPostureRules.CalculatePostureDamage(0, false));
            Assert.AreEqual(0f, BossPostureRules.CalculatePostureDamage(-10, true));
        }

        [Test]
        public void CalculatePostureDamage_LightAttack_UsesStandardScale()
        {
            // 20 damage * 1.0 (light) * 0.5 = 10 posture damage
            var posture = BossPostureRules.CalculatePostureDamage(20, false, 2.5f, 0.5f);
            Assert.AreEqual(10f, posture);
        }

        [Test]
        public void CalculatePostureDamage_HeavyAttack_MultipliesPosture()
        {
            // 20 damage * 2.5 (heavy) * 0.5 = 25 posture damage
            var posture = BossPostureRules.CalculatePostureDamage(20, true, 2.5f, 0.5f);
            Assert.AreEqual(25f, posture);
        }

        [Test]
        public void UpdatePosture_BeforeDelay_DoesNotDecay()
        {
            var posture = BossPostureRules.UpdatePosture(50f, 2f, 4f, 20f, 0.1f);
            Assert.AreEqual(50f, posture);
        }

        [Test]
        public void UpdatePosture_AfterDelay_DecaysAtRate()
        {
            // 50 - 20 * 0.5 = 40
            var posture = BossPostureRules.UpdatePosture(50f, 4.5f, 4f, 20f, 0.5f);
            Assert.AreEqual(40f, posture);
        }

        [Test]
        public void UpdatePosture_DoesNotDropBelowZero()
        {
            var posture = BossPostureRules.UpdatePosture(5f, 5f, 4f, 20f, 1f);
            Assert.AreEqual(0f, posture);
        }

        [Test]
        public void IsBroken_ThresholdReached()
        {
            Assert.IsFalse(BossPostureRules.IsBroken(99f, 100f));
            Assert.IsTrue(BossPostureRules.IsBroken(100f, 100f));
            Assert.IsTrue(BossPostureRules.IsBroken(120f, 100f));
        }

        [Test]
        public void StaggerDamage_WhenStaggered_AppliesCriticalMultiplier()
        {
            Assert.AreEqual(20, BossPostureRules.StaggerDamage(20, false, 1.5f));
            Assert.AreEqual(30, BossPostureRules.StaggerDamage(20, true, 1.5f));
        }

        [Test]
        public void Normalized_ClampedBetweenZeroAndOne()
        {
            Assert.AreEqual(0f, BossPostureRules.Normalized(-10f, 100f));
            Assert.AreEqual(0.5f, BossPostureRules.Normalized(50f, 100f));
            Assert.AreEqual(1f, BossPostureRules.Normalized(150f, 100f));
        }
    }
}
