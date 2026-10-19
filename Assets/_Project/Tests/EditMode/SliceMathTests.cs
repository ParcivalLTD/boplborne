using GraceRun.Player;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class SliceMathTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void SmallWiggleIsNotADrag() =>
            Assert.IsFalse(SliceMath.IsDrag(Vector2.zero, new Vector2(0.3f, 0.3f), 0.6f));

        [Test]
        public void MovingPastTheStartDistanceIsADrag() =>
            Assert.IsTrue(SliceMath.IsDrag(Vector2.zero, new Vector2(0.7f, 0f), 0.6f));

        [Test]
        public void SegmentInsideReachIsKeptWhole()
        {
            Assert.IsTrue(SliceMath.ClipToCircle(new Vector2(-1f, 0f), new Vector2(1f, 0f), Vector2.zero, 3f, out var a, out var b));
            Assert.AreEqual(-1f, a.x, Tolerance);
            Assert.AreEqual(1f, b.x, Tolerance);
        }

        [Test]
        public void SegmentCrossingReachIsCutAtTheEdge()
        {
            Assert.IsTrue(SliceMath.ClipToCircle(new Vector2(-5f, 0f), new Vector2(5f, 0f), Vector2.zero, 3f, out var a, out var b));
            Assert.AreEqual(-3f, a.x, Tolerance);
            Assert.AreEqual(3f, b.x, Tolerance);
        }

        [Test]
        public void SegmentStartingInsideIsCutWhereItLeaves()
        {
            Assert.IsTrue(SliceMath.ClipToCircle(Vector2.zero, new Vector2(0f, 10f), Vector2.zero, 2f, out var a, out var b));
            Assert.AreEqual(0f, a.y, Tolerance);
            Assert.AreEqual(2f, b.y, Tolerance);
        }

        [Test]
        public void SegmentOutOfReachIsDropped() =>
            Assert.IsFalse(SliceMath.ClipToCircle(new Vector2(-5f, 4f), new Vector2(5f, 4f), Vector2.zero, 3f, out _, out _));

        [Test]
        public void SegmentPointingAwayOutsideIsDropped() =>
            Assert.IsFalse(SliceMath.ClipToCircle(new Vector2(4f, 0f), new Vector2(8f, 0f), Vector2.zero, 3f, out _, out _));

        [Test]
        public void KnockbackFollowsTheCutPlusLift()
        {
            var push = SliceMath.Knockback(new Vector2(-2f, 0f), 6f, 3f);
            Assert.AreEqual(-6f, push.x, Tolerance);
            Assert.AreEqual(3f, push.y, Tolerance);
        }
    }
}
