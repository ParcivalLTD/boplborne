using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Bosses.Behaviours;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class MirrorMathTests
    {
        [Test]
        public void KindOf_NoTagsIsSlash() =>
            Assert.AreEqual(MirrorKind.Slash, MirrorMath.KindOf(AbilityTags.None));

        [TestCase(AbilityTags.Projectile)]
        [TestCase(AbilityTags.Summon)]
        [TestCase(AbilityTags.Projectile | AbilityTags.Summon)]
        public void KindOf_ProjectileOrSummonIsShard(AbilityTags tags) =>
            Assert.AreEqual(MirrorKind.Shard, MirrorMath.KindOf(tags));

        [Test]
        public void KindOf_MovementIsDash() =>
            Assert.AreEqual(MirrorKind.Dash, MirrorMath.KindOf(AbilityTags.Movement));

        [TestCase(AbilityTags.Area)]
        [TestCase(AbilityTags.Area | AbilityTags.Projectile)]
        [TestCase(AbilityTags.Area | AbilityTags.Movement)]
        public void KindOf_AreaIsBurstAndWinsOverOthers(AbilityTags tags) =>
            Assert.AreEqual(MirrorKind.Burst, MirrorMath.KindOf(tags));

        [Test]
        public void KindOf_ProjectileBeatsMovement() =>
            Assert.AreEqual(MirrorKind.Shard, MirrorMath.KindOf(AbilityTags.Projectile | AbilityTags.Movement));

        [TestCase(3f, 1f, -1f)]
        [TestCase(-2f, 0f, 2f)]
        [TestCase(5f, 5f, 5f)]
        public void Reflect_MirrorsAcrossPivot(float x, float pivot, float expected) =>
            Assert.AreEqual(expected, MirrorMath.Reflect(x, pivot), 0.001f);

        [TestCase(5f, 0f, 1f, 3f, 3f)]
        [TestCase(-2f, 0f, 1f, 3f, 1f)]
        [TestCase(2f, 0f, 1f, 3f, 2f)]
        [TestCase(10f, -4f, 1f, 3f, -1f)]
        [TestCase(-10f, -4f, 1f, 3f, -3f)]
        public void ShardY_ClampsTargetHeightBetweenKneeAndHead(float targetY, float floorY, float minAbove, float maxAbove, float expected) =>
            Assert.AreEqual(expected, MirrorMath.ShardY(targetY, floorY, minAbove, maxAbove), 0.001f);

        [Test]
        public void ReflectionX_IndexZeroReflectsAcrossTarget() =>
            Assert.AreEqual(-1f, MirrorMath.ReflectionX(0, 5f, 2f, 14f), 0.001f);

        [Test]
        public void ReflectionX_IndexOneReflectsAcrossArenaCentre() =>
            Assert.AreEqual(10f, MirrorMath.ReflectionX(1, -10f, -4f, 14f), 0.001f);

        [Test]
        public void ReflectionX_IndexZeroClampsToHalfWidth()
        {
            Assert.AreEqual(8f, MirrorMath.ReflectionX(0, -12f, 6f, 8f), 0.001f);
            Assert.AreEqual(-8f, MirrorMath.ReflectionX(0, 12f, -6f, 8f), 0.001f);
        }

        [Test]
        public void ReflectionX_IndexOneClampsToHalfWidth() =>
            Assert.AreEqual(8f, MirrorMath.ReflectionX(1, -12f, -4f, 8f), 0.001f);

        [Test]
        public void SeparatedX_FarEnoughStaysPut() =>
            Assert.AreEqual(8f, MirrorMath.SeparatedX(2f, 8f, 3f, 14f), 0.001f);

        [Test]
        public void SeparatedX_OverlapIsPushedToItsOwnSide() =>
            Assert.AreEqual(5f, MirrorMath.SeparatedX(2f, 3f, 3f, 14f), 0.001f);

        [Test]
        public void SeparatedX_ExactSpotGoesRight() =>
            Assert.AreEqual(5f, MirrorMath.SeparatedX(2f, 2f, 3f, 14f), 0.001f);

        [Test]
        public void SeparatedX_PushedPastTheWallFlipsSides()
        {
            var x = MirrorMath.SeparatedX(12f, 13f, 3f, 14f);
            Assert.AreEqual(9f, x, 0.001f);
        }

        [TestCase(-6f, -5f)]
        [TestCase(0f, 0.5f)]
        [TestCase(7f, 6f)]
        public void SeparatedX_ResultIsAlwaysApartAndInsideTheArena(float first, float second)
        {
            var x = MirrorMath.SeparatedX(first, second, 3f, 14f);
            Assert.GreaterOrEqual(Mathf.Abs(x - first), 2.99f);
            Assert.LessOrEqual(Mathf.Abs(x), 14f);
        }
    }
}
