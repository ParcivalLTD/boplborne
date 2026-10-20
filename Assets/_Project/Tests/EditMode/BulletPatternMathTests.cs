using GraceRun.Bosses.Behaviours;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class BulletPatternMathTests
    {
        [Test]
        public void ComputeSpreadAngles_SingleAngle_ReturnsCenter()
        {
            var angles = BulletPatternMath.ComputeSpreadAngles(1, 45f, 0f);
            Assert.AreEqual(1, angles.Length);
            Assert.AreEqual(0f, angles[0]);
        }

        [Test]
        public void ComputeSpreadAngles_ThreeAngles_Symmetric()
        {
            var angles = BulletPatternMath.ComputeSpreadAngles(3, 40f, 0f);
            Assert.AreEqual(3, angles.Length);
            Assert.AreEqual(-20f, angles[0]);
            Assert.AreEqual(0f, angles[1]);
            Assert.AreEqual(20f, angles[2]);
        }

        [Test]
        public void ComputeRingAngles_FourAngles_Orthogonal()
        {
            var angles = BulletPatternMath.ComputeRingAngles(4, 0f);
            Assert.AreEqual(4, angles.Length);
            Assert.AreEqual(0f, angles[0]);
            Assert.AreEqual(90f, angles[1]);
            Assert.AreEqual(180f, angles[2]);
            Assert.AreEqual(270f, angles[3]);
        }

        [Test]
        public void FacingAngle_RightFacing_Unchanged()
        {
            Assert.AreEqual(30f, BulletPatternMath.FacingAngle(1, 30f));
        }

        [Test]
        public void FacingAngle_LeftFacing_Mirrored()
        {
            Assert.AreEqual(150f, BulletPatternMath.FacingAngle(-1, 30f));
        }

        [Test]
        public void AngleToVelocity_Horizontal_PointsRight()
        {
            var vel = BulletPatternMath.AngleToVelocity(0f, 10f);
            Assert.AreEqual(10f, vel.x, 0.001f);
            Assert.AreEqual(0f, vel.y, 0.001f);
        }

        [Test]
        public void AngleToVelocity_Vertical_PointsUp()
        {
            var vel = BulletPatternMath.AngleToVelocity(90f, 10f);
            Assert.AreEqual(0f, vel.x, 0.001f);
            Assert.AreEqual(10f, vel.y, 0.001f);
        }

        [Test]
        public void Reflect_FloorBounce_InvertsY()
        {
            var vel = new Vector2(10f, -5f);
            var reflected = BulletPatternMath.Reflect(vel, Vector2.up);
            Assert.AreEqual(10f, reflected.x, 0.001f);
            Assert.AreEqual(5f, reflected.y, 0.001f);
        }
    }
}
