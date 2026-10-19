using GraceRun.Core;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    /// <summary>Guards against the Layers constants drifting from Project Settings > Tags and Layers.</summary>
    public class LayersTests
    {
        [TestCase(Layers.Ground, Layers.GroundIndex)]
        [TestCase(Layers.Player, Layers.PlayerIndex)]
        [TestCase(Layers.Ghost, Layers.GhostIndex)]
        [TestCase(Layers.Boss, Layers.BossIndex)]
        [TestCase(Layers.PlayerAttack, Layers.PlayerAttackIndex)]
        [TestCase(Layers.BossAttack, Layers.BossAttackIndex)]
        [TestCase(Layers.Ability, Layers.AbilityIndex)]
        [TestCase(Layers.Platform, Layers.PlatformIndex)]
        public void LayerConstantMatchesProjectSettings(string layerName, int expectedIndex)
        {
            Assert.AreEqual(expectedIndex, LayerMask.NameToLayer(layerName));
        }
    }
}
