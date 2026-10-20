using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>Picks the locomotion and packs it with the facing into one byte for the network.</summary>
    public static class PlayerLocomotionMath
    {
        private const byte FacingLeftBit = 0x80;

        public static PlayerLocomotion Pick(bool grounded, Vector2 velocity, float runThreshold)
        {
            if (grounded) return Mathf.Abs(velocity.x) > runThreshold ? PlayerLocomotion.Run : PlayerLocomotion.Idle;
            return velocity.y > 0f ? PlayerLocomotion.Jump : PlayerLocomotion.Fall;
        }

        public static byte Pack(PlayerLocomotion locomotion, float facing) =>
            (byte)((byte)locomotion | (facing < 0f ? FacingLeftBit : 0));

        public static PlayerLocomotion Locomotion(byte packed) => (PlayerLocomotion)(packed & ~FacingLeftBit);

        public static float Facing(byte packed) => (packed & FacingLeftBit) != 0 ? -1f : 1f;
    }
}
