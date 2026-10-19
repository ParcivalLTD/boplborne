using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The two bits of forgiveness around a jump: coyote time (you may still jump just after
    /// leaving the ground) and the jump buffer (a press just before landing is remembered).
    /// Pure logic so it can be tested without a scene.
    /// </summary>
    public class JumpGate
    {
        private float _coyoteLeft;
        private float _bufferLeft;
        private int _airJumpsLeft;

        public bool HasCoyote => _coyoteLeft > 0f;
        public bool HasBufferedPress => _bufferLeft > 0f;
        public int AirJumpsLeft => _airJumpsLeft;

        /// <summary>Gives back the air jumps (call while grounded).</summary>
        public void RefillAirJumps(int airJumps) => _airJumpsLeft = airJumps;

        /// <summary>
        /// True once when a press arrives with no floor in reach and an air jump left; spends it.
        /// Check <see cref="TryConsume"/> first so a ground jump never costs an air jump.
        /// </summary>
        public bool TryConsumeAirJump()
        {
            if (_bufferLeft <= 0f || _coyoteLeft > 0f || _airJumpsLeft <= 0) return false;
            _airJumpsLeft--;
            _bufferLeft = 0f;
            return true;
        }

        /// <summary>
        /// True once when a press arrives in the air while a wall is in reach; spends the press but
        /// no air jump. Check <see cref="TryConsume"/> first, and this before <see cref="TryConsumeAirJump"/>.
        /// </summary>
        public bool TryConsumeWallJump(bool wallInReach)
        {
            if (!wallInReach || _bufferLeft <= 0f || _coyoteLeft > 0f) return false;
            _bufferLeft = 0f;
            return true;
        }

        /// <summary>Refills coyote time while grounded; it drains once the feet leave the floor.</summary>
        public void SetGrounded(bool grounded, float coyoteTime)
        {
            if (grounded) _coyoteLeft = coyoteTime;
        }

        /// <summary>Remembers a jump press for the length of the buffer.</summary>
        public void Press(float jumpBuffer) => _bufferLeft = jumpBuffer;

        public void Tick(float deltaTime)
        {
            _coyoteLeft = Mathf.Max(0f, _coyoteLeft - deltaTime);
            _bufferLeft = Mathf.Max(0f, _bufferLeft - deltaTime);
        }

        /// <summary>True once when a remembered press meets a floor that is still in reach.</summary>
        public bool TryConsume()
        {
            if (_coyoteLeft <= 0f || _bufferLeft <= 0f) return false;
            Clear();
            return true;
        }

        /// <summary>Forgets both timers, so one press can never produce two jumps.</summary>
        public void Clear()
        {
            _coyoteLeft = 0f;
            _bufferLeft = 0f;
        }
    }
}
