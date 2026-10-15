using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>Plays each attack's telegraph sound (<see cref="AttackData.WindupCue"/>) when its wind-up starts. Every PC, local only.</summary>
    public class BossAttackAudio : MonoBehaviour
    {
        [SerializeField] private BossAttackRunner _runner;

        private void OnEnable() => _runner.PhaseChanged += OnAttackPhaseChanged;

        private void OnDisable() => _runner.PhaseChanged -= OnAttackPhaseChanged;

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            if (phase != AttackPhase.Windup || _runner.Attack == null) return;
            var cue = _runner.Attack.WindupCue;
            if (cue != null) cue.Play();
        }
    }
}
