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
        [SerializeField, Min(0), Tooltip("Extra jumps in the air before touching the ground again.")]
        private int _airJumps = 1;
        [SerializeField, Min(0f), Tooltip("Height of an air jump held all the way, in units.")]
        private float _airJumpHeight = 3f;

        [Header("Air feel")]
        [SerializeField, Min(0f), Tooltip("Near the top of a held jump (vertical speed below this), gravity eases off.")]
        private float _apexThreshold = 2f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Gravity multiplier at the top of a held jump: a short hang for aiming.")]
        private float _apexGravityMultiplier = 0.55f;
        [SerializeField, Min(1f), Tooltip("Hold down while falling: extra gravity on top of the fall gravity.")]
        private float _fastFallGravityMultiplier = 1.8f;
        [SerializeField, Min(0f), Tooltip("Top falling speed while holding down.")]
        private float _fastFallMaxSpeed = 32f;
        [SerializeField, Range(0.1f, 1f), Tooltip("How far down the stick or keys must be held to count as down.")]
        private float _downInputThreshold = 0.5f;

        [Header("Walls")]
        [SerializeField, Min(0f), Tooltip("Top falling speed while pushing into a wall in the air.")]
        private float _wallSlideSpeed = 4f;
        [SerializeField, Min(0f), Tooltip("Height of a wall jump, in units.")]
        private float _wallJumpHeight = 3.2f;
        [SerializeField, Min(0f), Tooltip("Sideways speed a wall jump kicks away with.")]
        private float _wallJumpPush = 11f;
        [SerializeField, Min(0f), Tooltip("After a wall jump, input can't steer for this long, so the kick carries you off the wall.")]
        private float _wallJumpControlLock = 0.15f;
        [SerializeField, Min(0f), Tooltip("How long after leaving a wall a wall jump still counts.")]
        private float _wallCoyoteTime = 0.1f;
        [SerializeField, Tooltip("Box probed on each side to find a wall: width and height in units.")]
        private Vector2 _wallCheckSize = new(0.15f, 1.2f);

        [Header("One-way platforms")]
        [SerializeField, Min(0f), Tooltip("Down + jump on a ledge: how long the ledge lets you fall through.")]
        private float _dropThroughTime = 0.3f;

        [Header("Forgiveness")]
        [SerializeField, Min(0f)] private float _coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float _jumpBuffer = 0.1f;

        [Header("Knockback")]
        [SerializeField, Min(0f), Tooltip("After a push, input can't steer for this long, so the push carries the body.")]
        private float _knockbackControlLock = 0.4f;

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

        /// <summary>Extra jumps in the air before touching the ground again.</summary>
        public int AirJumps => _airJumps;

        public float ApexThreshold => _apexThreshold;
        public float ApexGravityMultiplier => _apexGravityMultiplier;
        public float FastFallGravityMultiplier => _fastFallGravityMultiplier;
        public float FastFallMaxSpeed => _fastFallMaxSpeed;
        public float DownInputThreshold => _downInputThreshold;
        public float WallSlideSpeed => _wallSlideSpeed;
        public float WallJumpPush => _wallJumpPush;
        public float WallJumpControlLock => _wallJumpControlLock;
        public float WallCoyoteTime => _wallCoyoteTime;
        public Vector2 WallCheckSize => _wallCheckSize;
        public float DropThroughTime => _dropThroughTime;

        /// <summary>Upward speed of a wall jump.</summary>
        public float WallJumpVelocity => VelocityFor(_wallJumpHeight);

        /// <summary>How long after walking off a ledge a jump still counts.</summary>
        public float CoyoteTime => _coyoteTime;

        /// <summary>How long before landing a jump press is remembered.</summary>
        public float JumpBuffer => _jumpBuffer;

        /// <summary>Seconds after a knockback during which input does not steer horizontally.</summary>
        public float KnockbackControlLock => _knockbackControlLock;

        /// <summary>Box probed under the feet to decide whether the player is grounded.</summary>
        public Vector2 GroundCheckSize => _groundCheckSize;

        public float JumpStretch => _jumpStretch;
        public float LandSquash => _landSquash;
        public float SquashRecoverTime => _squashRecoverTime;

        /// <summary>Upward speed that reaches <see cref="JumpHeight"/> under the rising gravity.</summary>
        public float JumpVelocity => VelocityFor(_jumpHeight);

        /// <summary>Upward speed that reaches the air jump height under the rising gravity.</summary>
        public float AirJumpVelocity => VelocityFor(_airJumpHeight);

        private float VelocityFor(float height) =>
            Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * _gravityScale * height);
    }
}
