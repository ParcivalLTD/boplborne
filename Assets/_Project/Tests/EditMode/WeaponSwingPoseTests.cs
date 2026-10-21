using GraceRun.Player;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class WeaponSwingPoseTests
    {
        private static readonly WeaponPose Rest = new(new Vector2(-0.3f, 0.2f), 130f, 0f);
        private static readonly WeaponSwingLook Arc = new(WeaponSwingStyle.Arc, 70f, 110f, 0.5f);
        private static readonly WeaponSwingLook Thrust = new(WeaponSwingStyle.Thrust, 0f, 0f, 1f);

        [Test]
        public void Idle_StaysAtRest()
        {
            var pose = WeaponSwingPose.Evaluate(Arc, SwingPhase.Idle, 1f, 0f, 1f, Rest);
            Assert.AreEqual(Rest.Position, pose.Position);
            Assert.AreEqual(Rest.Angle, pose.Angle, 0.001f);
        }

        [Test]
        public void Arc_StartupEndsRaisedAboveTheAim()
        {
            var pose = WeaponSwingPose.Evaluate(Arc, SwingPhase.Startup, 1f, 0f, 1f, Rest);
            Assert.AreEqual(70f, pose.Angle, 0.01f);
            Assert.AreEqual(0f, pose.Smear, 0.001f);
        }

        [Test]
        public void Arc_ActiveSweepsDownThroughTheAim()
        {
            var start = WeaponSwingPose.Evaluate(Arc, SwingPhase.Active, 0f, 0f, 1f, Rest);
            var end = WeaponSwingPose.Evaluate(Arc, SwingPhase.Active, 1f, 0f, 1f, Rest);
            Assert.AreEqual(70f, start.Angle, 0.01f);
            Assert.AreEqual(-40f, end.Angle, 0.01f);
            Assert.Greater(start.Smear, 0.5f);
        }

        [Test]
        public void Arc_AimingLeftMirrorsTheSweep()
        {
            var start = WeaponSwingPose.Evaluate(Arc, SwingPhase.Active, 0f, 180f, -1f, Rest);
            var end = WeaponSwingPose.Evaluate(Arc, SwingPhase.Active, 1f, 180f, -1f, Rest);
            Assert.AreEqual(110f, start.Angle, 0.01f);
            Assert.AreEqual(220f, end.Angle, 0.01f);
        }

        [Test]
        public void Arc_RecoveryEndsAtRest()
        {
            var pose = WeaponSwingPose.Evaluate(Arc, SwingPhase.Recovery, 1f, 0f, 1f, Rest);
            Assert.AreEqual(Rest.Position.x, pose.Position.x, 0.001f);
            Assert.AreEqual(Rest.Position.y, pose.Position.y, 0.001f);
            Assert.AreEqual(0f, pose.Smear, 0.001f);
        }

        [Test]
        public void Thrust_PullsBackThenPushesOutAlongTheAim()
        {
            var back = WeaponSwingPose.Evaluate(Thrust, SwingPhase.Startup, 1f, 0f, 1f, Rest);
            var front = WeaponSwingPose.Evaluate(Thrust, SwingPhase.Active, 1f, 0f, 1f, Rest);
            Assert.Less(back.Position.x, 0f);
            Assert.AreEqual(1f, front.Position.x, 0.001f);
            Assert.AreEqual(0f, front.Angle, 0.001f);
        }

        [Test]
        public void Thrust_AimingUpPushesUp()
        {
            var front = WeaponSwingPose.Evaluate(Thrust, SwingPhase.Active, 1f, 90f, 1f, Rest);
            Assert.AreEqual(0f, front.Position.x, 0.001f);
            Assert.AreEqual(1f, front.Position.y, 0.001f);
        }
    }
}
