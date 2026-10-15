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
        private float _moveDirection;
        private float _dashVelocity;
        private float _dashTimer;

        /// <summary>+1 facing right, -1 facing left.</summary>
        public sbyte Facing => _facing.Value;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        public override void OnNetworkSpawn()
        {
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
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.fixedDeltaTime;
                _body.linearVelocityX = _dashVelocity;
                return;
            }

            _body.linearVelocityX = _moveDirection * _data.WalkSpeed;
        }

        /// <summary>Server only: walks toward <paramref name="targetX"/> and faces it.</summary>
        public void ServerMoveToward(float targetX)
        {
            _moveDirection = Mathf.Sign(targetX - _body.position.x);
            ServerFace(_moveDirection);
        }

        /// <summary>Server only: stands still (also ends a dash).</summary>
        public void ServerStop()
        {
            _moveDirection = 0f;
            _dashTimer = 0f;
        }

        /// <summary>Server only: moves at <paramref name="velocity"/> on x for <paramref name="duration"/> seconds, ignoring walking.</summary>
        public void ServerDash(float velocity, float duration)
        {
            if (!IsServer) return;
            _dashVelocity = velocity;
            _dashTimer = duration;
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
