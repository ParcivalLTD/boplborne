using GraceRun.Core.Audio;
using GraceRun.Core.Juice;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Sounds and particles for a player's own actions, on every PC: a whoosh per swing (a heavier
    /// one on the last hit of the combo) and a sound plus a dust puff per dodge. Local cosmetic only.
    /// </summary>
    public class PlayerActionFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerFeedbackData _data;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private LightAttackData _attackData;
        [SerializeField] private Transform _feet;

        private void OnEnable()
        {
            _attack.SwingStarted += OnSwingStarted;
            _dodge.RollStarted += OnRollStarted;
        }

        private void OnDisable()
        {
            _attack.SwingStarted -= OnSwingStarted;
            _dodge.RollStarted -= OnRollStarted;
        }

        private void OnSwingStarted(int hitIndex, float facing)
        {
            var last = hitIndex == _attackData.Hits.Count - 1;
            Play(last && _data.FinisherCue != null ? _data.FinisherCue : _data.SwingCue);
        }

        private void OnRollStarted()
        {
            Play(_data.DodgeCue);
            Particles.Play(_data.DodgeDust, _feet.position);
        }

        private static void Play(AudioCue cue)
        {
            if (cue != null) cue.Play();
        }
    }
}
