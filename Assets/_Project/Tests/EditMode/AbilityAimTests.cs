using System.Collections.Generic;
using GraceRun.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class AbilityAimTests
    {
        [Test]
        public void NoInputAimsAlongTheFacing()
        {
            Assert.AreEqual(new Vector2(-1f, 0f), AbilityAim.FromMove(Vector2.zero, -1f, out var tilted));
            Assert.IsFalse(tilted);
        }

        [Test]
        public void HeldSideChoosesTheSideButStaysLevel()
        {
            var aim = AbilityAim.FromMove(new Vector2(-1f, 0f), 1f, out var tilted);
            Assert.AreEqual(new Vector2(-1f, 0f), aim);
            Assert.IsFalse(tilted);
        }

        [Test]
        public void UpAndSideAimsDiagonally()
        {
            var aim = AbilityAim.FromMove(new Vector2(0.7071f, 0.7071f), -1f, out var tilted);
            Assert.IsTrue(tilted);
            Assert.AreEqual(45f, Vector2.Angle(Vector2.right, aim), 0.1f);
        }

        [Test]
        public void StraightDownAimsDown()
        {
            var aim = AbilityAim.FromMove(new Vector2(0f, -1f), 1f, out var tilted);
            Assert.IsTrue(tilted);
            Assert.AreEqual(Vector2.down, aim);
        }

        [Test]
        public void SmallStickTiltIsIgnored()
        {
            AbilityAim.FromMove(new Vector2(1f, 0.3f), 1f, out var tilted);
            Assert.IsFalse(tilted);
        }

        [Test]
        public void PickTargetTakesTheOneClosestToTheAimLine()
        {
            var targets = new List<Vector2> { new(10f, 5f), new(10f, 1f), new(-10f, 0f) };
            Assert.AreEqual(1, AbilityAim.PickTarget(Vector2.zero, Vector2.right, targets, 30f, 30f));
        }

        [Test]
        public void PickTargetIgnoresOutsideTheConeAndRange()
        {
            var targets = new List<Vector2> { new(10f, 10f), new(40f, 0f) };
            Assert.AreEqual(-1, AbilityAim.PickTarget(Vector2.zero, Vector2.right, targets, 30f, 30f));
        }

        [Test]
        public void PickTargetPrefersTheNearerOnATie()
        {
            var targets = new List<Vector2> { new(20f, 0f), new(8f, 0f) };
            Assert.AreEqual(1, AbilityAim.PickTarget(Vector2.zero, Vector2.right, targets, 30f, 30f));
        }
    }
}
