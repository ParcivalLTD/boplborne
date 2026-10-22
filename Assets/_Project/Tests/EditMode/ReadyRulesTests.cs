using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ReadyRulesTests
    {
        [Test]
        public void NobodyConnectedIsNeverReady() => Assert.IsFalse(ReadyRules.AllReady(0, 0));

        [Test]
        public void AllReadyWhenEveryoneIsReady() => Assert.IsTrue(ReadyRules.AllReady(3, 3));

        [Test]
        public void NotAllReadyWhileSomeoneWaits() => Assert.IsFalse(ReadyRules.AllReady(3, 2));

        [Test]
        public void SoloPlayerReadyStarts() => Assert.IsTrue(ReadyRules.AllReady(1, 1));
    }
}
