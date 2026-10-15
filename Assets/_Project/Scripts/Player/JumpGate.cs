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

        public bool HasCoyote => _coyoteLeft > 0f;
        public bool HasBufferedPress => _bufferLeft > 0f;

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
