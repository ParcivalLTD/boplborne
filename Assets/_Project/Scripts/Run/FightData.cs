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
        [SerializeField, Min(1f), Tooltip("Longest the full-screen story may hold the party, so one stuck PC cannot freeze the run.")]
        private float _storyTimeoutSeconds = 90f;
        [SerializeField, Min(0f), Tooltip("Hub: seconds between the last player readying up and the run starting. Anyone un-readying cancels it.")]
        private float _readyStartDelaySeconds = 2.5f;
        [SerializeField] private BossData _boss;

        public float ReadyStartDelaySeconds => _readyStartDelaySeconds;
        public float CountdownSeconds => _countdownSeconds;
        public float FightBannerSeconds => _fightBannerSeconds;
        public float StoryTimeoutSeconds => _storyTimeoutSeconds;
        public BossData Boss => _boss;
    }
}
