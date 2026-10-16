using System.Collections.Generic;
using GraceRun.Bosses.Behaviours;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class BroodMotherMathTests
    {
        [TestCase(-20f, 14f, -14f)]
        [TestCase(5f, 14f, 5f)]
        [TestCase(18f, 14f, 14f)]
        public void ClampArenaX_BoundsToHalfWidth(float x, float halfWidth, float expected) =>
            Assert.AreEqual(expected, BroodMotherMath.ClampArenaX(x, halfWidth));

        [TestCase(2, 0, 4, 2)]
        [TestCase(2, 3, 4, 1)]
        [TestCase(2, 4, 4, 0)]
        [TestCase(2, 5, 4, 0)]
        public void CrawlersToSpawn_CapsAtMaxAlive(int requested, int alive, int maxAlive, int expected) =>
            Assert.AreEqual(expected, BroodMotherMath.CrawlersToSpawn(requested, alive, maxAlive));

        [Test]
        public void IsInWebZone_ChecksHorizontalRadiusAndVerticalHeight()
        {
            var web = new Vector2(4f, -5f);
            Assert.IsTrue(BroodMotherMath.IsInWebZone(new Vector2(5.5f, -4.4f), web, 2.5f, 1f));
            Assert.IsFalse(BroodMotherMath.IsInWebZone(new Vector2(7.0f, -4.4f), web, 2.5f, 1f));
            Assert.IsFalse(BroodMotherMath.IsInWebZone(new Vector2(4.0f, -2.5f), web, 2.5f, 1f));
        }

        [Test]
        public void CollectWebZoneX_SoloPlayer_AddsFlankingSecondZone()
        {
            var result = new List<float>();
            BroodMotherMath.CollectWebZoneX(new[] { -4f }, 1, 5f, 1.5f, 14f, 4, result);
            CollectionAssert.AreEqual(new[] { -4f, 1f }, result);
        }

        [Test]
        public void CollectWebZoneX_MergesStackedPlayersAndKeepsSpreadPlayers()
        {
            var result = new List<float>();
            BroodMotherMath.CollectWebZoneX(new[] { -6f, -5.2f, 8f }, -1, 5f, 1.5f, 14f, 4, result);
            CollectionAssert.AreEqual(new[] { -6f, 8f }, result);
        }

        [Test]
        public void FadeAlpha_HoldsBaseThenFadesLinearly()
        {
            Assert.AreEqual(0.6f, BroodMotherMath.FadeAlpha(1f, 5f, 1f, 0.6f), 0.001f);
            Assert.AreEqual(0.3f, BroodMotherMath.FadeAlpha(4.5f, 5f, 1f, 0.6f), 0.001f);
            Assert.AreEqual(0f, BroodMotherMath.FadeAlpha(5f, 5f, 1f, 0.6f), 0.001f);
        }
    }
}
