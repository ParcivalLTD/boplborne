using Unity.Cinemachine;
using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Screen shake tuning. The strength multiplier is the player-facing settings value
    /// (the Settings slider writes it later); everything else shapes a strength-1 shake.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Shake Data", fileName = "ShakeData")]
    public class ShakeData : ScriptableObject
    {
        [SerializeField, Range(0f, 2f)] private float _strengthMultiplier = 1f;
        [SerializeField, Min(0f), Tooltip("Camera offset in world units for a strength-1 shake.")]
        private float _baseForce = 0.4f;
        [SerializeField, Min(0.01f)] private float _duration = 0.25f;
        [SerializeField] private CinemachineImpulseDefinition.ImpulseShapes _shape =
            CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        [SerializeField, Min(0f)] private float _testStrength = 1f;

        /// <summary>Global scale on every shake; 0 turns shake off.</summary>
        public float StrengthMultiplier => _strengthMultiplier;
        public float BaseForce => _baseForce;
        public float Duration => _duration;
        public CinemachineImpulseDefinition.ImpulseShapes Shape => _shape;

        /// <summary>Strength used by the debug menu's test button.</summary>
        public float TestStrength => _testStrength;
    }
}
