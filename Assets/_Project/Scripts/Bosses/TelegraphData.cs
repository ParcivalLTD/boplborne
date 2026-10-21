using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Look of the boss telegraph: the pulse of the gold "hit now" weak point (the art is Art/FX/weak_point.png). Gold is reserved for gameplay
    /// signals (docs/GDD.md); there is no red glow or zone on bosses (decision 26). Asset under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Telegraph Data", fileName = "TelegraphData")]
    public class TelegraphData : ScriptableObject
    {
        [Header("Weak point")]
        [SerializeField, Min(0f)] private float _weakPointPulseRate = 4f;
        [SerializeField, Min(0f)] private float _weakPointPulseScale = 0.15f;

        public float WeakPointPulseRate => _weakPointPulseRate;
        public float WeakPointPulseScale => _weakPointPulseScale;
    }
}
