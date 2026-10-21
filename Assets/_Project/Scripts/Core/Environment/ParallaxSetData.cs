using System;
using UnityEngine;

namespace GraceRun.Core.Environment
{
    /// <summary>One background layer: art, how much it follows the camera, and where it sits.</summary>
    [Serializable]
    public struct ParallaxLayer
    {
        [Tooltip("Layer art. Imported to tile sideways; made seamless by mirroring.")]
        public Sprite Sprite;
        [Tooltip("0 = pinned to the world, 1 = moves with the camera (looks infinitely far).")]
        [Range(0f, 1f)] public float Follow;
        [Tooltip("World units above the camera centre where the sprite centre sits.")]
        public float YOffset;
        [Tooltip("Sorting order; stays far behind the arena art (-20 and below are placeholders' range).")]
        public int SortingOrder;
        public Color Tint;
    }

    /// <summary>The background layers of one arena. Cosmetic only.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Environment/Parallax Set", fileName = "ParallaxSet")]
    public class ParallaxSetData : ScriptableObject
    {
        [SerializeField] private ParallaxLayer[] _layers = Array.Empty<ParallaxLayer>();

        public ParallaxLayer[] Layers => _layers;

        /// <summary>Replaces the layers; used by the editor builder.</summary>
        public void SetLayers(ParallaxLayer[] layers) => _layers = layers;
    }
}
