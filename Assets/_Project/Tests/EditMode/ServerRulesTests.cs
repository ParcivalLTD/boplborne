using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ServerRulesTests
    {
        [Test]
        public void ResetsAfterTheGraceTimeWithNobodyConnected()
        {
            Assert.IsFalse(ServerRules.ShouldResetToHub(0, 29.9, 30, false));
            Assert.IsTrue(ServerRules.ShouldResetToHub(0, 30, 30, false));
        }

        [Test]
        public void NeverResetsWhileSomeoneIsConnected()
        {
            Assert.IsFalse(ServerRules.ShouldResetToHub(1, 999, 30, false));
        }

        [Test]
        public void DoesNotResetAServerAlreadySittingCleanInTheHub()
        {
            Assert.IsFalse(ServerRules.ShouldResetToHub(0, 999, 30, true));
        }

        [Test]
        public void ZeroGraceResetsAtOnce()
        {
            Assert.IsTrue(ServerRules.ShouldResetToHub(0, 0, 0, false));
        }

        [TestCase(45, "45s")]
        [TestCase(750, "12m 30s")]
        [TestCase(3725, "1h 02m")]
        [TestCase(90000 + 3600 * 4 + 300, "1d 5h 05m")]
        [TestCase(-5, "0s")]
        public void FormatsUptime(double seconds, string expected)
        {
            Assert.AreEqual(expected, ServerRules.FormatUptime(seconds));
        }

        [Test]
        public void StatusLineNamesTheHubAndFloors()
        {
            Assert.AreEqual("[Server] players 0/4, Waiting, hub, up 5s", ServerRules.FormatStatus(0, 4, "Waiting", 0, 5));
            Assert.AreEqual("[Server] players 2/4, Fighting, floor 3, up 1h 02m", ServerRules.FormatStatus(2, 4, "Fighting", 3, 3725));
        }
    }
}
