using System.Collections.Generic;
using GraceRun.Bosses.Behaviours;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class StormWyrmMathTests
    {
        [Test]
        public void CollectColumnX_PutsOneColumnUnderEachSpreadPlayer_Sorted()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { 5f, -3f, 1f }, 1f, 0f, 14f, 8, result);
            CollectionAssert.AreEqual(new[] { -3f, 1f, 5f }, result);
        }

        [Test]
        public void CollectColumnX_MergesPlayersCloserThanMergeDistance()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -6f, -5.2f, 8f }, 1.5f, 0f, 14f, 8, result);
            CollectionAssert.AreEqual(new[] { -6f, 8f }, result);
        }

        [Test]
        public void CollectColumnX_KeepsPlayersExactlyMergeDistanceApart()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { 0f, 2f }, 2f, 0f, 14f, 8, result);
            CollectionAssert.AreEqual(new[] { 0f, 2f }, result);
        }

        [Test]
        public void CollectColumnX_FlankGoesTowardArenaCentreFromNegativeSide()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -4f }, 1f, 5f, 14f, 4, result);
            CollectionAssert.AreEqual(new[] { -4f, 1f }, result);
        }

        [Test]
        public void CollectColumnX_FlankGoesTowardArenaCentreFromPositiveSide()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { 4f }, 1f, 5f, 14f, 4, result);
            CollectionAssert.AreEqual(new[] { 4f, -1f }, result);
        }

        [Test]
        public void CollectColumnX_SkipsFlankWhenCrowded()
        {
            var result = new List<float>();
            // -4 flank would go to 1, which is 0.5 from the 0.5 player column: skipped.
            // 0.5 flank would go to -4.5, which is 0.5 from the -4 column: skipped.
            StormWyrmMath.CollectColumnX(new[] { -4f, 0.5f }, 1.5f, 5f, 14f, 8, result);
            CollectionAssert.AreEqual(new[] { -4f, 0.5f }, result);
        }

        [Test]
        public void CollectColumnX_NoFlanksWhenFlankOffsetIsZero()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -4f }, 1f, 0f, 14f, 4, result);
            CollectionAssert.AreEqual(new[] { -4f }, result);
        }

        [Test]
        public void CollectColumnX_ClampsPlayersToHalfWidth()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { 20f }, 1f, 0f, 14f, 4, result);
            CollectionAssert.AreEqual(new[] { 14f }, result);
        }

        [Test]
        public void CollectColumnX_CapsAtMaxColumnsKeepingPlayersFirst()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -10f, -5f, 0f, 5f, 10f }, 1f, 0f, 14f, 3, result);
            CollectionAssert.AreEqual(new[] { -10f, -5f, 0f }, result);
        }

        [Test]
        public void CollectColumnX_FlanksDoNotPushOutPlayerColumnsWhenCapped()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -10f, 0f }, 1f, 5f, 14f, 2, result);
            CollectionAssert.AreEqual(new[] { -10f, 0f }, result);
        }

        [Test]
        public void CollectColumnX_PartialFlankFillsRemainingCap()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -10f }, 1f, 5f, 14f, 2, result);
            CollectionAssert.AreEqual(new[] { -10f, -5f }, result);
        }

        [Test]
        public void CollectColumnX_ZeroMaxColumnsGivesEmpty()
        {
            var result = new List<float>();
            StormWyrmMath.CollectColumnX(new[] { -4f, 4f }, 1f, 5f, 14f, 0, result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void CollectColumnX_NullInputGivesEmptyAndClearsResult()
        {
            var result = new List<float> { 1f, 2f };
            StormWyrmMath.CollectColumnX(null, 1f, 5f, 14f, 4, result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void CollectColumnX_EmptyInputGivesEmpty()
        {
            var result = new List<float> { 3f };
            StormWyrmMath.CollectColumnX(new float[0], 1f, 5f, 14f, 4, result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void PickBroken_NeverBreaksAllPlatformsWhenCountIsMoreThanOne()
        {
            for (var seed = 0; seed < 100; seed++)
            {
                var result = new List<int>();
                StormWyrmMath.PickBroken(3, seed, 1f, result);
                Assert.AreEqual(2, result.Count, "seed " + seed);
            }
        }

        [Test]
        public void PickBroken_BreaksAtLeastOneWhenFractionIsZero()
        {
            var result = new List<int>();
            StormWyrmMath.PickBroken(4, 7, 0f, result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void PickBroken_BreaksRoundedUpFractionOfPlatforms()
        {
            var result = new List<int>();
            StormWyrmMath.PickBroken(5, 3, 0.5f, result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void PickBroken_SingleplatformAlwaysBreaks()
        {
            var result = new List<int>();
            StormWyrmMath.PickBroken(1, 99, 0f, result);
            CollectionAssert.AreEqual(new[] { 0 }, result);
        }

        [Test]
        public void PickBroken_ZeroCountGivesEmpty()
        {
            var result = new List<int> { 5 };
            StormWyrmMath.PickBroken(0, 1, 0.5f, result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void PickBroken_SameSeedGivesSamePicks()
        {
            var first = new List<int>();
            var second = new List<int>();
            StormWyrmMath.PickBroken(6, 42, 0.5f, first);
            StormWyrmMath.PickBroken(6, 42, 0.5f, second);
            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void PickBroken_PicksAreDistinctValidIndices()
        {
            for (var seed = 0; seed < 50; seed++)
            {
                var result = new List<int>();
                StormWyrmMath.PickBroken(6, seed, 0.5f, result);
                var seen = new HashSet<int>(result);
                Assert.AreEqual(result.Count, seen.Count, "duplicate index for seed " + seed);
                foreach (var index in result)
                    Assert.IsTrue(index >= 0 && index < 6, "index out of range for seed " + seed);
            }
        }

        [Test]
        public void BreakStateAt_NegativeElapsedIsIntact() =>
            Assert.AreEqual(PlatformBreakState.Intact, StormWyrmMath.BreakStateAt(-0.1f, 1f, 2f));

        [Test]
        public void BreakStateAt_StartsCracking() =>
            Assert.AreEqual(PlatformBreakState.Cracking, StormWyrmMath.BreakStateAt(0f, 1f, 2f));

        [Test]
        public void BreakStateAt_InsideCrackIsCracking() =>
            Assert.AreEqual(PlatformBreakState.Cracking, StormWyrmMath.BreakStateAt(0.5f, 1f, 2f));

        [Test]
        public void BreakStateAt_AtCrackEndIsBroken() =>
            Assert.AreEqual(PlatformBreakState.Broken, StormWyrmMath.BreakStateAt(1f, 1f, 2f));

        [Test]
        public void BreakStateAt_InsideBrokenIsBroken() =>
            Assert.AreEqual(PlatformBreakState.Broken, StormWyrmMath.BreakStateAt(2.5f, 1f, 2f));

        [Test]
        public void BreakStateAt_AtBrokenEndIsIntactAgain() =>
            Assert.AreEqual(PlatformBreakState.Intact, StormWyrmMath.BreakStateAt(3f, 1f, 2f));

        [Test]
        public void BreakStateAt_LongAfterIsIntact() =>
            Assert.AreEqual(PlatformBreakState.Intact, StormWyrmMath.BreakStateAt(10f, 1f, 2f));

        [Test]
        public void ColumnPhaseAt_BeforeDelayIsTelegraph() =>
            Assert.AreEqual(ColumnPhase.Telegraph, StormWyrmMath.ColumnPhaseAt(0.2f, 0.5f, 1f, 0.25f));

        [Test]
        public void ColumnPhaseAt_AtDelayIsStrike() =>
            Assert.AreEqual(ColumnPhase.Strike, StormWyrmMath.ColumnPhaseAt(0.5f, 0.5f, 1f, 0.25f));

        [Test]
        public void ColumnPhaseAt_InsideStrikeIsStrike() =>
            Assert.AreEqual(ColumnPhase.Strike, StormWyrmMath.ColumnPhaseAt(1f, 0.5f, 1f, 0.25f));

        [Test]
        public void ColumnPhaseAt_AtStrikeEndIsFade() =>
            Assert.AreEqual(ColumnPhase.Fade, StormWyrmMath.ColumnPhaseAt(1.5f, 0.5f, 1f, 0.25f));

        [Test]
        public void ColumnPhaseAt_InsideFadeIsFade() =>
            Assert.AreEqual(ColumnPhase.Fade, StormWyrmMath.ColumnPhaseAt(1.7f, 0.5f, 1f, 0.25f));

        [Test]
        public void ColumnPhaseAt_AtFadeEndIsDone() =>
            Assert.AreEqual(ColumnPhase.Done, StormWyrmMath.ColumnPhaseAt(1.75f, 0.5f, 1f, 0.25f));

        [Test]
        public void ColumnPhaseAt_LongAfterIsDone() =>
            Assert.AreEqual(ColumnPhase.Done, StormWyrmMath.ColumnPhaseAt(5f, 0.5f, 1f, 0.25f));
    }
}
