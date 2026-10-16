using System.Collections.Generic;
using GraceRun.Abilities;
using Unity.Collections;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Server-side glue between <see cref="DraftDeck"/> and the game: builds the pool from
    /// <see cref="RunConfig"/>, leaves out what a loadout already holds, and owns the run's random numbers.
    /// </summary>
    public class DraftDealer
    {
        private readonly RunConfig _config;
        private readonly AbilityCatalog _catalog;
        private readonly System.Random _random;
        private readonly List<DraftCandidate> _pool = new();
        private readonly HashSet<int> _excluded = new();

        public DraftDealer(RunConfig config, AbilityCatalog catalog, int seed)
        {
            _config = config;
            _catalog = catalog;
            _random = new System.Random(seed);
            foreach (var ability in config.DraftPool)
            {
                var id = catalog.IdOf(ability);
                if (id == AbilityCatalog.None) Debug.LogError($"Draft pool ability {ability} is not in the AbilityCatalog.", config);
                else _pool.Add(new DraftCandidate(id, ability.Rarity, ability.MinFloor));
            }
        }

        /// <summary>A new hand on <paramref name="floor"/> without the abilities in <paramref name="loadout"/> or in <paramref name="alsoExclude"/>.</summary>
        public FixedList32Bytes<short> Deal(AbilityLoadout loadout, FixedList32Bytes<short> alsoExclude, int floor)
        {
            _excluded.Clear();
            for (var slot = 0; loadout != null && slot < AbilityLoadout.SlotCount; slot++)
            {
                var owned = loadout.Get(slot);
                if (owned != null) _excluded.Add(_catalog.IdOf(owned));
            }
            foreach (var id in alsoExclude) _excluded.Add(id);

            var dealt = DraftDeck.Deal(_pool, _excluded, floor, _config.LegendaryMinFloor,
                _config.RarityWeights, _config.CardsOffered, _random);
            var cards = new FixedList32Bytes<short>();
            foreach (var id in dealt) cards.Add((short)id);
            return cards;
        }
    }
}
