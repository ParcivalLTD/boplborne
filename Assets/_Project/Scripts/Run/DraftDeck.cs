using System;
using System.Collections.Generic;
using GraceRun.Abilities;

namespace GraceRun.Run
{
    /// <summary>
    /// Pure draft dealing (docs/GDD.md, Draft): each card first rolls a rarity by its weight among
    /// the rarities still on offer, then picks an ability of that rarity at random. Abilities the
    /// player owns, abilities above the floor and legendaries before their floor are never dealt,
    /// and a hand never holds the same ability twice.
    /// </summary>
    public static class DraftDeck
    {
        private static readonly AbilityRarity[] Rarities =
            { AbilityRarity.Common, AbilityRarity.Rare, AbilityRarity.Legendary };

        /// <summary>
        /// Up to <paramref name="count"/> distinct catalog ids from <paramref name="pool"/>; fewer when
        /// not enough abilities are eligible. <paramref name="excluded"/> holds ids that must not be dealt.
        /// </summary>
        public static List<int> Deal(IReadOnlyList<DraftCandidate> pool, ICollection<int> excluded, int floor,
            int legendaryMinFloor, RarityWeights weights, int count, Random random)
        {
            var eligible = new List<DraftCandidate>();
            foreach (var candidate in pool)
                if (IsEligible(candidate, excluded, floor, legendaryMinFloor, weights) && !Contains(eligible, candidate.Id))
                    eligible.Add(candidate);

            var hand = new List<int>(count);
            while (hand.Count < count && eligible.Count > 0)
            {
                var rarity = RollRarity(eligible, weights, random);
                var index = PickOfRarity(eligible, rarity, random);
                hand.Add(eligible[index].Id);
                eligible.RemoveAt(index);
            }
            return hand;
        }

        /// <summary>True when <paramref name="candidate"/> may be dealt on <paramref name="floor"/>.</summary>
        public static bool IsEligible(DraftCandidate candidate, ICollection<int> excluded, int floor,
            int legendaryMinFloor, RarityWeights weights)
        {
            if (excluded != null && excluded.Contains(candidate.Id)) return false;
            if (candidate.MinFloor > floor) return false;
            if (candidate.Rarity == AbilityRarity.Legendary && floor < legendaryMinFloor) return false;
            return weights.For(candidate.Rarity) > 0f;
        }

        private static AbilityRarity RollRarity(List<DraftCandidate> eligible, RarityWeights weights, Random random)
        {
            var total = 0f;
            foreach (var rarity in Rarities)
                if (HasRarity(eligible, rarity)) total += weights.For(rarity);

            var roll = random.NextDouble() * total;
            foreach (var rarity in Rarities)
            {
                if (!HasRarity(eligible, rarity)) continue;
                roll -= weights.For(rarity);
                if (roll < 0d) return rarity;
            }
            return eligible[eligible.Count - 1].Rarity;
        }

        private static int PickOfRarity(List<DraftCandidate> eligible, AbilityRarity rarity, Random random)
        {
            var matches = 0;
            foreach (var candidate in eligible)
                if (candidate.Rarity == rarity) matches++;

            var pick = random.Next(matches);
            for (var i = 0; i < eligible.Count; i++)
                if (eligible[i].Rarity == rarity && pick-- == 0) return i;
            return eligible.Count - 1;
        }

        private static bool HasRarity(List<DraftCandidate> eligible, AbilityRarity rarity)
        {
            foreach (var candidate in eligible)
                if (candidate.Rarity == rarity) return true;
            return false;
        }

        private static bool Contains(List<DraftCandidate> list, int id)
        {
            foreach (var candidate in list)
                if (candidate.Id == id) return true;
            return false;
        }
    }
}
