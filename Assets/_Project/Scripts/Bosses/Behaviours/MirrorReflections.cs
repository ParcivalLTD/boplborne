using GraceRun.Combat;
using GraceRun.Core.Framing;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// The Mirror Duelist's phase 2 twist: two pale reflections copy each of its melee attacks a moment later,
    /// one standing across the target from the Duelist and one across the arena. They cannot be hurt; they
    /// only strike. Built locally on every PC from a template (not networked); like every boss hit, a
    /// reflection's strike is detected on the victim's PC.
    /// </summary>
    [RequireComponent(typeof(BossAttackRunner), typeof(BossBrain))]
    public class MirrorReflections : MonoBehaviour
    {
        [SerializeField] private MirrorDuelistData _data;
        [SerializeField, Tooltip("Template: SpriteRenderer, plus a child with BoxCollider2D + Hitbox on the BossAttack layer.")]
        private GameObject _template;
        [SerializeField, Tooltip("Optional: the Duelist's poses; a reflection shows the wind-up, strike and recovery pose of the copied attack.")]
        private BossPoseSet _poses;
        [SerializeField, Min(1f), Tooltip("Half width authored for the 32-wide arena; widened with the real arena.")]
        private float _arenaHalfWidth = 14.5f;

        private const int Count = 2;

        private readonly GameObject[] _roots = new GameObject[Count];
        private readonly SpriteRenderer[] _art = new SpriteRenderer[Count];
        private readonly BoxCollider2D[] _shapes = new BoxCollider2D[Count];
        private readonly Hitbox[] _hitboxes = new Hitbox[Count];
        private BossAttackRunner _runner;
        private BossBrain _brain;
        private AttackData _planned;
        private float _plannedAt;
        private readonly Vector3[] _baseScale = new Vector3[Count];
        private readonly float[] _armedAt = new float[Count];
        private BossPoseSet.AttackPose _plannedPose;
        private sbyte[] _faces = new sbyte[Count];

        private void Awake()
        {
            _runner = GetComponent<BossAttackRunner>();
            _brain = GetComponent<BossBrain>();
        }

        private void OnEnable() => _runner.PhaseChanged += OnPhase;

        private void OnDisable()
        {
            _runner.PhaseChanged -= OnPhase;
            for (var i = 0; i < Count; i++) if (_roots[i] != null) Destroy(_roots[i]);
        }

        private void Update()
        {
            if (_planned == null) return;
            var t = Time.time - _plannedAt;
            var finish = _planned.Windup + Mathf.Max(_data.ReflectionDelays.x, _data.ReflectionDelays.y) + _planned.Active + _data.ReflectionFade;
            for (var i = 0; i < Count; i++)
            {
                var strike = _planned.Windup + _data.ReflectionDelays[i];
                SetArmed(i, t >= strike && t < strike + _planned.Active);
                ShowPose(i, t, strike);
                _art[i].color = StrikeFlash(i, ReflectionColour(t, finish));
                _art[i].transform.localScale = Squash(i);
            }
            if (t < finish) return;
            for (var i = 0; i < Count; i++) SetArmed(i, false);
            Hide();
            _planned = null;
        }

        private void OnPhase(AttackPhase phase)
        {
            if (phase != AttackPhase.Windup || _brain.Phase < 2) return;
            var attack = _runner.Attack;
            if (attack == null || attack.Damage <= 0 || _runner.IsFeint) return;

            _planned = attack;
            _plannedAt = Time.time;
            _plannedPose = _poses != null ? _poses.PoseFor(attack.Id) : null;
            var targetX = TargetX();
            var half = ArenaBounds.WidenedHalfWidth(_arenaHalfWidth);
            for (var i = 0; i < Count; i++)
            {
                Ensure(i);
                var x = MirrorMath.ReflectionX(i, transform.position.x, targetX, half);
                if (i == 1) x = MirrorMath.SeparatedX(_roots[0].transform.position.x, x, _data.ReflectionMinGap, half);
                _faces[i] = (sbyte)(targetX >= x ? 1 : -1);
                _roots[i].transform.position = new Vector3(x, transform.position.y, 0f);
                _art[i].flipX = _faces[i] > 0; // the drawn poses face left
                _armedAt[i] = -10f;
                _art[i].transform.localScale = _baseScale[i];
                if (_poses != null && _poses.Idle.Length > 0) _art[i].sprite = _poses.Idle[0];
                _shapes[i].size = attack.HitboxSize;
                _shapes[i].offset = new Vector2(attack.HitboxOffset.x * _faces[i], attack.HitboxOffset.y);
                _roots[i].SetActive(true);
            }
            if (_data.ReflectionAppearCue != null) _data.ReflectionAppearCue.Play();
        }

        private Color ReflectionColour(float t, float end)
        {
            var fadeIn = Mathf.Clamp01(t / 0.25f);
            var fadeOut = Mathf.Clamp01((end - t) / Mathf.Max(0.01f, _data.ReflectionFade));
            return new Color(0.75f, 0.7f, 1f, _data.ReflectionAlpha * fadeIn * fadeOut);
        }

        private void ShowPose(int i, float t, float strike)
        {
            if (_plannedPose == null) return;
            var pose = t < strike ? _plannedPose.Windup : t < strike + _planned.Active ? _plannedPose.Strike : _plannedPose.Recovery;
            if (pose != null) _art[i].sprite = pose;
        }

        /// <summary>A short white-hot flash on the frame a reflection arms its strike.</summary>
        private Color StrikeFlash(int i, Color colour)
        {
            var k = FlashAmount(i);
            if (k <= 0f) return colour;
            return new Color(Mathf.Lerp(colour.r, 1f, k), Mathf.Lerp(colour.g, 1f, k), Mathf.Lerp(colour.b, 1f, k),
                Mathf.Min(1f, colour.a + k * 0.5f));
        }

        private Vector3 Squash(int i)
        {
            var k = FlashAmount(i) * _data.ReflectionSquash;
            return new Vector3(_baseScale[i].x * (1f + k), _baseScale[i].y * (1f - k), _baseScale[i].z);
        }

        private float FlashAmount(int i) =>
            _data.ReflectionFlashTime <= 0f ? 0f : Mathf.Clamp01(1f - (Time.time - _armedAt[i]) / _data.ReflectionFlashTime);

        private void SetArmed(int i, bool armed)
        {
            if (armed == _hitboxes[i].IsArmed) return;
            if (!armed)
            {
                _hitboxes[i].Disarm();
                return;
            }
            if (_data.ReflectionStrikeCue != null) _data.ReflectionStrikeCue.Play();
            var damage = Mathf.RoundToInt(_planned.Damage * _data.ReflectionDamageScale * _brain.DamageScale);
            _armedAt[i] = Time.time;
            _hitboxes[i].Arm(damage, new Vector2(_planned.Knockback.x * _faces[i], _planned.Knockback.y));
        }

        private void Ensure(int i)
        {
            if (_roots[i] != null) return;
            _roots[i] = Instantiate(_template);
            _roots[i].name = "MirrorReflection" + i;
            _art[i] = _roots[i].GetComponentInChildren<SpriteRenderer>(true);
            _baseScale[i] = _art[i].transform.localScale;
            _hitboxes[i] = _roots[i].GetComponentInChildren<Hitbox>(true);
            _shapes[i] = _hitboxes[i].GetComponent<BoxCollider2D>();
            _hitboxes[i].SetAttacker(GetComponent<NetworkObject>());
        }

        private void Hide()
        {
            for (var i = 0; i < Count; i++) if (_roots[i] != null) _roots[i].SetActive(false);
        }

        private float TargetX()
        {
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p != null && p.IsSpawned && p.NetworkObjectId == _runner.TargetId) return p.transform.position.x;
            }
            return 0f;
        }
    }
}
