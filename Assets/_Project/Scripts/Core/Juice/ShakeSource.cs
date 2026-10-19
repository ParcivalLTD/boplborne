using Unity.Cinemachine;
using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Scene-side half of <see cref="Shake"/>: fires uniform Cinemachine impulses that the
    /// arena camera's impulse listener turns into camera offset.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class ShakeSource : MonoBehaviour
    {
        [SerializeField] private ShakeData _data;

        private CinemachineImpulseSource _impulse;

        public float TestStrength => _data.TestStrength;

        private void Awake()
        {
            _impulse = GetComponent<CinemachineImpulseSource>();
            var definition = _impulse.ImpulseDefinition;
            definition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            definition.ImpulseShape = _data.Shape;
            definition.ImpulseDuration = _data.Duration;
        }

        private void OnEnable() => Shake.Register(this);

        private void OnDisable() => Shake.Unregister(this);

        /// <summary>Fires one shake in a random direction.</summary>
        public void Play(float strength)
        {
            var force = Shake.ComputeForce(strength, _data.StrengthMultiplier * Settings.GameSettings.ShakeMultiplier, _data.BaseForce);
            if (force <= 0f) return;
            var direction = Random.insideUnitCircle.normalized;
            if (direction == Vector2.zero) direction = Vector2.down;
            _impulse.GenerateImpulseWithVelocity(direction * force);
        }
    }
}
