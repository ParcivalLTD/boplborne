using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Shows the gold weak point during the punish window at the start of an attack's recovery,
    /// on every PC (from <see cref="BossAttackRunner"/>). There is no red glow or red zone: the
    /// wind-up is read from the boss pose and the telegraph sound (decision 26). Local cosmetic only.
    /// </summary>
    public class Telegraph : MonoBehaviour
    {
        [SerializeField] private TelegraphData _look;
        [SerializeField] private BossAttackRunner _runner;
        [SerializeField] private SpriteRenderer _weakPoint;

        private Vector3 _weakPointScale;

        private void Awake()
        {
            _weakPointScale = _weakPoint.transform.localScale;
            _weakPoint.enabled = false;
        }

        private void LateUpdate()
        {
            var attack = _runner.Attack;
            if (attack == null || _runner.Phase != AttackPhase.Recovery)
            {
                _weakPoint.enabled = false;
                return;
            }

            var sinceRecovery = (float)_runner.Elapsed - attack.Windup - attack.Active;
            if (sinceRecovery < attack.PunishWindow) ShowWeakPoint(attack, _runner.Facing, sinceRecovery);
            else _weakPoint.enabled = false;
        }

        private void ShowWeakPoint(AttackData attack, sbyte facing, float time)
        {
            var point = _weakPoint.transform;
            point.localPosition = new Vector3(attack.WeakPointOffset.x * facing, attack.WeakPointOffset.y, 0f);
            var pulse = 1f + _look.WeakPointPulseScale * Mathf.Sin(time * _look.WeakPointPulseRate * 2f * Mathf.PI);
            point.localScale = _weakPointScale * pulse;
            _weakPoint.enabled = true;
        }
    }
}
