using System.Collections.Generic;
using GraceRun.Bosses;
using GraceRun.Core.Framing;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class ArenaFramingTests
    {
        private static readonly Vector2 Half = new Vector2(20f, 11.25f);
        private static readonly Rect Wide = new Rect(-40f, -9.25f, 80f, 22.5f);

        [Test]
        public void BoxCenter_IsMiddleOfTheBoundingBox() =>
            Assert.AreEqual(new Vector2(5f, 1f), ArenaFraming.BoxCenter(new List<Vector2> { new Vector2(-10f, 0f), new Vector2(20f, 2f), new Vector2(0f, 1f) }, Vector2.zero));

        [Test]
        public void BoxCenter_EmptyUsesFallback() =>
            Assert.AreEqual(new Vector2(3f, 4f), ArenaFraming.BoxCenter(new List<Vector2>(), new Vector2(3f, 4f)));

        [Test]
        public void ClampToBounds_KeepsTheWholeViewInside()
        {
            var clamped = ArenaFraming.ClampToBounds(new Vector2(-39f, 50f), Half, Wide);
            Assert.AreEqual(-20f, clamped.x, 0.001f);
            Assert.AreEqual(1.99999f, clamped.y, 0.01f); // bounds are exactly one view tall: centred
        }

        [Test]
        public void ClampToBounds_ArenaSmallerThanViewIsCentred()
        {
            var small = new Rect(-18f, -9f, 36f, 20f);
            Assert.AreEqual(new Vector2(0f, 1f), ArenaFraming.ClampToBounds(new Vector2(7f, 9f), Half, small));
        }

        [Test]
        public void KeepInside_PullsTheCameraTowardAPlayerNearTheEdge()
        {
            var center = ArenaFraming.KeepInside(Vector2.zero, new Vector2(30f, 0f), Half, 5f);
            Assert.AreEqual(15f, center.x, 0.001f); // player sits 5 units inside the right edge (35 - 5 = 30)
        }

        [Test]
        public void KeepInside_LeavesAPlayerWhoIsAlreadyInsideAlone() =>
            Assert.AreEqual(new Vector2(2f, 1f), ArenaFraming.KeepInside(new Vector2(2f, 1f), new Vector2(10f, 3f), Half, 5f));

        [Test]
        public void Solve_PlayersFarApartKeepTheLocalOneOnScreen()
        {
            var local = new Vector2(-30f, -3f);
            var focus = new List<Vector2> { local, new Vector2(30f, -3f) };
            var center = ArenaFraming.Solve(focus, local, Half, Wide, 5f, Vector2.zero);
            Assert.LessOrEqual(Mathf.Abs(local.x - center.x), Half.x - 5f + 0.001f);
        }

        [Test]
        public void Solve_PlayersCloseTogetherAreCentred()
        {
            var local = new Vector2(6f, -3f);
            var focus = new List<Vector2> { local, new Vector2(10f, -3f) };
            Assert.AreEqual(8f, ArenaFraming.Solve(focus, local, Half, Wide, 5f, Vector2.zero).x, 0.001f);
        }

        [Test]
        public void Solve_NoPlayersStaysPut() =>
            Assert.AreEqual(new Vector2(12f, 2f), ArenaFraming.Solve(new List<Vector2>(), null, Half, Wide, 5f, new Vector2(12f, 2f)));

        [TestCase(-38f)]
        [TestCase(0f)]
        [TestCase(38f)]
        public void Solve_NeverLeavesTheArenaAndKeepsTheLocalPlayerVisible(float localX)
        {
            var local = new Vector2(localX, -3f);
            var center = ArenaFraming.Solve(new List<Vector2> { local }, local, Half, Wide, 5f, Vector2.zero);
            Assert.GreaterOrEqual(center.x - Half.x, Wide.xMin - 0.001f);
            Assert.LessOrEqual(center.x + Half.x, Wide.xMax + 0.001f);
            Assert.LessOrEqual(Mathf.Abs(local.x - center.x), Half.x);
        }

        [Test]
        public void IsOnScreen_RespectsTheHideMargin()
        {
            Assert.IsTrue(EdgeIndicatorMath.IsOnScreen(new Vector2(19f, 0f), Vector2.zero, Half, 0.5f));
            Assert.IsFalse(EdgeIndicatorMath.IsOnScreen(new Vector2(19.8f, 0f), Vector2.zero, Half, 0.5f));
        }

        [Test]
        public void TryPlace_OnScreenGivesNoMarker() =>
            Assert.IsFalse(EdgeIndicatorMath.TryPlace(new Vector2(5f, 2f), Vector2.zero, Half, 1f, 0f, out _, out _));

        [Test]
        public void TryPlace_RightOfTheViewSitsOnTheRightEdgeAndPointsRight()
        {
            Assert.IsTrue(EdgeIndicatorMath.TryPlace(new Vector2(60f, 0f), Vector2.zero, Half, 1f, 0f, out var edge, out var angle));
            Assert.AreEqual(19f, edge.x, 0.001f);
            Assert.AreEqual(0f, edge.y, 0.001f);
            Assert.AreEqual(0f, angle, 0.001f);
        }

        [Test]
        public void TryPlace_AboveTheViewSitsOnTheTopEdgeAndPointsUp()
        {
            Assert.IsTrue(EdgeIndicatorMath.TryPlace(new Vector2(0f, 40f), Vector2.zero, Half, 1f, 0f, out var edge, out var angle));
            Assert.AreEqual(10.25f, edge.y, 0.001f);
            Assert.AreEqual(90f, angle, 0.001f);
        }

        [Test]
        public void TryPlace_DiagonalStaysInsideTheInsetRectangleOnTheLineToTheTarget()
        {
            var target = new Vector2(60f, 30f);
            Assert.IsTrue(EdgeIndicatorMath.TryPlace(target, Vector2.zero, Half, 1f, 0f, out var edge, out _));
            Assert.LessOrEqual(Mathf.Abs(edge.x), 19.001f);
            Assert.LessOrEqual(Mathf.Abs(edge.y), 10.251f);
            Assert.AreEqual(target.y / target.x, edge.y / edge.x, 0.001f);
        }

        [Test]
        public void TryPlace_FollowsTheCameraCentre()
        {
            Assert.IsTrue(EdgeIndicatorMath.TryPlace(new Vector2(0f, 0f), new Vector2(50f, 0f), Half, 1f, 0f, out var edge, out var angle));
            Assert.AreEqual(31f, edge.x, 0.001f);
            Assert.AreEqual(180f, Mathf.Abs(angle), 0.001f);
        }

        [TestCase(11f, 12f, 2f, 1f)]
        [TestCase(13f, 12f, 2f, 2f)]
        [TestCase(13f, 12f, 0.5f, 1f)]
        public void ChaseSpeedMultiplier_OnlyBeyondTheChaseDistance(float distance, float chase, float multiplier, float expected) =>
            Assert.AreEqual(expected, BossMath.ChaseSpeedMultiplier(distance, chase, multiplier), 0.001f);

        [Test]
        public void WidenedHalfWidth_WithNoArenaKeepsTheAuthoredValue() =>
            Assert.AreEqual(14.5f, ArenaBounds.WidenedHalfWidth(14.5f), 0.001f);
    }
}
