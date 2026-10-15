using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class JumpGateTests
    {
        private const float Coyote = 0.1f;
        private const float Buffer = 0.1f;

        [Test]
        public void PressWhileGroundedJumps()
        {
            var gate = new JumpGate();
            gate.SetGrounded(true, Coyote);
            gate.Press(Buffer);

            Assert.IsTrue(gate.TryConsume());
        }

        [Test]
        public void OnePressGivesOneJump()
        {
            var gate = new JumpGate();
            gate.SetGrounded(true, Coyote);
            gate.Press(Buffer);
            gate.TryConsume();

            Assert.IsFalse(gate.TryConsume());
        }

        [Test]
        public void JumpStillWorksJustAfterLeavingTheGround()
        {
            var gate = new JumpGate();
            gate.SetGrounded(true, Coyote);
            gate.SetGrounded(false, Coyote);
            gate.Tick(Coyote * 0.5f);
            gate.Press(Buffer);

            Assert.IsTrue(gate.TryConsume());
        }

        [Test]
        public void JumpFailsOnceCoyoteTimeRanOut()
        {
            var gate = new JumpGate();
            gate.SetGrounded(true, Coyote);
            gate.SetGrounded(false, Coyote);
            gate.Tick(Coyote + 0.01f);
            gate.Press(Buffer);

            Assert.IsFalse(gate.TryConsume());
        }

        [Test]
        public void PressJustBeforeLandingIsRemembered()
        {
            var gate = new JumpGate();
            gate.Press(Buffer);
            gate.Tick(Buffer * 0.5f);

            Assert.IsFalse(gate.TryConsume());

            gate.SetGrounded(true, Coyote);

            Assert.IsTrue(gate.TryConsume());
        }

        [Test]
        public void PressIsForgottenAfterTheBuffer()
        {
            var gate = new JumpGate();
            gate.Press(Buffer);
            gate.Tick(Buffer + 0.01f);
            gate.SetGrounded(true, Coyote);

            Assert.IsFalse(gate.TryConsume());
        }
    }
}
