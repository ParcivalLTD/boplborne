using System;
using GraceRun.Bosses.Behaviours;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Waves of small enemies a boss fight adds on top of the boss: every <see cref="Interval"/> seconds,
    /// <see cref="PerWave"/> crawlers drop in from the arena edges, never more than <see cref="MaxAlive"/> at once.
    /// </summary>
    [Serializable]
    public struct SwarmSpec
    {
        [Tooltip("The minion to send; empty = no swarm.")]
        public BroodCrawler Crawler;
        [Min(1)] public int PerWave;
        [Min(1)] public int MaxAlive;
        [Min(1f), Tooltip("Seconds between waves.")]
        public float Interval;
        [Min(0f), Tooltip("Seconds after the fight starts until the first wave.")]
        public float FirstDelay;
        [Min(1f), Tooltip("Half the playfield width; crawlers appear at its edges.")]
        public float ArenaHalfWidth;
        [Tooltip("Height of the drop-in point above the boss spawn point.")]
        public float SpawnHeight;
    }
}
