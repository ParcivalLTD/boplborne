using System;
using System.Collections.Generic;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Running and jumping for the player you own: acceleration with air control, a variable
    /// jump height with a short hang at the top, air jumps, wall slides and wall jumps, fast falls
    /// (hold down), dropping through one-way ledges (down + jump), coyote time and a jump buffer.
    /// Runs on the owner only; everyone else sees the result through the owner-authoritative
    /// NetworkTransform.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private PlayerMovementData _data;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Transform _groundCheck;
        [SerializeField, Tooltip("Optional. Lets down + jump fall through one-way ledges.")]
        private PlayerPlatformDrop _platformDrop;

        private static readonly List<PlayerMovement> Owned = new();

        private readonly JumpGate _jumpGate = new();
        private readonly WallProbe _wallProbe = new();
        private readonly Collider2D[] _groundHits = new Collider2D[4];

        private Rigidbody2D _body;
        private Collider2D _collider;
        private ContactFilter2D _groundFilter;
        private ContactFilter2D _solidGroundFilter;
        private bool _grounded;
        private bool _jumpHeld;
        private float _facing = 1f;
        private float _lockTimer;
        private float _slowTimer;
        private float _slowMultiplier = 1f;
        private float _wallSide;
        private float _lastWallSide;
        private float _wallCoyote;

        /// <summary>Every player movement component simulated on this PC (owner).</summary>
        public static IReadOnlyList<PlayerMovement> AllOwned => Owned;

        /// <summary>Raised on the owner the moment a jump leaves the ground (or a wall).</summary>
        public event Action Jumped;

        /// <summary>Raised on the owner the moment the feet touch the ground again.</summary>
        public event Action Landed;

        /// <summary>True while the feet are on something solid.</summary>
        public bool IsGrounded => _grounded;

        /// <summary>True while sliding down a wall (in the air, pushing into it, falling).</summary>
        public bool IsWallSliding { get; private set; }

        /// <summary>+1 or -1: the last horizontal direction the player pushed.</summary>
        public float Facing => _facing;

        /// <summary>While true another component (the dodge) owns horizontal velocity.</summary>
        public bool ControlSuspended { get; set; }

        /// <summary>While true gravity is off (an up, down or diagonal dodge roll owns the vertical speed too).</summary>
        public bool GravitySuspended { get; set; }

        /// <summary>True while a knockback lock keeps input from steering (see <see cref="LockControl"/>).</summary>
        public bool ControlLocked => _lockTimer > 0f;

        /// <summary>True while a hazard (such as a sticky web zone) is slowing horizontal movement.</summary>
        public bool IsSlowed => _slowTimer > 0f;

        /// <summary>Scales the run speed, e.g. slowed while attacking. 1 is normal.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        private bool DownHeld => _input.Move.y < -_data.DownInputThreshold;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _groundFilter = new ContactFilter2D { useLayerMask = true, layerMask = Layers.StandableMask, useTriggers = false };
            _solidGroundFilter = new ContactFilter2D { useLayerMask = true, layerMask = Layers.Mask(Layers.GroundIndex), useTriggers = false };
        }

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (_platformDrop != null) _platformDrop.enabled = IsOwner;
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
            UpdateWall(deltaTime);
            _jumpGate.SetGrounded(_grounded, _data.CoyoteTime);
            if (_grounded) _jumpGate.RefillAirJumps(_data.AirJumps);

            ApplyHorizontal(deltaTime);
            if (_jumpGate.TryConsume()) Jump(_data.JumpVelocity);
            else if (!ControlSuspended && _jumpGate.TryConsumeWallJump(_wallCoyote > 0f)) WallJump();
            else if (_jumpGate.TryConsumeAirJump()) Jump(_data.AirJumpVelocity);
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
            var dropping = _platformDrop != null && _platformDrop.IsDropping;
            var hits = Physics2D.OverlapBox(
                _groundCheck.position, _data.GroundCheckSize, 0f, dropping ? _solidGroundFilter : _groundFilter, _groundHits);
            var grounded = hits > 0 && _body.linearVelocityY <= 0.01f;

            if (grounded && !_grounded) Landed?.Invoke();
            _grounded = grounded;
        }

        private void UpdateWall(float deltaTime)
        {
            _wallSide = _grounded ? 0f : _wallProbe.Probe(_collider.bounds, _data.WallCheckSize);
            if (_wallSide != 0f)
            {
                _lastWallSide = _wallSide;
                _wallCoyote = _data.WallCoyoteTime;
            }
            else
            {
                _wallCoyote = Mathf.Max(0f, _wallCoyote - deltaTime);
            }
            IsWallSliding = _wallSide != 0f && _body.linearVelocityY < 0f && _input.Move.x * _wallSide > 0.5f;
        }

        private void ApplyHorizontal(float deltaTime)
        {
            var inputX = Mathf.Clamp(_input.Move.x, -1f, 1f);
            if (Mathf.Abs(inputX) > 0.01f) _facing = Mathf.Sign(inputX);
            if (ControlSuspended || ControlLocked) return;

            var slow = _slowTimer > 0f ? _slowMultiplier : 1f;
            var buffMult = Run.PartyBuffTuning.TryGetActive(out var buffs, out var buff) ? buffs.MoveMultiplier(buff) : 1f;
            var wanted = inputX * _data.RunSpeed * SpeedMultiplier * slow * buffMult;
            var changing = Mathf.Abs(wanted) > 0.01f;
            var rate = changing ? _data.Acceleration : _data.Deceleration;
            if (!_grounded) rate *= _data.AirControl;

            var speed = Mathf.MoveTowards(_body.linearVelocityX, wanted, rate * deltaTime);
            _body.linearVelocityX = speed;
        }

        private void Jump(float velocity)
        {
            _body.linearVelocityY = velocity;
            _grounded = false;
            Jumped?.Invoke();
        }

        private void WallJump()
        {
            var away = -_lastWallSide;
            _body.linearVelocity = new Vector2(away * _data.WallJumpPush, _data.WallJumpVelocity);
            _facing = away;
            _wallCoyote = 0f;
            LockControl(_data.WallJumpControlLock);
            Jumped?.Invoke();
        }

        private void ApplyGravity()
        {
            if (GravitySuspended)
            {
                _body.gravityScale = 0f;
                return;
            }
            var velocityY = _body.linearVelocityY;
            var rising = velocityY > 0.01f;
            var scale = rising && _jumpHeld ? _data.GravityScale : _data.GravityScale * _data.FallGravityMultiplier;
            if (_jumpHeld && !_grounded && Mathf.Abs(velocityY) < _data.ApexThreshold)
                scale = _data.GravityScale * _data.ApexGravityMultiplier;

            var fastFall = !_grounded && velocityY < 0f && DownHeld;
            if (fastFall) scale = _data.GravityScale * _data.FallGravityMultiplier * _data.FastFallGravityMultiplier;
            _body.gravityScale = scale;

            var maxFall = fastFall ? _data.FastFallMaxSpeed : IsWallSliding ? _data.WallSlideSpeed : _data.MaxFallSpeed;
            if (velocityY < -maxFall) _body.linearVelocityY = -maxFall;
        }

        private void OnJumpPressed()
        {
            if (_grounded && DownHeld && _platformDrop != null && _platformDrop.TryDrop())
            {
                _grounded = false;
                return;
            }
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
