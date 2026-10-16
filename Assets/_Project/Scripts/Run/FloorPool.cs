using System;
using GraceRun.Bosses;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// The bosses that can appear on one floor of a run (docs/ARCHITECTURE.md, RunConfig).
    /// When a floor has more than one boss, the server picks one from the run seed.
    /// </summary>
    [Serializable]
    public struct FloorPool
    {
        [SerializeField] private BossData[] _bosses;

        public BossData[] Bosses => _bosses;
    }
}
