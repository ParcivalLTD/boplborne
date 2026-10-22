using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class CastPoseMathTests
    {
        [Test]
        public void PhasesRunWindupReleaseRecoveryThenEnd()
        {
            Assert.AreEqual(CastPhase.Windup, CastPoseMath.PhaseAt(0.05f, 0.1f, 0.2f));
            Assert.AreEqual(CastPhase.Release, CastPoseMath.PhaseAt(0.1f, 0.1f, 0.2f));
            Assert.AreEqual(CastPhase.Recovery, CastPoseMath.PhaseAt(0.1f + CastPoseMath.ReleaseHold, 0.1f, 0.2f));
            Assert.AreEqual(CastPhase.None, CastPoseMath.PhaseAt(0.1f + CastPoseMath.ReleaseHold + 0.2f + 0.001f, 0.1f, 0.2f));
        }

        [Test]
        public void NoWindupStartsAtTheRelease()
        {
            Assert.AreEqual(CastPhase.Release, CastPoseMath.PhaseAt(0f, 0f, 0.2f));
        }

        [Test]
        public void BeforeTheCastIsNone() => Assert.AreEqual(CastPhase.None, CastPoseMath.PhaseAt(-1f, 0.1f, 0.2f));

        [Test]
        public void DurationAddsUp() => Assert.AreEqual(0.1f + CastPoseMath.ReleaseHold + 0.2f, CastPoseMath.Duration(0.1f, 0.2f), 0.0001f);
    }
}
