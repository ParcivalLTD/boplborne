using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests.EditMode
{
    public class PlayerAnimationRulesTests
    {
        private const float RunThreshold = 0.25f;
        private const float AirThreshold = 0.15f;

        [Test]
        public void ResolveState_GroundedAndStill_ReturnsIdle()
        {
            var state = PlayerAnimationRules.ResolveState(
                false, false, false, false, 0, true, 0.05f, 0f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Idle, state);
        }

        [Test]
        public void ResolveState_GroundedAndMoving_ReturnsRun()
        {
            var state = PlayerAnimationRules.ResolveState(
                false, false, false, false, 0, true, 5f, 0f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Run, state);
        }

        [Test]
        public void ResolveState_AirborneRising_ReturnsJump()
        {
            var state = PlayerAnimationRules.ResolveState(
                false, false, false, false, 0, false, 2f, 6f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Jump, state);
        }

        [Test]
        public void ResolveState_AirborneFalling_ReturnsFall()
        {
            var state = PlayerAnimationRules.ResolveState(
                false, false, false, false, 0, false, 2f, -4f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Fall, state);
        }

        [TestCase(0, PlayerAnimationRules.Attack1)]
        [TestCase(1, PlayerAnimationRules.Attack2)]
        [TestCase(2, PlayerAnimationRules.Attack3)]
        public void ResolveState_Swinging_ReturnsMatchingComboClip(int hitIndex, string expected)
        {
            var state = PlayerAnimationRules.ResolveState(
                false, false, false, true, hitIndex, true, 3f, 0f, RunThreshold, AirThreshold);
            Assert.AreEqual(expected, state);
        }

        [Test]
        public void ResolveState_DodgeOverridesAttackAndMovement()
        {
            var state = PlayerAnimationRules.ResolveState(
                false, false, true, true, 1, false, 8f, 4f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Dodge, state);
        }

        [Test]
        public void ResolveState_HurtOverridesDodgeAndAttack()
        {
            var state = PlayerAnimationRules.ResolveState(
                false, true, true, true, 2, true, 4f, 0f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Hurt, state);
        }

        [Test]
        public void ResolveState_GhostOverridesEverything()
        {
            var state = PlayerAnimationRules.ResolveState(
                true, true, true, true, 2, false, 5f, -3f, RunThreshold, AirThreshold);
            Assert.AreEqual(PlayerAnimationRules.Ghost, state);
        }

        [Test]
        public void ResolveFacing_KeepsFacingWithinDeadzone_FlipsOutside()
        {
            Assert.AreEqual(-1f, PlayerAnimationRules.ResolveFacing(-1f, 0.05f, 0.15f));
            Assert.AreEqual(1f, PlayerAnimationRules.ResolveFacing(-1f, 2f, 0.15f));
            Assert.AreEqual(-1f, PlayerAnimationRules.ResolveFacing(1f, -2f, 0.15f));
        }
    }
}
