using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Shows the current attack's warnings on every PC, from <see cref="BossAttackRunner"/>:
    /// a red glow pulsing faster and faster on the attacking boss part during the wind-up, an
    /// optional red zone over the area the hitbox will cover, and a gold weak point during the
    /// punish window. Local cosmetic only.
    /// </summary>
    public class Telegraph : MonoBehaviour
    {
        [SerializeField] private TelegraphData _look;
        [SerializeField] private BossAttackRunner _runner;
        [SerializeField, Tooltip("Red overlay sprites, one per boss part. AttackData.GlowPart indexes this list.")]
        private SpriteRenderer[] _glowParts;
        [SerializeField, Tooltip("Not mirrored by the facing; positioned from the attack's hitbox.")]
        private SpriteRenderer _zone;
        [SerializeField] private SpriteRenderer _weakPoint;

        private Vector3 _weakPointScale;

        private void Awake()
        {
            _weakPointScale = _weakPoint.transform.localScale;
            HideAll();
        }

        private void LateUpdate()
        {
            var attack = _runner.Attack;
            if (attack == null)
            {
                HideAll();
                return;
            }

            var elapsed = (float)_runner.Elapsed;
            var facing = _runner.Facing;
            switch (_runner.Phase)
            {
                case AttackPhase.Windup:
                    ShowGlow(attack.GlowPart, PulseAlpha(attack, elapsed));
                    ShowZone(attack, facing, _look.ZoneWindupAlpha);
                    HideWeakPoint();
                    break;
                case AttackPhase.Active:
                    ShowGlow(attack.GlowPart, _look.GlowMaxAlpha);
                    ShowZone(attack, facing, _look.ZoneActiveAlpha);
                    HideWeakPoint();
                    break;
                case AttackPhase.Recovery:
                    ShowGlow(-1, 0f);
                    _zone.enabled = false;
                    var sinceRecovery = elapsed - attack.Windup - attack.Active;
                    if (sinceRecovery < attack.PunishWindow) ShowWeakPoint(attack, facing, sinceRecovery);
                    else HideWeakPoint();
                    break;
                default:
                    HideAll();
                    break;
            }
        }

        private float PulseAlpha(AttackData attack, float elapsed)
        {
            // The pulse rate ramps linearly from start to end over the wind-up; integrate it for the wave.
            var windup = Mathf.Max(attack.Windup, 0.0001f);
            var t = Mathf.Clamp01(elapsed / windup);
            var cycles = windup * (_look.PulseRateStart * t + (_look.PulseRateEnd - _look.PulseRateStart) * t * t * 0.5f);
            var wave = 0.5f - 0.5f * Mathf.Cos(cycles * 2f * Mathf.PI);
            return Mathf.Lerp(_look.GlowMinAlpha, _look.GlowMaxAlpha, wave);
        }

        private void ShowGlow(int part, float alpha)
        {
            for (var i = 0; i < _glowParts.Length; i++)
            {
                var on = i == part;
                _glowParts[i].enabled = on;
                if (on) _glowParts[i].color = WithAlpha(_look.TelegraphRed, alpha);
            }
        }

        private void ShowZone(AttackData attack, sbyte facing, float alpha)
        {
            if (attack.TelegraphType != TelegraphType.GroundZone)
            {
                _zone.enabled = false;
                return;
            }

            var zone = _zone.transform;
            zone.localPosition = new Vector3(attack.HitboxOffset.x * facing, attack.HitboxOffset.y, 0f);
            zone.localScale = new Vector3(attack.HitboxSize.x, attack.HitboxSize.y, 1f);
            _zone.color = WithAlpha(_look.TelegraphRed, alpha);
            _zone.enabled = true;
        }

        private void ShowWeakPoint(AttackData attack, sbyte facing, float time)
        {
            var point = _weakPoint.transform;
            point.localPosition = new Vector3(attack.WeakPointOffset.x * facing, attack.WeakPointOffset.y, 0f);
            var pulse = 1f + _look.WeakPointPulseScale * Mathf.Sin(time * _look.WeakPointPulseRate * 2f * Mathf.PI);
            point.localScale = _weakPointScale * pulse;
            _weakPoint.color = _look.WeakPointGold;
            _weakPoint.enabled = true;
        }

        private void HideWeakPoint() => _weakPoint.enabled = false;

        private void HideAll()
        {
            ShowGlow(-1, 0f);
            _zone.enabled = false;
            HideWeakPoint();
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
