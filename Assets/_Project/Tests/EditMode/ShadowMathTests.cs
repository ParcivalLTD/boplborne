using GraceRun.Core.Environment;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ShadowMathTests
    {
        [Test]
        public void OnTheGroundIsFullSizeAndStrength()
        {
            Assert.AreEqual(1f, ShadowMath.Scale(0f, 7f, 0.45f), 0.0001f);
            Assert.AreEqual(1f, ShadowMath.Strength(0f, 7f), 0.0001f);
        }

        [Test]
        public void AtTheFadeHeightIsSmallestAndGone()
        {
            Assert.AreEqual(0.45f, ShadowMath.Scale(7f, 7f, 0.45f), 0.0001f);
            Assert.AreEqual(0f, ShadowMath.Strength(7f, 7f), 0.0001f);
            Assert.AreEqual(0f, ShadowMath.Strength(20f, 7f), 0.0001f);
        }

        [Test]
        public void HalfwayIsBetween()
        {
            Assert.AreEqual(0.725f, ShadowMath.Scale(3.5f, 7f, 0.45f), 0.0001f);
            Assert.AreEqual(0.5f, ShadowMath.Strength(3.5f, 7f), 0.0001f);
        }
    }
}
