using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Every number that decides how running and jumping feel. Tune the asset under
    /// Data/Player, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Player Movement Data", fileName = "PlayerMovementData")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("Run")]
        [SerializeField, Min(0f)] private float _runSpeed = 9f;
        [SerializeField, Min(0f)] private float _acceleration = 90f;
        [SerializeField, Min(0f)] private float _deceleration = 120f;
        [SerializeField, Range(0f, 1f)] private float _airControl = 0.65f;

        [Header("Jump")]
        [SerializeField, Min(0f)] private float _jumpHeight = 3.5f;
        [SerializeField, Range(0f, 1f)] private float _jumpCutMultiplier = 0.45f;
        [SerializeField, Min(0.01f)] private float _gravityScale = 4f;
        [SerializeField, Min(1f)] private float _fallGravityMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float _maxFallSpeed = 30f;

        [Header("Forgiveness")]
        [SerializeField, Min(0f)] private float _coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float _jumpBuffer = 0.1f;

        [Header("Ground check")]
        [SerializeField] private Vector2 _groundCheckSize = new(1.3f, 0.2f);

        [Header("Squash and stretch (local visual only)")]
        [SerializeField, Range(0f, 0.5f)] private float _jumpStretch = 0.18f;
        [SerializeField, Range(0f, 0.5f)] private float _landSquash = 0.22f;
        [SerializeField, Min(0.01f)] private float _squashRecoverTime = 0.12f;

        /// <summary>Top horizontal speed in units per second.</summary>
        public float RunSpeed => _runSpeed;

        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;

        /// <summary>How much of the ground acceleration works while airborne.</summary>
        public float AirControl => _airControl;

        /// <summary>Height of a jump held all the way, in units.</summary>
        public float JumpHeight => _jumpHeight;

        /// <summary>Rising speed is multiplied by this when the jump button is released early.</summary>
        public float JumpCutMultiplier => _jumpCutMultiplier;

        public float GravityScale => _gravityScale;

        /// <summary>Extra gravity once falling, so the arc feels snappy rather than floaty.</summary>
        public float FallGravityMultiplier => _fallGravityMultiplier;

        public float MaxFallSpeed => _maxFallSpeed;

        /// <summary>How long after walking off a ledge a jump still counts.</summary>
        public float CoyoteTime => _coyoteTime;

        /// <summary>How long before landing a jump press is remembered.</summary>
        public float JumpBuffer => _jumpBuffer;

        /// <summary>Box probed under the feet to decide whether the player is grounded.</summary>
        public Vector2 GroundCheckSize => _groundCheckSize;

        public float JumpStretch => _jumpStretch;
        public float LandSquash => _landSquash;
        public float SquashRecoverTime => _squashRecoverTime;

        /// <summary>Upward speed that reaches <see cref="JumpHeight"/> under the rising gravity.</summary>
        public float JumpVelocity =>
            Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * _gravityScale * _jumpHeight);
    }
}
