using GraceRun.Core;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor
{
    /// <summary>
    /// Applies the 2D collision matrix from docs/ARCHITECTURE.md ("Physics layers").
    /// Game layers collide only with the partners listed here; built-in layers are left alone.
    /// Run it again from the menu whenever the table changes.
    /// </summary>
    public static class LayerCollisionSetup
    {
        private const int LayerCount = 32;

        private static readonly int[] GameLayers =
        {
            Layers.GroundIndex, Layers.PlayerIndex, Layers.GhostIndex, Layers.BossIndex,
            Layers.PlayerAttackIndex, Layers.BossAttackIndex, Layers.AbilityIndex, Layers.PlatformIndex,
        };

        // Each pair collides both ways. Anything involving a game layer that is not listed is ignored.
        private static readonly (int, int)[] CollidingPairs =
        {
            (Layers.GroundIndex, Layers.PlayerIndex),
            (Layers.GroundIndex, Layers.BossIndex),
            (Layers.GroundIndex, Layers.AbilityIndex),
            (Layers.PlayerIndex, Layers.AbilityIndex),
            (Layers.PlayerIndex, Layers.PlayerAttackIndex),
            (Layers.PlayerIndex, Layers.BossAttackIndex),
            (Layers.BossIndex, Layers.PlayerAttackIndex),
            (Layers.BossIndex, Layers.AbilityIndex),
            (Layers.PlayerAttackIndex, Layers.AbilityIndex),
            (Layers.AbilityIndex, Layers.AbilityIndex),
            (Layers.PlatformIndex, Layers.PlayerIndex),
        };

        [MenuItem("Grace Run/Apply 2D Layer Collision Matrix")]
        public static void Apply()
        {
            foreach (int a in GameLayers)
            {
                for (int b = 0; b < LayerCount; b++)
                {
                    Physics2D.IgnoreLayerCollision(a, b, !Collides(a, b));
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Grace Run: 2D layer collision matrix applied.");
        }

        private static bool Collides(int a, int b)
        {
            foreach ((int x, int y) in CollidingPairs)
            {
                if ((x == a && y == b) || (x == b && y == a)) return true;
            }

            return false;
        }
    }
}
