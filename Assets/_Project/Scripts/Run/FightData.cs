using GraceRun.Bosses;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Tuning for the single-fight loop (M3.3): countdown length, how long "FIGHT!" stays up,
    /// and which boss is fought. Replaced by RunConfig when the run exists (M6).
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Fight Data", fileName = "FightData")]
    public class FightData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _countdownSeconds = 3f;
        [SerializeField, Min(0f), Tooltip("How long the \"FIGHT!\" banner stays after the countdown.")]
        private float _fightBannerSeconds = 1f;
        [SerializeField] private BossData _boss;

        public float CountdownSeconds => _countdownSeconds;
        public float FightBannerSeconds => _fightBannerSeconds;
        public BossData Boss => _boss;
    }
}
