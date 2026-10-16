using GraceRun.Abilities;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class AbilityMathTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void SingleEffectFliesAtTheLaunchAngle()
        {
            var direction = AbilityMath.LaunchDirection(1f, 45f, 10f, 0, 1);
            Assert.AreEqual(Mathf.Sqrt(0.5f), direction.x, Tolerance);
            Assert.AreEqual(Mathf.Sqrt(0.5f), direction.y, Tolerance);
        }

        [Test]
        public void VolleyFansAroundTheLaunchAngle()
        {
            var low = AbilityMath.LaunchDirection(1f, 20f, 10f, 0, 3);
            var middle = AbilityMath.LaunchDirection(1f, 20f, 10f, 1, 3);
            var high = AbilityMath.LaunchDirection(1f, 20f, 10f, 2, 3);
            Assert.AreEqual(10f, Vector2.SignedAngle(Vector2.right, low), Tolerance);
            Assert.AreEqual(20f, Vector2.SignedAngle(Vector2.right, middle), Tolerance);
            Assert.AreEqual(30f, Vector2.SignedAngle(Vector2.right, high), Tolerance);
        }

        [Test]
        public void FacingLeftMirrorsTheDirection()
        {
            var direction = AbilityMath.LaunchDirection(-1f, 30f, 0f, 0, 1);
            Assert.Less(direction.x, 0f);
            Assert.Greater(direction.y, 0f);
        }

        [Test]
        public void CasterAimTiltsOnlyWhenHoldingUpOrDown()
        {
            Assert.AreEqual(Vector2.left, AbilityMath.CasterAim(-1f, 0.2f));
            var up = AbilityMath.CasterAim(1f, 1f);
            Assert.AreEqual(45f, Vector2.SignedAngle(Vector2.right, up), Tolerance);
            Assert.AreEqual(1f, up.magnitude, Tolerance);
        }

        [Test]
        public void PullPointsAtTheCentreAtFullSpeedOutsideTheCalmRadius()
        {
            var pull = AbilityMath.PullVelocity(new Vector2(-4f, 0f), Vector2.zero, 10f, 1f);
            Assert.AreEqual(10f, pull.x, Tolerance);
            Assert.AreEqual(0f, pull.y, Tolerance);
        }

        [Test]
        public void PullEasesOffInsideTheCalmRadius()
        {
            var pull = AbilityMath.PullVelocity(new Vector2(0f, 0.5f), Vector2.zero, 10f, 1f);
            Assert.AreEqual(-5f, pull.y, Tolerance);
            Assert.AreEqual(Vector2.zero, AbilityMath.PullVelocity(Vector2.zero, Vector2.zero, 10f, 1f));
        }

        [Test]
        public void PushGoesAwayFromTheBlastWithLift()
        {
            var push = AbilityMath.PushAway(Vector2.zero, new Vector2(2f, 0f), 10f, 5f);
            Assert.AreEqual(10f, push.x, Tolerance);
            Assert.AreEqual(5f, push.y, Tolerance);
        }

        [Test]
        public void BlastAboveABodyNeverPushesItDown()
        {
            var push = AbilityMath.PushAway(new Vector2(0.5f, 2f), Vector2.zero, 10f, 5f);
            Assert.AreEqual(-10f, push.x, Tolerance);
            Assert.AreEqual(5f, push.y, Tolerance);
            var straightDown = AbilityMath.PushAway(new Vector2(0f, 2f), Vector2.zero, 10f, 5f);
            Assert.AreEqual(15f, straightDown.y, Tolerance);
        }
    }
}
