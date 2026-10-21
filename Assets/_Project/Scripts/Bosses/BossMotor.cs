using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Moves and turns the boss. The server drives the body (server-auth NetworkTransform); the
    /// facing is a server-write NetworkVariable that every PC uses to mirror the visuals.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossMotor : NetworkBehaviour
    {
        [SerializeField] private BossData _data;
        [SerializeField, Tooltip("Mirrored on x when the boss faces left.")]
        private Transform _visuals;

        private readonly NetworkVariable<sbyte> _facing = new(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Rigidbody2D _body;
        private float _defaultGravity;
        private float _moveDirection;
        private float _moveTargetX;
        private float _dashVelocity;
        private float _dashTimer;
        private bool _perched;
        private Vector2 _perchPosition;
        private float _dropVelocity;
        private float _dropTimer;
        private float _speedMultiplier = 1f;
        private float _environmentSlow = 1f;
        private float _environmentSlowUntil;

        /// <summary>Server only: walking speed scale from status effects (a frost slow). 1 is normal.</summary>
        public float StatusSpeedMultiplier { get; set; } = 1f;

        /// <summary>+1 facing right, -1 facing left.</summary>
        public sbyte Facing => _facing.Value;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _defaultGravity = _body.gravityScale;
        }

        public override void OnNetworkSpawn()
        {
            _speedMultiplier = 1f;
            _facing.OnValueChanged += OnFacingChanged;
            ApplyFacing(_facing.Value);
        }

        public override void OnNetworkDespawn()
        {
            _facing.OnValueChanged -= OnFacingChanged;
        }

        private void FixedUpdate()
        {
            if (!IsServer || !IsSpawned) return;
            if (_perched)
            {
                _body.linearVelocity = Vector2.zero;
                _body.MovePosition(_perchPosition);
                return;
            }
            if (_dropTimer > 0f)
            {
                _dropTimer -= Time.fixedDeltaTime;
                _body.linearVelocity = new Vector2(0f, _dropVelocity);
                return;
            }
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.fixedDeltaTime;
                _body.linearVelocityX = _dashVelocity;
                return;
            }

            var chase = BossMath.ChaseSpeedMultiplier(Mathf.Abs(_moveTargetX - _body.position.x), _data.ChaseDistance, _data.ChaseSpeedMultiplier);
            var environment = Time.time < _environmentSlowUntil ? _environmentSlow : 1f;
            _body.linearVelocityX = _moveDirection * (_data.WalkSpeed * _speedMultiplier * chase * StatusSpeedMultiplier * environment);
        }

        /// <summary>Server only: slows walking while standing in a hazard (web). Call every physics step; it lapses a moment after the last call.</summary>
        public void ServerApplyEnvironmentSlow(float multiplier)
        {
            if (!IsServer) return;
            var active = Time.time < _environmentSlowUntil;
            _environmentSlow = active ? Mathf.Min(_environmentSlow, multiplier) : multiplier;
            _environmentSlowUntil = Time.time + 0.1f;
        }

        /// <summary>Server only: scales walking speed (e.g. when a surviving Bell Twin enrages).</summary>
        public void ServerSetSpeedMultiplier(float multiplier)
        {
            if (IsServer) _speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        /// <summary>Server only: walks toward <paramref name="targetX"/> and faces it.</summary>
        public void ServerMoveToward(float targetX)
        {
            _moveTargetX = targetX;
            _moveDirection = Mathf.Sign(targetX - _body.position.x);
            ServerFace(_moveDirection);
        }

        /// <summary>Server only: stands still (also ends a dash or perch).</summary>
        public void ServerStop()
        {
            _moveDirection = 0f;
            _dashTimer = 0f;
            ServerClearVertical();
        }

        /// <summary>Server only: moves at <paramref name="velocity"/> on x for <paramref name="duration"/> seconds, ignoring walking.</summary>
        public void ServerDash(float velocity, float duration)
        {
            if (!IsServer) return;
            _dashVelocity = velocity;
            _dashTimer = duration;
        }

        /// <summary>Server only: holds the boss at <paramref name="position"/> (e.g. ceiling perch during wind-up) with gravity off.</summary>
        public void ServerPerch(Vector2 position)
        {
            if (!IsServer) return;
            _perched = true;
            _perchPosition = position;
            _dropTimer = 0f;
            _dashTimer = 0f;
            _moveDirection = 0f;
            _body.gravityScale = 0f;
            _body.linearVelocity = Vector2.zero;
            _body.position = position;
            transform.position = position;
        }

        /// <summary>Server only: releases a perch and slams straight down at <paramref name="downSpeed"/> for <paramref name="duration"/> seconds.</summary>
        public void ServerDrop(float downSpeed, float duration)
        {
            if (!IsServer) return;
            _perched = false;
            _body.gravityScale = _defaultGravity;
            _dropVelocity = -Mathf.Abs(downSpeed);
            _dropTimer = duration;
        }

        /// <summary>Server only: restores normal gravity and clears any ceiling perch or vertical drop.</summary>
        public void ServerClearVertical()
        {
            if (!IsServer) return;
            _perched = false;
            _dropTimer = 0f;
            if (_body != null) _body.gravityScale = _defaultGravity;
        }

        /// <summary>Server only: turns toward the sign of <paramref name="direction"/> (0 keeps the facing).</summary>
        public void ServerFace(float direction)
        {
            if (!IsServer || Mathf.Approximately(direction, 0f)) return;
            _facing.Value = (sbyte)(direction > 0f ? 1 : -1);
        }

        /// <summary>+1 or -1 toward <paramref name="targetX"/>, or the current facing when level with it.</summary>
        public sbyte DirectionTo(float targetX)
        {
            var dx = targetX - _body.position.x;
            return Mathf.Approximately(dx, 0f) ? _facing.Value : (sbyte)(dx > 0f ? 1 : -1);
        }

        private void OnFacingChanged(sbyte previous, sbyte current) => ApplyFacing(current);

        private void ApplyFacing(sbyte facing)
        {
            var scale = _visuals.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            _visuals.localScale = scale;
        }
    }
}
