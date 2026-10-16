using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class DraftRulesTests
    {
        [Test]
        public void PickFillsTheFirstEmptySlot() =>
            Assert.AreEqual(1, DraftRules.ResolveSlot(2, new[] { false, true, true }));

        [Test]
        public void PickNeverReplacesWhileASlotIsFree() =>
            Assert.AreEqual(2, DraftRules.ResolveSlot(0, new[] { false, false, true }));

        [Test]
        public void FullLoadoutReplacesTheChosenSlot() =>
            Assert.AreEqual(1, DraftRules.ResolveSlot(1, new[] { false, false, false }));

        [Test]
        public void FullLoadoutWithBadSlotHasNoSlot() =>
            Assert.AreEqual(DraftRules.NoSlot, DraftRules.ResolveSlot(3, new[] { false, false, false }));

        [TestCase(1, 0, 1)]
        [TestCase(1, 1, 0)]
        [TestCase(1, 2, 0)]
        [TestCase(0, 0, 0)]
        public void RerollsLeftNeverGoBelowZero(int perRun, int used, int expected) =>
            Assert.AreEqual(expected, DraftRules.RerollsLeft(perRun, used));
    }
}
