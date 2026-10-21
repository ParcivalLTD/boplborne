using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Plays each attack's telegraph sound (<see cref="AttackData.WindupCue"/>) when its wind-up starts and its
    /// impact sound (<see cref="AttackData.StrikeCue"/>) when it goes live. Every PC, local only.
    /// </summary>
    public class BossAttackAudio : MonoBehaviour
    {
        [SerializeField] private BossAttackRunner _runner;

        private void OnEnable() => _runner.PhaseChanged += OnAttackPhaseChanged;

        private void OnDisable() => _runner.PhaseChanged -= OnAttackPhaseChanged;

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            if (_runner.Attack == null) return;
            var cue = phase == AttackPhase.Windup ? _runner.Attack.WindupCue
                : phase == AttackPhase.Active ? _runner.Attack.StrikeCue : null;
            if (cue != null) cue.Play();
        }
    }
}
