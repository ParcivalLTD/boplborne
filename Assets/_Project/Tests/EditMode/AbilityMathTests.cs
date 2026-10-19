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

        [Test]
        public void CursorAimPointsAtTheCursor()
        {
            var aim = AbilityMath.CursorAim(new Vector2(1f, 1f), new Vector2(-2f, 5f), 1f);
            Assert.AreEqual(-0.6f, aim.x, Tolerance);
            Assert.AreEqual(0.8f, aim.y, Tolerance);
        }

        [Test]
        public void CursorOnTheCasterFallsBackToFacing() =>
            Assert.AreEqual(Vector2.left, AbilityMath.CursorAim(Vector2.one, Vector2.one, -1f));

        [Test]
        public void SafeAimTurnsZeroAndNaNIntoRight()
        {
            Assert.AreEqual(Vector2.right, AbilityMath.SafeAim(Vector2.zero));
            Assert.AreEqual(Vector2.right, AbilityMath.SafeAim(new Vector2(float.NaN, 1f)));
            Assert.AreEqual(1f, AbilityMath.SafeAim(new Vector2(3f, 4f)).magnitude, Tolerance);
        }

        [Test]
        public void ElevationIsMeasuredOnTheAimsOwnSide()
        {
            Assert.AreEqual(45f, AbilityMath.Elevation(new Vector2(-1f, 1f)), Tolerance);
            Assert.AreEqual(-90f, AbilityMath.Elevation(Vector2.down), Tolerance);
        }

        [Test]
        public void AimedOffsetWithoutElevationOnlyMirrors() =>
            Assert.AreEqual(new Vector2(-4f, 1.5f), AbilityMath.AimedOffset(new Vector2(4f, 1.5f), -1f, 0f));

        [Test]
        public void AimedOffsetTurnsTowardTheAim()
        {
            var up = AbilityMath.AimedOffset(new Vector2(4f, 0f), 1f, 90f);
            Assert.AreEqual(0f, up.x, Tolerance);
            Assert.AreEqual(4f, up.y, Tolerance);
        }

        [Test]
        public void CursorAimedLaunchMatchesTheCursor()
        {
            var aim = new Vector2(-1f, 1f).normalized;
            var direction = AbilityMath.LaunchDirection(AbilityMath.SideOf(aim.x), AbilityMath.Elevation(aim), 0f, 0, 1);
            Assert.AreEqual(aim.x, direction.x, Tolerance);
            Assert.AreEqual(aim.y, direction.y, Tolerance);
        }
    }
}
