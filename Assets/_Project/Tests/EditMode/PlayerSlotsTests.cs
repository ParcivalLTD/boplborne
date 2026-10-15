using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class PlayerSlotsTests
    {
        [Test]
        public void FirstFourClientsGetSlotsZeroToThree()
        {
            var slots = new PlayerSlots(4);

            for (var i = 0; i < 4; i++)
            {
                Assert.IsTrue(slots.TryTake((ulong)i, out var slot));
                Assert.AreEqual(i, slot);
            }
        }

        [Test]
        public void FifthClientGetsNoSlot()
        {
            var slots = new PlayerSlots(4);
            for (var i = 0; i < 4; i++) slots.TryTake((ulong)i, out _);

            Assert.IsFalse(slots.TryTake(99, out var slot));
            Assert.AreEqual(-1, slot);
        }

        [Test]
        public void ReleasedSlotIsReused()
        {
            var slots = new PlayerSlots(4);
            for (var i = 0; i < 4; i++) slots.TryTake((ulong)i, out _);

            slots.Release(1);

            Assert.IsTrue(slots.TryTake(99, out var slot));
            Assert.AreEqual(1, slot);
        }

        [Test]
        public void SameClientKeepsItsSlot()
        {
            var slots = new PlayerSlots(4);
            slots.TryTake(7, out var first);

            Assert.IsTrue(slots.Has(7));
            Assert.IsTrue(slots.TryTake(7, out var again));
            Assert.AreEqual(first, again);
            Assert.AreEqual(1, slots.Count);
        }
    }
}
