using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>Sounds and particles for a player's own actions (swing, dodge). Hit juice lives in HitFeedbackData.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Player Feedback Data", fileName = "PlayerFeedbackData")]
    public class PlayerFeedbackData : ScriptableObject
    {
        [SerializeField] private AudioCue _swingCue;
        [SerializeField, Tooltip("Played on the third hit of the combo instead of the swing cue.")]
        private AudioCue _finisherCue;
        [SerializeField] private AudioCue _dodgeCue;
        [SerializeField] private ParticleSystem _dodgeDust;

        public AudioCue SwingCue => _swingCue;
        public AudioCue FinisherCue => _finisherCue;
        public AudioCue DodgeCue => _dodgeCue;
        public ParticleSystem DodgeDust => _dodgeDust;
    }
}
