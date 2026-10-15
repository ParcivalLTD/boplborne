using System;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The dodge roll: a burst of horizontal speed in the input direction (or the facing
    /// direction), invulnerable for the first part. Runs on the owner; the invulnerable state is
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
        private float _direction;

        /// <summary>Raised on the owner when a roll starts.</summary>
        public event Action Dodged;

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

        private void FixedUpdate()
        {
            if (!_dodging) return;

            _elapsed += Time.fixedDeltaTime;
            _body.linearVelocityX = _direction * _stats.DodgeSpeed;

            if (_invulnerable.Value && _elapsed >= _stats.DodgeInvulnerableTime) _invulnerable.Value = false;
            if (_elapsed >= _stats.DodgeDuration) EndDodge();
        }

        private void OnDodgePressed()
        {
            if (_dodging || !_stamina.TrySpend(_stats.DodgeCost)) return;

            var inputX = _input.Move.x;
            _direction = Mathf.Abs(inputX) > 0.01f ? Mathf.Sign(inputX) : _movement.Facing;
            _elapsed = 0f;
            _dodging = true;
            _movement.ControlSuspended = true;
            _invulnerable.Value = _stats.DodgeInvulnerableTime > 0f;
            _body.linearVelocityX = _direction * _stats.DodgeSpeed;
            Dodged?.Invoke();
        }

        private void EndDodge()
        {
            _dodging = false;
            _movement.ControlSuspended = false;
            if (_invulnerable.Value) _invulnerable.Value = false;
        }

        private void OnInvulnerableChanged(bool previous, bool current) => _flash.SetHeld(current);
    }
}
