using System;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Running and jumping for the player you own: acceleration with air control, a variable
    /// jump height, faster falling, coyote time and a jump buffer. Runs on the owner only;
    /// everyone else sees the result through the owner-authoritative NetworkTransform.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private PlayerMovementData _data;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Transform _groundCheck;

        private readonly JumpGate _jumpGate = new();
        private readonly Collider2D[] _groundHits = new Collider2D[4];

        private static readonly System.Collections.Generic.List<PlayerMovement> Owned = new();

        private Rigidbody2D _body;
        private ContactFilter2D _groundFilter;
        private bool _grounded;
        private bool _jumpHeld;
        private float _facing = 1f;
        private float _lockTimer;
        private float _slowTimer;
        private float _slowMultiplier = 1f;

        /// <summary>Every player movement component simulated on this PC (owner).</summary>
        public static System.Collections.Generic.IReadOnlyList<PlayerMovement> AllOwned => Owned;

        /// <summary>Raised on the owner the moment a jump leaves the ground.</summary>
        public event Action Jumped;

        /// <summary>Raised on the owner the moment the feet touch the ground again.</summary>
        public event Action Landed;

        /// <summary>True while the feet are on something solid.</summary>
        public bool IsGrounded => _grounded;

        /// <summary>+1 or -1: the last horizontal direction the player pushed.</summary>
        public float Facing => _facing;

        /// <summary>While true another component (the dodge) owns horizontal velocity.</summary>
        public bool ControlSuspended { get; set; }

        /// <summary>True while a knockback lock keeps input from steering (see <see cref="LockControl"/>).</summary>
        public bool ControlLocked => _lockTimer > 0f;

        /// <summary>True while a hazard (such as a sticky web zone) is slowing horizontal movement.</summary>
        public bool IsSlowed => _slowTimer > 0f;

        /// <summary>Scales the run speed, e.g. slowed while attacking. 1 is normal.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _groundFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.GroundIndex),
                useTriggers = false,
            };
        }

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (!IsOwner) return;

            if (!Owned.Contains(this)) Owned.Add(this);
            _input.JumpPressed += OnJumpPressed;
            _input.JumpReleased += OnJumpReleased;
            _body.gravityScale = _data.GravityScale;
        }

        public override void OnNetworkDespawn()
        {
            Owned.Remove(this);
            if (_input == null) return;
            _input.JumpPressed -= OnJumpPressed;
            _input.JumpReleased -= OnJumpReleased;
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;
            UpdateGrounded();
            _jumpGate.SetGrounded(_grounded, _data.CoyoteTime);

            ApplyHorizontal(deltaTime);
            if (_jumpGate.TryConsume()) Jump();
            ApplyGravity();

            _jumpGate.Tick(deltaTime);
            if (_lockTimer > 0f) _lockTimer -= deltaTime;
            if (_slowTimer > 0f)
            {
                _slowTimer -= deltaTime;
                if (_slowTimer <= 0f) _slowMultiplier = 1f;
            }
        }

        /// <summary>Owner only. Input stops steering for <paramref name="seconds"/>, so a push carries the body.</summary>
        public void LockControl(float seconds)
        {
            _lockTimer = Mathf.Max(_lockTimer, seconds);
        }

        /// <summary>Owner only. Scales run speed by <paramref name="multiplier"/> for <paramref name="seconds"/>.</summary>
        public void ApplySlow(float multiplier, float seconds)
        {
            if (seconds <= 0f) return;
            _slowMultiplier = _slowTimer > 0f ? Mathf.Min(_slowMultiplier, multiplier) : multiplier;
            _slowTimer = Mathf.Max(_slowTimer, seconds);
        }

        private void UpdateGrounded()
        {
            var hits = Physics2D.OverlapBox(
                _groundCheck.position, _data.GroundCheckSize, 0f, _groundFilter, _groundHits);
            var grounded = hits > 0 && _body.linearVelocityY <= 0.01f;

            if (grounded && !_grounded) Landed?.Invoke();
            _grounded = grounded;
        }

        private void ApplyHorizontal(float deltaTime)
        {
            var inputX = Mathf.Clamp(_input.Move.x, -1f, 1f);
            if (Mathf.Abs(inputX) > 0.01f) _facing = Mathf.Sign(inputX);
            if (ControlSuspended || ControlLocked) return;

            var slow = _slowTimer > 0f ? _slowMultiplier : 1f;
            var wanted = inputX * _data.RunSpeed * SpeedMultiplier * slow;
            var changing = Mathf.Abs(wanted) > 0.01f;
            var rate = changing ? _data.Acceleration : _data.Deceleration;
            if (!_grounded) rate *= _data.AirControl;

            var speed = Mathf.MoveTowards(_body.linearVelocityX, wanted, rate * deltaTime);
            _body.linearVelocityX = speed;
        }

        private void Jump()
        {
            _body.linearVelocityY = _data.JumpVelocity;
            _grounded = false;
            Jumped?.Invoke();
        }

        private void ApplyGravity()
        {
            var rising = _body.linearVelocityY > 0.01f;
            _body.gravityScale = rising && _jumpHeld
                ? _data.GravityScale
                : _data.GravityScale * _data.FallGravityMultiplier;

            if (_body.linearVelocityY < -_data.MaxFallSpeed) _body.linearVelocityY = -_data.MaxFallSpeed;
        }

        private void OnJumpPressed()
        {
            _jumpHeld = true;
            _jumpGate.Press(_data.JumpBuffer);
        }

        private void OnJumpReleased()
        {
            _jumpHeld = false;
            if (_body.linearVelocityY > 0f) _body.linearVelocityY *= _data.JumpCutMultiplier;
        }
    }
}
