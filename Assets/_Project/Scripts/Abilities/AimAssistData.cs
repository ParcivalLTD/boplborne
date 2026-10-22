using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// How keyboard and gamepad ability aim helps: the cone and range in which an ability snaps to a
    /// boss, and the look of the aim preview. Referenced by the <see cref="AbilityCatalog"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Aim Assist Data", fileName = "AimAssistData")]
    public class AimAssistData : ScriptableObject
    {
        [SerializeField, Range(0f, 90f), Tooltip("Half-angle (degrees) around your aim in which an enemy pulls the aim onto itself.")]
        private float _coneDegrees = 28f;
        [SerializeField, Min(0f), Tooltip("Enemies farther than this (units) are never snapped to.")]
        private float _maxRange = 24f;
        [SerializeField, Min(0f), Tooltip("Length of the aim line while you hold a direction or hold to aim.")]
        private float _previewLength = 5f;
        [SerializeField, Min(0.1f), Tooltip("Radius of the ring drawn on the enemy the aim has snapped to.")]
        private float _ringRadius = 0.9f;
        [SerializeField, Min(0f)] private float _lineWidth = 0.1f;
        [SerializeField] private Color _color = new(0.95f, 0.91f, 0.84f, 0.9f);

        public float ConeDegrees => _coneDegrees;
        public float MaxRange => _maxRange;
        public float PreviewLength => _previewLength;
        public float RingRadius => _ringRadius;
        public float LineWidth => _lineWidth;
        public Color Color => _color;
    }
}
