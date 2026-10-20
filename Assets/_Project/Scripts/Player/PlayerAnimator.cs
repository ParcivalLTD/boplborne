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
        [SerializeField, Tooltip("Mirrored on X for facing left; holds the body and the hat.")]
        private Transform _rig;
        [SerializeField] private PlayerAnimSync _sync;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private Health _health;
        [SerializeField] private Ghost _ghost;

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
            _current = 0;
        }

        private void OnDisable()
        {
            _attack.SwingStarted -= OnSwingStarted;
            _dodge.RollStarted -= OnRollStarted;
            _health.Changed -= OnHealthChanged;
        }

        private void Update()
        {
            _hurtLeft -= Time.deltaTime;
            _dodgeLeft -= Time.deltaTime;

            var swinging = _attack.IsSwinging;
            var state = PickState(swinging);
            var facing = swinging && state == _attackState ? _attackSide : _sync.Facing;
            _rig.localScale = new Vector3(facing, 1f, 1f);

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
            if (swinging) return _attackState;

            return _sync.Locomotion switch
            {
                PlayerLocomotion.Run => RunId,
                PlayerLocomotion.Jump => JumpId,
                PlayerLocomotion.Fall => FallId,
                _ => IdleId,
            };
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
