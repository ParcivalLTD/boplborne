using GraceRun.Abilities;
using GraceRun.Combat;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class AbilityMathExtraTests
    {
        [Test]
        public void AimPointStopsAtTheCursor()
        {
            var point = AbilityMath.AimPoint(Vector2.zero, Vector2.right, 4f, 10f);
            Assert.AreEqual(4f, point.x, 0.001f);
            Assert.AreEqual(0f, point.y, 0.001f);
        }

        [Test]
        public void AimPointNeverGoesPastTheRange()
        {
            var point = AbilityMath.AimPoint(new Vector2(1f, 1f), Vector2.up, 50f, 8f);
            Assert.AreEqual(1f, point.x, 0.001f);
            Assert.AreEqual(9f, point.y, 0.001f);
        }

        [Test]
        public void AimPointWithoutCursorUsesTheFullRange()
        {
            var point = AbilityMath.AimPoint(Vector2.zero, new Vector2(-3f, 0f), 0f, 6f);
            Assert.AreEqual(-6f, point.x, 0.001f);
        }

        [Test]
        public void FanDirectionSingleChildKeepsTheDirection()
        {
            var direction = AbilityMath.FanDirection(Vector2.up, 30f, 0, 1);
            Assert.AreEqual(0f, direction.x, 0.001f);
            Assert.AreEqual(1f, direction.y, 0.001f);
        }

        [Test]
        public void FanDirectionSpreadsChildrenEvenlyAroundTheDirection()
        {
            var left = AbilityMath.FanDirection(Vector2.up, 30f, 0, 3);
            var middle = AbilityMath.FanDirection(Vector2.up, 30f, 1, 3);
            var right = AbilityMath.FanDirection(Vector2.up, 30f, 2, 3);
            Assert.AreEqual(0f, middle.x, 0.001f);
            Assert.AreEqual(-left.x, right.x, 0.001f);
            Assert.AreEqual(30f, Vector2.Angle(middle, right), 0.01f);
        }

        [Test]
        public void ScaledDamageHalvesAndRounds()
        {
            Assert.AreEqual(5, DamageMath.Scaled(10, 0.5f));
            Assert.AreEqual(4, DamageMath.Scaled(7, 0.5f));
        }

        [Test]
        public void ScaledDamageKeepsAtLeastOne()
        {
            Assert.AreEqual(1, DamageMath.Scaled(1, 0.1f));
            Assert.AreEqual(0, DamageMath.Scaled(0, 0.5f));
        }
    }
}
