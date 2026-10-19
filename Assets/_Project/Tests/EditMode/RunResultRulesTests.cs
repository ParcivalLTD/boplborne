using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class RunResultRulesTests
    {
        [TestCase(0f, "00:00")]
        [TestCase(45f, "00:45")]
        [TestCase(60f, "01:00")]
        [TestCase(125f, "02:05")]
        [TestCase(3665f, "01:01:05")]
        public void FormatTimeFormatsSecondsCorrectly(float seconds, string expected)
        {
            Assert.AreEqual(expected, RunResultRules.FormatTime(seconds));
        }

        [TestCase(true, 3, 3, "All 3 Floors Cleared!")]
        [TestCase(true, 7, 7, "All 7 Floors Cleared!")]
        [TestCase(false, 1, 3, "Reached Floor 1 of 3")]
        [TestCase(false, 2, 3, "Reached Floor 2 of 3")]
        [TestCase(false, 3, 3, "Reached Floor 3 of 3")]
        public void FormatFloorsDescribesOutcomeClearly(bool victory, int floorReached, int totalFloors, string expected)
        {
            Assert.AreEqual(expected, RunResultRules.FormatFloors(victory, floorReached, totalFloors));
        }

        [Test]
        public void TotalDamageAndKnockbacksSumProperly()
        {
            var damage = new[] { 100, 250, 0, 50 };
            var knockbacks = new[] { 2, 0, 5, 1 };

            Assert.AreEqual(400, RunResultRules.CalculateTotalDamage(damage));
            Assert.AreEqual(8, RunResultRules.CalculateTotalKnockbacks(knockbacks));
        }

        [Test]
        public void FindTopCulpritReturnsHighestOrMinusOneIfZero()
        {
            Assert.AreEqual(-1, RunResultRules.FindTopKnockbackCulprit(new[] { 0, 0, 0, 0 }));
            Assert.AreEqual(1, RunResultRules.FindTopKnockbackCulprit(new[] { 1, 4, 2, 0 }));
            Assert.AreEqual(2, RunResultRules.FindTopKnockbackCulprit(new[] { 0, 1, 9, 3 }));
        }

        [Test]
        public void FindTopDamageDealerReturnsHighestOrMinusOneIfZero()
        {
            Assert.AreEqual(-1, RunResultRules.FindTopDamageDealer(new[] { 0, 0, 0, 0 }));
            Assert.AreEqual(0, RunResultRules.FindTopDamageDealer(new[] { 500, 100, 200, 0 }));
            Assert.AreEqual(3, RunResultRules.FindTopDamageDealer(new[] { 50, 100, 200, 800 }));
        }
    }
}
