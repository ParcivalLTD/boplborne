using GraceRun.UI.Toon;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ToonMotionTests
    {
        [Test]
        public void Spring_SettlesOnItsTarget()
        {
            var spring = new Spring(0f);
            for (var i = 0; i < 240; i++) spring.Step(1f, 1f / 60f);
            Assert.IsTrue(spring.IsAt(1f));
        }

        [Test]
        public void Spring_WithLowDamping_OvershootsBeforeSettling()
        {
            var spring = new Spring(0f);
            var peak = 0f;
            for (var i = 0; i < 120; i++)
            {
                spring.Step(1f, 1f / 60f, 9f, 0.3f);
                if (spring.Value > peak) peak = spring.Value;
            }
            Assert.Greater(peak, 1.05f);
        }

        [Test]
        public void Spring_WithFullDamping_DoesNotOvershoot()
        {
            var spring = new Spring(0f);
            var peak = 0f;
            for (var i = 0; i < 240; i++)
            {
                spring.Step(1f, 1f / 60f, 9f, 1f);
                if (spring.Value > peak) peak = spring.Value;
            }
            Assert.LessOrEqual(peak, 1.001f);
        }

        [Test]
        public void Spring_SurvivesAHugeFrame()
        {
            var spring = new Spring(0f);
            spring.Step(1f, 5f);
            Assert.IsFalse(float.IsNaN(spring.Value));
            Assert.Less(System.Math.Abs(spring.Value), 5f);
        }

        [Test]
        public void Stepped_HoldsEachFrameForOneTick()
        {
            Assert.AreEqual(0f, ToonMotion.Stepped(0.03f, 12f), 0.0001f);
            Assert.AreEqual(1f / 12f, ToonMotion.Stepped(0.09f, 12f), 0.0001f);
        }

        [Test]
        public void PopCurve_StartsAtZeroOvershootsAndEndsAtOne()
        {
            Assert.AreEqual(0f, ToonMotion.PopCurve(0f), 0.0001f);
            Assert.AreEqual(1f, ToonMotion.PopCurve(1f), 0.0001f);
            var peak = 0f;
            for (var i = 0; i <= 100; i++) peak = System.Math.Max(peak, ToonMotion.PopCurve(i / 100f));
            Assert.Greater(peak, 1.05f);
        }
    }
}
