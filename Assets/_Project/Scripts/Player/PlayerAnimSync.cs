using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The owner's locomotion (idle, run, jump, fall) and facing, shared with everyone so other
    /// PCs play the same clip. The owner simulates its own movement, so it writes this
    /// (owner-write NetworkVariable, one byte, only when it changes). Cosmetic only.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerAnimSync : NetworkBehaviour
    {
        [SerializeField] private PlayerAnimData _data;
        [SerializeField] private PlayerMovement _movement;

        private readonly NetworkVariable<byte> _state = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private Rigidbody2D _body;

        public PlayerLocomotion Locomotion => PlayerLocomotionMath.Locomotion(_state.Value);

        /// <summary>+1 facing right, -1 facing left.</summary>
        public float Facing => PlayerLocomotionMath.Facing(_state.Value);

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!IsSpawned || !IsOwner) return;

            var grounded = _movement.IsGrounded && !_movement.IsWallSliding;
            var locomotion = PlayerLocomotionMath.Pick(grounded, _body.linearVelocity, _data.RunSpeedThreshold);
            var packed = PlayerLocomotionMath.Pack(locomotion, _movement.Facing);
            if (packed != _state.Value) _state.Value = packed;
        }
    }
}
