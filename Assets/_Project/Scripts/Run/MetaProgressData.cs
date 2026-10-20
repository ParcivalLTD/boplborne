using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Persistent meta-progression data saved locally per player as JSON (docs/ARCHITECTURE.md).
    /// Tracks run milestones and unlocked abilities, hats, and boss alternate pools.
    /// Never includes permanent stat boosts (docs/GDD.md).
    /// </summary>
    [Serializable]
    public class MetaProgressData
    {
        [SerializeField] private int _version = 1;
        [SerializeField] private int _highestFloorReached = 1;
        [SerializeField] private int _totalRuns = 0;
        [SerializeField] private int _totalVictories = 0;
        [SerializeField] private List<string> _unlockedAbilities = new();
        [SerializeField] private List<string> _unlockedHats = new();
        [SerializeField] private List<string> _unlockedBossPools = new();

        public int Version => _version;
        public int HighestFloorReached { get => _highestFloorReached; set => _highestFloorReached = value; }
        public int TotalRuns { get => _totalRuns; set => _totalRuns = value; }
        public int TotalVictories { get => _totalVictories; set => _totalVictories = value; }

        public List<string> UnlockedAbilities => _unlockedAbilities;
        public List<string> UnlockedHats => _unlockedHats;
        public List<string> UnlockedBossPools => _unlockedBossPools;

        public MetaProgressData()
        {
            _version = 1;
            _highestFloorReached = 1;
            _totalRuns = 0;
            _totalVictories = 0;
            _unlockedAbilities = new List<string>();
            _unlockedHats = new List<string>();
            _unlockedBossPools = new List<string>();
        }
    }
}
