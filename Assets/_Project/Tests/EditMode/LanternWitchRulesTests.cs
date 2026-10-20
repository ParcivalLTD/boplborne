using GraceRun.Bosses;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests.EditMode
{
    [TestFixture]
    public class LanternWitchRulesTests
    {
        [Test]
        public void ComputeWispPositions_ReturnsRequestedCount()
        {
            var positions = LanternWitchRules.ComputeWispPositions(Vector2.zero, 4, 15f);
            Assert.AreEqual(4, positions.Length);
        }

        [Test]
        public void ComputeWispPositions_PositionsAreSymmetrical()
        {
            var positions = LanternWitchRules.ComputeWispPositions(Vector2.zero, 4, 10f, angleOffsetDeg: 0f);
            // 0 deg: (10, 0)
            Assert.AreEqual(10f, positions[0].x, 0.01f);
            Assert.AreEqual(0f, positions[0].y, 0.01f);

            // 180 deg: (-10, 0)
            Assert.AreEqual(-10f, positions[2].x, 0.01f);
            Assert.AreEqual(0f, positions[2].y, 0.01f);
        }

        [Test]
        public void IsInsideLight_WhenWithinRadius_ReturnsTrue()
        {
            var centers = new[] { new Vector2(0f, 0f) };
            Assert.IsTrue(LanternWitchRules.IsInsideLight(new Vector2(2f, 2f), centers, 5f));
        }

        [Test]
        public void IsInsideLight_WhenOutsideRadius_ReturnsFalse()
        {
            var centers = new[] { new Vector2(0f, 0f) };
            Assert.IsFalse(LanternWitchRules.IsInsideLight(new Vector2(10f, 10f), centers, 5f));
        }

        [Test]
        public void IsInsideLight_WithMultipleCenters_ChecksAnySource()
        {
            var centers = new[] { new Vector2(-15f, 0f), new Vector2(15f, 0f) };

            // Point near second light
            Assert.IsTrue(LanternWitchRules.IsInsideLight(new Vector2(14f, 1f), centers, 5f));

            // Point in the dark middle between lights
            Assert.IsFalse(LanternWitchRules.IsInsideLight(new Vector2(0f, 0f), centers, 5f));
        }

        [Test]
        public void ComputeBeamEnd_CalculatesDirectionalOffset()
        {
            var origin = new Vector2(5f, 5f);
            var end = LanternWitchRules.ComputeBeamEnd(origin, 0f, 20f); // 0 degrees = right
            Assert.AreEqual(25f, end.x, 0.01f);
            Assert.AreEqual(5f, end.y, 0.01f);
        }

        [Test]
        public void GloomAlpha_WhenLit_ReturnsLitAlpha()
        {
            var alpha = LanternWitchRules.GloomAlpha(true, darkAlpha: 0.8f, litAlpha: 0.1f);
            Assert.AreEqual(0.1f, alpha);
        }

        [Test]
        public void GloomAlpha_WhenDark_ReturnsDarkAlpha()
        {
            var alpha = LanternWitchRules.GloomAlpha(false, darkAlpha: 0.8f, litAlpha: 0.1f);
            Assert.AreEqual(0.8f, alpha);
        }
    }
}
