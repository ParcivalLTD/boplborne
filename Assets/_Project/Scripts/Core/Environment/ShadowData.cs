using UnityEngine;

namespace GraceRun.Core.Environment
{
    /// <summary>
    /// How the soft ground shadow under every body looks (see <see cref="GroundShadow"/>): colour and
    /// strength, how it shrinks and fades as the body rises, and how flat the ellipse is.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Shadow Data", fileName = "ShadowData")]
    public class ShadowData : ScriptableObject
    {
        [SerializeField, Tooltip("Shadow colour; the alpha is the strength on the floor.")]
        private Color _color = new(0.07f, 0.06f, 0.09f, 0.38f);
        [SerializeField, Min(0.05f), Tooltip("Height of the ellipse as a share of its width.")]
        private float _flatness = 0.28f;
        [SerializeField, Min(0.5f), Tooltip("A body this far (units) above the ground casts no shadow.")]
        private float _fadeHeight = 7f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Size of the shadow at the fade height, against its size on the floor.")]
        private float _smallestScale = 0.45f;
        [SerializeField, Min(1f), Tooltip("How far down (units) the shadow looks for ground.")]
        private float _searchDistance = 12f;

        [SerializeField, Tooltip("Arena: shadow tint under team A players (P1, P3).")]
        private Color _teamAColor = new(0.3f, 0.55f, 1f, 0.6f);
        [SerializeField, Tooltip("Arena: shadow tint under team B players (P2, P4).")]
        private Color _teamBColor = new(1f, 0.6f, 0.24f, 0.6f);

        public Color TeamAColor => _teamAColor;
        public Color TeamBColor => _teamBColor;
        public Color Color => _color;
        public float Flatness => _flatness;
        public float FadeHeight => _fadeHeight;
        public float SmallestScale => _smallestScale;
        public float SearchDistance => _searchDistance;
    }
}
