using GraceRun.Combat;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Drives the player's <see cref="Animator"/> and horizontal sprite flip on every PC from
    /// movement and combat state (idle, run, jump, fall, dodge, attack1–3, hurt, ghost).
    /// Local cosmetic only; safe on Host, Client and Server-only builds.
    /// </summary>
    public class PlayerAnimator : NetworkBehaviour
    {
        [SerializeField] private PlayerAnimationData _data;
        [SerializeField] private PlayerMovementData _movementData;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private SpriteRenderer _hatRenderer;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private Health _health;
        [SerializeField] private Ghost _ghost;
        [SerializeField] private Transform _groundCheck;

        private Rigidbody2D _body;
        private Vector3 _lastPosition;
        private Vector2 _remoteVelocity;
        private float _facing = 1f;
        private float _hurtTimer;
        private float _dodgeTimer;
        private float _attackTimer;
        private int _attackIndex;
        private string _currentState;

        public string CurrentState => _currentState;
        public float Facing => _facing;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            _lastPosition = transform.position;
            if (_attack != null) _attack.SwingStarted += OnSwingStarted;
            if (_dodge != null) _dodge.RollStarted += OnRollStarted;
            if (_health != null) _health.HitTaken += OnHitTaken;
            if (_ghost != null) _ghost.Changed += OnGhostChanged;
        }

        private void OnDisable()
        {
            if (_attack != null) _attack.SwingStarted -= OnSwingStarted;
            if (_dodge != null) _dodge.RollStarted -= OnRollStarted;
            if (_health != null) _health.HitTaken -= OnHitTaken;
            if (_ghost != null) _ghost.Changed -= OnGhostChanged;
        }

        private void LateUpdate()
        {
            if (_data == null || _animator == null) return;

            var dt = Time.deltaTime;
            if (_hurtTimer > 0f) _hurtTimer -= dt;
            if (_dodgeTimer > 0f) _dodgeTimer -= dt;
            if (_attackTimer > 0f) _attackTimer -= dt;

            SampleMotion(dt, out var isGrounded, out var vx, out var vy);

            var isGhost = _ghost != null && _ghost.IsGhost;
            var isDodging = (_dodge != null && (_dodge.IsDodging || _dodge.IsInvulnerable)) || _dodgeTimer > 0f;
            var isSwinging = (_attack != null && _attack.IsSwinging) || _attackTimer > 0f;
            var isHurt = _hurtTimer > 0f && !isDodging;

            var state = PlayerAnimationRules.ResolveState(
                isGhost,
                isHurt,
                isDodging,
                isSwinging,
                _attackIndex,
                isGrounded,
                Mathf.Abs(vx),
                vy,
                _data.RunSpeedThreshold,
                _data.AirSpeedThreshold);

            ApplyState(state);
            ApplyFacing();
        }

        private void SampleMotion(float dt, out bool isGrounded, out float vx, out float vy)
        {
            var pos = transform.position;
            if (dt > 0.0001f)
            {
                var delta = (Vector2)(pos - _lastPosition) / dt;
                _remoteVelocity = Vector2.Lerp(_remoteVelocity, delta, 0.5f);
            }
            _lastPosition = pos;

            var ownerMoving = _movement != null && _movement.enabled;
            if (ownerMoving && _body != null)
            {
                vx = _body.linearVelocity.x;
                vy = _body.linearVelocity.y;
                isGrounded = _movement.IsGrounded;
                if (_attackTimer <= 0f) _facing = _movement.Facing;
                return;
            }

            vx = _remoteVelocity.x;
            vy = _remoteVelocity.y;
            isGrounded = _groundCheck != null && _movementData != null &&
                Physics2D.OverlapBox(_groundCheck.position, _movementData.GroundCheckSize, 0f, Layers.StandableMask) != null;
            if (_attackTimer <= 0f)
                _facing = PlayerAnimationRules.ResolveFacing(_facing, vx, _data.FacingFlipThreshold);
        }

        private void ApplyState(string state)
        {
            if (_currentState == state) return;
            _currentState = state;
            if (_animator != null && _animator.runtimeAnimatorController != null)
                _animator.Play(state, 0, 0f);
        }

        private void ApplyFacing()
        {
            var flip = _facing < 0f;
            if (_bodyRenderer != null) _bodyRenderer.flipX = flip;
            if (_hatRenderer != null) _hatRenderer.flipX = flip;
        }

        private void OnSwingStarted(int hitIndex, float side)
        {
            _attackIndex = hitIndex;
            _attackTimer = _data != null ? _data.AttackClipDuration : 0.28f;
            if (Mathf.Abs(side) > 0.01f) _facing = side < 0f ? -1f : 1f;
            _currentState = null;
        }

        private void OnRollStarted()
        {
            _dodgeTimer = _data != null ? _data.DodgeClipDuration : 0.35f;
            _attackTimer = 0f;
            _currentState = null;
        }

        private void OnHitTaken(DamageInfo info)
        {
            if (info.Amount <= 0) return;
            _hurtTimer = _data != null ? _data.HurtDuration : 0.22f;
            _currentState = null;
        }

        private void OnGhostChanged(bool ghost)
        {
            if (ghost)
            {
                _hurtTimer = 0f;
                _dodgeTimer = 0f;
                _attackTimer = 0f;
            }
            _currentState = null;
        }
    }
}
