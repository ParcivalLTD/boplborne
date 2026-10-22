using GraceRun.Abilities;
using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Picks the player's clip on every PC and mirrors the rig for facing. Priority: ghost, hurt,
    /// dodge, attack, then locomotion from <see cref="PlayerAnimSync"/>. Everything it reads is
    /// already on every PC (HP, the roll's i-frame flag, replayed swings, the synced locomotion),
    /// so it sends nothing. Local cosmetic only.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int IdleId = Animator.StringToHash("Idle");
        private static readonly int RunId = Animator.StringToHash("Run");
        private static readonly int JumpId = Animator.StringToHash("Jump");
        private static readonly int FallId = Animator.StringToHash("Fall");
        private static readonly int DodgeId = Animator.StringToHash("Dodge");
        private static readonly int Attack1Id = Animator.StringToHash("Attack1");
        private static readonly int Attack2Id = Animator.StringToHash("Attack2");
        private static readonly int Attack3Id = Animator.StringToHash("Attack3");
        private static readonly int HurtId = Animator.StringToHash("Hurt");
        private static readonly int GhostId = Animator.StringToHash("Ghost");

        [SerializeField] private PlayerAnimData _data;
        [SerializeField] private PlayerStatsData _stats;
        [SerializeField] private Animator _animator;
        [SerializeField, Tooltip("The plain controller, used by a weapon without its own animations.")]
        private RuntimeAnimatorController _baseController;
        [SerializeField, Tooltip("Mirrored on X for facing left; holds the body and the hat.")]
        private Transform _rig;
        [SerializeField] private PlayerAnimSync _sync;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField, Tooltip("The body sprite the swing poses are drawn on.")] private SpriteRenderer _body;
        [SerializeField, Tooltip("The hat, moved with the head in each pose.")] private Transform _hat;
        [SerializeField] private Health _health;
        [SerializeField] private Ghost _ghost;
        [SerializeField, Tooltip("Raises the cast poses (every PC).")] private AbilityRunner _abilities;

        /// <summary>Not an Animator state: a weapon's held pose, set straight on the body sprite.</summary>
        private const int PosedState = -1;

        /// <summary>Not an Animator state: an ability cast pose, set straight on the body sprite.</summary>
        private const int CastState = -2;

        private WeaponBodyPoses.MovePoses _cast;
        private float _castStart = float.NegativeInfinity;
        private float _castWindup;
        private float _castRecovery;
        private float _castSide = 1f;

        private int _current;
        private int _attackState = Attack1Id;
        private float _attackSide = 1f;
        private bool _restartAttack;
        private float _hurtLeft;
        private float _dodgeLeft;

        private void OnEnable()
        {
            _attack.SwingStarted += OnSwingStarted;
            _dodge.RollStarted += OnRollStarted;
            _health.Changed += OnHealthChanged;
            _weapon.Changed += OnWeaponChanged;
            if (_abilities != null) _abilities.CastStarted += OnCastStarted;
            OnWeaponChanged(_weapon.Current);
        }

        private void OnDisable()
        {
            _attack.SwingStarted -= OnSwingStarted;
            _dodge.RollStarted -= OnRollStarted;
            _health.Changed -= OnHealthChanged;
            _weapon.Changed -= OnWeaponChanged;
            if (_abilities != null) _abilities.CastStarted -= OnCastStarted;
        }

        private void OnCastStarted(AbilityData ability, Vector2 aim)
        {
            if (ability.CastPose == null) return;
            _cast = ability.CastPose.Poses;
            _castWindup = ability.CastWindup;
            _castRecovery = ability.CastRecovery;
            _castSide = AbilityMath.SideOf(aim.x);
            _castStart = Time.time;
        }

        /// <summary>A weapon with its own idle, run, jump and fall holds it in hand; the controller swap restarts the clip.</summary>
        private void OnWeaponChanged(WeaponData weapon)
        {
            RuntimeAnimatorController controller = weapon.Animations != null ? weapon.Animations : _baseController;
            if (_animator.runtimeAnimatorController != controller) _animator.runtimeAnimatorController = controller;
            _current = 0;
        }

        private void Update()
        {
            _hurtLeft -= Time.deltaTime;
            _dodgeLeft -= Time.deltaTime;

            var swinging = _attack.IsSwinging;
            var state = PickState(swinging);
            var attackLike = state == _attackState || state == PosedState;
            var facing = state == CastState ? _castSide : swinging && attackLike ? _attackSide : _sync.Facing;
            _rig.localScale = new Vector3(facing, 1f, 1f);

            if (state == CastState)
            {
                ShowCastPose();
                return;
            }
            if (state == PosedState)
            {
                ShowPose();
                return;
            }
            if (!_animator.enabled)
            {
                _animator.enabled = true;
                _current = 0;
            }

            var restart = state == _attackState && _restartAttack;
            _restartAttack = false;
            if (state == _current && !restart) return;

            _current = state;
            _animator.Play(state, 0, 0f);
        }

        private int PickState(bool swinging)
        {
            if (_ghost.IsGhost) return GhostId;
            if (_hurtLeft > 0f) return HurtId;
            if (_dodgeLeft > 0f) return DodgeId;
            if (CastPoseMath.PhaseAt(Time.time - _castStart, _castWindup, _castRecovery) != CastPhase.None) return CastState;
            if (swinging) return _weapon.Current.BodyPoses != null ? PosedState : _attackState;

            return _sync.Locomotion switch
            {
                PlayerLocomotion.Run => RunId,
                PlayerLocomotion.Jump => JumpId,
                PlayerLocomotion.Fall => FallId,
                _ => IdleId,
            };
        }

        /// <summary>A weapon with its own body poses holds one pose per swing phase, so the art matches the move's timing.</summary>
        private void ShowPose()
        {
            _animator.enabled = false;
            var poses = _weapon.Current.BodyPoses.Get(_attack.CurrentBodyMove);
            var sprite = WeaponBodyPoses.SpriteFor(poses, _attack.Phase);
            if (sprite != null) _body.sprite = sprite;
            _hat.localPosition = WeaponBodyPoses.HatFor(poses, _attack.Phase);
        }

        /// <summary>The cast pose of the current phase (wind-up, release, recovery) drawn straight on the body sprite.</summary>
        private void ShowCastPose()
        {
            _animator.enabled = false;
            var phase = CastPoseMath.PhaseAt(Time.time - _castStart, _castWindup, _castRecovery);
            var swing = phase == CastPhase.Windup ? SwingPhase.Startup : phase == CastPhase.Release ? SwingPhase.Active : SwingPhase.Recovery;
            var sprite = WeaponBodyPoses.SpriteFor(_cast, swing);
            if (sprite != null) _body.sprite = sprite;
            _hat.localPosition = WeaponBodyPoses.HatFor(_cast, swing);
        }

        private void OnSwingStarted(int hitIndex, float side)
        {
            _attackState = _attack.IsFinisher(hitIndex) ? Attack3Id : hitIndex % 2 == 0 ? Attack1Id : Attack2Id;
            _attackSide = side < 0f ? -1f : 1f;
            _restartAttack = true;
            _dodgeLeft = 0f;
        }

        private void OnRollStarted() => _dodgeLeft = _stats.DodgeDuration;

        private void OnHealthChanged(int previous, int current)
        {
            if (current < previous && current > 0) _hurtLeft = _data.HurtTime;
        }
    }
}
