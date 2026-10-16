using System.Collections.Generic;
using System.Linq;
using GraceRun.Abilities;
using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class DraftDeckTests
    {
        private const int LegendaryFloor = 4;
        private static readonly RarityWeights Weights = new(70f, 25f, 5f);

        private static readonly DraftCandidate[] Pool =
        {
            new(0, AbilityRarity.Common),
            new(1, AbilityRarity.Rare),
            new(2, AbilityRarity.Common),
            new(3, AbilityRarity.Common),
            new(4, AbilityRarity.Common),
            new(5, AbilityRarity.Common),
            new(6, AbilityRarity.Legendary),
            new(7, AbilityRarity.Rare, minFloor: 3),
        };

        private static List<int> Deal(int floor, ICollection<int> owned = null, int seed = 1, RarityWeights? weights = null,
            int count = 3, IReadOnlyList<DraftCandidate> pool = null) =>
            DraftDeck.Deal(pool ?? Pool, owned ?? new HashSet<int>(), floor, LegendaryFloor, weights ?? Weights, count,
                new System.Random(seed));

        [Test]
        public void DealsThreeDistinctCards()
        {
            for (var seed = 0; seed < 200; seed++)
            {
                var hand = Deal(1, seed: seed);
                Assert.AreEqual(3, hand.Count);
                Assert.AreEqual(3, hand.Distinct().Count());
            }
        }

        [Test]
        public void NeverDealsAnOwnedAbility()
        {
            var owned = new HashSet<int> { 0, 2, 3 };
            for (var seed = 0; seed < 200; seed++)
                CollectionAssert.IsEmpty(Deal(1, owned, seed).Intersect(owned));
        }

        [Test]
        public void NoLegendaryBeforeItsFloor()
        {
            var legendaryOnly = new RarityWeights(0f, 0f, 1f);
            CollectionAssert.IsEmpty(Deal(LegendaryFloor - 1, weights: legendaryOnly));
        }

        [Test]
        public void LegendaryFromItsFloor()
        {
            var legendaryOnly = new RarityWeights(0f, 0f, 1f);
            CollectionAssert.AreEqual(new[] { 6 }, Deal(LegendaryFloor, weights: legendaryOnly));
        }

        [Test]
        public void AbilityAboveTheFloorIsNotDealt()
        {
            var rareOnly = new RarityWeights(0f, 1f, 0f);
            CollectionAssert.AreEquivalent(new[] { 1 }, Deal(2, weights: rareOnly));
            CollectionAssert.AreEquivalent(new[] { 1, 7 }, Deal(3, weights: rareOnly));
        }

        [Test]
        public void ZeroWeightRarityIsNeverDealt()
        {
            var noRares = new RarityWeights(1f, 0f, 1f);
            for (var seed = 0; seed < 200; seed++)
                CollectionAssert.DoesNotContain(Deal(5, seed: seed, weights: noRares), 1);
        }

        [Test]
        public void SmallPoolDealsFewerCards()
        {
            var owned = new HashSet<int> { 0, 1, 2, 3, 4 };
            CollectionAssert.AreEqual(new[] { 5 }, Deal(1, owned));
        }

        [Test]
        public void DuplicatesInThePoolAreDealtOnce()
        {
            var pool = new[] { new DraftCandidate(0, AbilityRarity.Common), new DraftCandidate(0, AbilityRarity.Common) };
            CollectionAssert.AreEqual(new[] { 0 }, Deal(1, pool: pool));
        }

        [Test]
        public void SameSeedDealsTheSameHand() =>
            CollectionAssert.AreEqual(Deal(5, seed: 42), Deal(5, seed: 42));

        [Test]
        public void RarityWeightsShapeTheOdds()
        {
            var rares = 0;
            var cards = 0;
            for (var seed = 0; seed < 2000; seed++)
            {
                var hand = Deal(1, seed: seed, count: 1);
                cards += hand.Count;
                if (hand[0] == 1) rares++;
            }
            // Floor 1: one rare against commons, 70:25 weights, so about 26% of single cards are rare.
            Assert.That(rares / (float)cards, Is.InRange(0.2f, 0.33f));
        }
    }
}
