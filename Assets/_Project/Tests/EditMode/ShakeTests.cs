using GraceRun.Core.Juice;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ShakeTests
    {
        [Test]
        public void ForceScalesWithStrengthAndMultiplier()
        {
            Assert.AreEqual(0.8f, Shake.ComputeForce(2f, 1f, 0.4f), 0.0001f);
            Assert.AreEqual(0.2f, Shake.ComputeForce(1f, 0.5f, 0.4f), 0.0001f);
        }

        [Test]
        public void ZeroMultiplierTurnsShakeOff()
        {
            Assert.AreEqual(0f, Shake.ComputeForce(1f, 0f, 0.4f));
        }

        [Test]
        public void NegativeInputsNeverProduceForce()
        {
            Assert.AreEqual(0f, Shake.ComputeForce(-1f, 1f, 0.4f));
            Assert.AreEqual(0f, Shake.ComputeForce(1f, -1f, 0.4f));
        }
    }
}
