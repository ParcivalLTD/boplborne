using System;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The dodge roll: a burst of speed in the move-input direction, any way including up and
    /// diagonals (or the facing direction), invulnerable for the first part. Up, down and diagonal
    /// rolls switch gravity off; a flat roll keeps it. Runs on the owner; the invulnerable state is
    /// an owner-write NetworkVariable so every client shows the white flash and, later, so
    /// victim-side hit detection can skip hits during i-frames.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerDodge : NetworkBehaviour, IInvulnerability
    {
        [SerializeField] private PlayerStatsData _stats;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerStamina _stamina;
        [SerializeField] private SpriteFlash _flash;

        private readonly NetworkVariable<bool> _invulnerable = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private Rigidbody2D _body;
        private bool _dodging;
        private float _elapsed;
        private Vector2 _direction;

        /// <summary>Raised on the owner when a roll starts.</summary>
        public event Action Dodged;

        /// <summary>
        /// Raised on every PC when a roll starts, for cosmetics. Comes from the i-frame flag, so it
        /// needs DodgeInvulnerableTime above 0.
        /// </summary>
        public event Action RollStarted;

        public bool IsDodging => _dodging;

        /// <summary>True during the i-frames at the start of a roll, on every client.</summary>
        public bool IsInvulnerable => _invulnerable.Value;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        public override void OnNetworkSpawn()
        {
            _invulnerable.OnValueChanged += OnInvulnerableChanged;
            _flash.SetHeld(_invulnerable.Value);

            enabled = IsOwner;
            if (IsOwner) _input.DodgePressed += OnDodgePressed;
        }

        public override void OnNetworkDespawn()
        {
            _invulnerable.OnValueChanged -= OnInvulnerableChanged;
            if (_input != null) _input.DodgePressed -= OnDodgePressed;
            if (_flash != null) _flash.SetHeld(false);
        }

        private void OnDisable()
        {
            if (_dodging) EndDodge();
        }

        private void FixedUpdate()
        {
            if (!_dodging) return;

            _elapsed += Time.fixedDeltaTime;
            ApplyRollVelocity();

            if (_invulnerable.Value && _elapsed >= _stats.DodgeInvulnerableTime) _invulnerable.Value = false;
            if (_elapsed >= _stats.DodgeDuration) EndDodge();
        }

        private void OnDodgePressed()
        {
            if (!enabled || _dodging || !_stamina.TrySpend(_stats.DodgeCost)) return;

            _direction = ActionAim.DodgeDirection(_input.Move, _movement.Facing, _movement.IsGrounded);
            _elapsed = 0f;
            _dodging = true;
            _movement.ControlSuspended = true;
            _movement.GravitySuspended = !IsFlat;
            _invulnerable.Value = _stats.DodgeInvulnerableTime > 0f;
            ApplyRollVelocity();
            Dodged?.Invoke();
        }

        private void EndDodge()
        {
            _dodging = false;
            if (!IsFlat && _body.linearVelocityY > 0f) _body.linearVelocityY *= _stats.DodgeExitLift;
            _movement.ControlSuspended = false;
            _movement.GravitySuspended = false;
            if (_invulnerable.Value) _invulnerable.Value = false;
        }

        // A flat roll only owns the horizontal speed, as before: gravity, ledges and jumping out still work.
        private bool IsFlat => Mathf.Approximately(_direction.y, 0f);

        private void ApplyRollVelocity()
        {
            if (IsFlat) _body.linearVelocityX = _direction.x * _stats.DodgeSpeed;
            else _body.linearVelocity = _direction * _stats.DodgeSpeed;
        }

        private void OnInvulnerableChanged(bool previous, bool current)
        {
            _flash.SetHeld(current);
            if (current && !previous) RollStarted?.Invoke();
        }
    }
}
