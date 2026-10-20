using System;
using UnityEngine;

namespace GraceRun.Core
{
    /// <summary>
    /// A player's three body shades. The sprite shader swaps P1's blue ramp for these
    /// (see docs/ART_PIPELINE.md "Player colours").
    /// </summary>
    [Serializable]
    public struct PlayerRamp
    {
        public Color Dark;
        public Color Mid;
        public Color Light;
    }
}
