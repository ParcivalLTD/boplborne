using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Shows the boss pose that matches what it is doing, on every PC: the attack's wind-up /
    /// strike / recovery pose from the shared attack timeline, an idle loop, a stagger on a phase
    /// change and the death collapse. The red ground zone, the sound and the gold weak point
    /// (Telegraph) come on top; there is no red glow on the boss art. Local cosmetic only.
    /// </summary>
    public class BossPoseView : MonoBehaviour
    {
        [SerializeField] private BossPoseSet _poses;
        [SerializeField] private BossAttackRunner _runner;
        [SerializeField] private BossBrain _brain;
        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer _body;

        private float _staggerUntil;
        private float _deadSince = -1f;

        private void OnEnable() => _brain.PhaseChanged += OnPhaseChanged;

        private void OnDisable() => _brain.PhaseChanged -= OnPhaseChanged;

        private void OnPhaseChanged(int phase) => _staggerUntil = Time.time + _poses.StaggerTime;

        /// <summary>Swaps the pose set at runtime (a boss prefab shared by two bodies, e.g. the Bell Twins).</summary>
        public void SetPoses(BossPoseSet poses)
        {
            if (poses != null) _poses = poses;
        }

        private void LateUpdate()
        {
            if (_health.IsDead)
            {
                if (_deadSince < 0f) _deadSince = Time.time;
                Show(BossPoseSet.Frame(_poses.Death, _poses.DeathFps, Time.time - _deadSince, false));
                return;
            }
            _deadSince = -1f;

            var attack = _runner.Attack;
            var pose = attack != null ? _poses.PoseFor(attack.Id) : null;
            if (pose != null)
            {
                switch (_runner.Phase)
                {
                    case AttackPhase.Windup:
                        Show(pose.Windup);
                        return;
                    case AttackPhase.Active:
                        Show(pose.Strike);
                        return;
                    case AttackPhase.Recovery:
                        Show(pose.Recovery);
                        return;
                }
            }

            var staggered = Time.time < _staggerUntil || _brain.IsStaggered || _brain.IsTransitioning;
            if (staggered && _poses.Stagger != null) Show(_poses.Stagger);
            else Show(BossPoseSet.Frame(_poses.Idle, _poses.IdleFps, Time.time, true));
        }

        private void Show(Sprite sprite)
        {
            if (sprite != null) _body.sprite = sprite;
        }
    }
}
