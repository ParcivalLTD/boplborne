using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>Numbers for the training dummy. Tune the asset under Data/Config, never the code.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Training Dummy Data", fileName = "TrainingDummyData")]
    public class TrainingDummyData : ScriptableObject, IHealthStats
    {
        [SerializeField, Min(1)] private int _maxHP = 200;
        [SerializeField, Min(0f)] private float _refillDelay = 1.5f;

        [SerializeField, Min(1f)] private float _idleFps = 8f;
        [SerializeField, Min(1f)] private float _hitFps = 12f;

        public int MaxHP => _maxHP;

        /// <summary>Frame rate of the idle sway.</summary>
        public float IdleFps => _idleFps;

        /// <summary>Frame rate of the hit reaction clip, which plays once per hit.</summary>
        public float HitFps => _hitFps;

        /// <summary>Seconds at 0 HP before the dummy refills to full.</summary>
        public float RefillDelay => _refillDelay;
    }
}
