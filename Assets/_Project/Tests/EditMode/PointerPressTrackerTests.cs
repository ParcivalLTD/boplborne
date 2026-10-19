using System.Collections.Generic;
using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class PointerPressTrackerTests
    {
        private readonly List<int> _fired = new();
        private readonly List<int> _pressed = new();
        private readonly List<int> _released = new();

        [SetUp]
        public void Clear()
        {
            _fired.Clear();
            _pressed.Clear();
            _released.Clear();
        }

        private void Flush(PointerPressTracker tracker, bool overUi = false, System.Action<int> onRelease = null) =>
            tracker.Flush(overUi, _pressed.Add, slot => { _released.Add(slot); onRelease?.Invoke(slot); }, _fired.Add);

        [Test]
        public void PressAloneFiresNothing()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(0);
            Flush(tracker);

            CollectionAssert.AreEqual(new[] { 0 }, _pressed);
            Assert.IsEmpty(_fired);
            Assert.IsTrue(tracker.IsHeld(0));
        }

        [Test]
        public void ClickFiresOnRelease()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(2);
            Flush(tracker);
            tracker.QueueUp(2);
            Flush(tracker);

            CollectionAssert.AreEqual(new[] { 2 }, _fired);
            Assert.IsFalse(tracker.IsHeld(2));
        }

        [Test]
        public void ClaimedHoldDoesNotFire()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(0);
            Flush(tracker);
            tracker.Claim(0);
            tracker.QueueUp(0);
            Flush(tracker);

            Assert.IsEmpty(_fired);
            CollectionAssert.AreEqual(new[] { 0 }, _released);
        }

        [Test]
        public void ClaimInsideTheReleaseCallbackStillStopsTheFire()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(0);
            Flush(tracker);
            tracker.QueueUp(0);
            Flush(tracker, onRelease: tracker.Claim);

            Assert.IsEmpty(_fired);
        }

        [Test]
        public void ClaimOnlyLastsForOneHold()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(0);
            Flush(tracker);
            tracker.Claim(0);
            tracker.QueueUp(0);
            Flush(tracker);
            tracker.QueueDown(0);
            tracker.QueueUp(0);
            Flush(tracker);

            CollectionAssert.AreEqual(new[] { 0 }, _fired);
        }

        [Test]
        public void PressOverUiNeverFires()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(1);
            Flush(tracker, overUi: true);
            tracker.QueueUp(1);
            Flush(tracker);

            Assert.IsEmpty(_pressed);
            Assert.IsEmpty(_fired);
        }

        [Test]
        public void ClickWithinOneFrameFires()
        {
            var tracker = new PointerPressTracker(3);
            tracker.QueueDown(1);
            tracker.QueueUp(1);
            Flush(tracker);

            CollectionAssert.AreEqual(new[] { 1 }, _fired);
        }
    }
}
