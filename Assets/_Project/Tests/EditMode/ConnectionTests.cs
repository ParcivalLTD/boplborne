using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    /// <summary>Connection rules, address parsing and launch options from docs/NETWORKING.md.</summary>
    public class ConnectionTests
    {
        private const string Version = "0.1.0";

        [Test]
        public void Approval_AllowsAFreeSlotOnTheSameVersion()
        {
            Assert.AreEqual(
                ApprovalResult.Approved, ConnectionApproval.Evaluate(Version, Version, 3, 4, false));
        }

        [Test]
        public void Approval_RefusesTheFifthPlayer()
        {
            Assert.AreEqual(
                ApprovalResult.ServerFull, ConnectionApproval.Evaluate(Version, Version, 4, 4, false));
        }

        [Test]
        public void Approval_RefusesAVersionMismatch()
        {
            Assert.AreEqual(
                ApprovalResult.VersionMismatch, ConnectionApproval.Evaluate("0.2.0", Version, 0, 4, false));
        }

        [Test]
        public void Approval_RefusesAJoinDuringAFight()
        {
            Assert.AreEqual(
                ApprovalResult.FightInProgress, ConnectionApproval.Evaluate(Version, Version, 1, 4, true));
        }

        [Test]
        public void Approval_RefusesAnEmptyPayload()
        {
            Assert.AreEqual(ApprovalResult.BadPayload, ConnectionApproval.Evaluate(null, Version, 0, 4, false));
        }

        [Test]
        public void Payload_RoundTripsTheGameVersion()
        {
            Assert.IsTrue(ConnectionPayload.TryDeserialize(ConnectionPayload.Serialize(Version), out var payload));
            Assert.AreEqual(Version, payload.GameVersion);
        }

        [Test]
        public void Address_UsesTheDefaultPortWhenNoneIsTyped()
        {
            Assert.IsTrue(AddressParser.TryParse(" 127.0.0.1 ", 7777, out var address, out var port));
            Assert.AreEqual("127.0.0.1", address);
            Assert.AreEqual(7777, port);
        }

        [Test]
        public void Address_ReadsAPortAfterTheColon()
        {
            Assert.IsTrue(AddressParser.TryParse("25.1.2.3:7778", 7777, out var address, out var port));
            Assert.AreEqual("25.1.2.3", address);
            Assert.AreEqual(7778, port);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("25.1.2.3:")]
        [TestCase("25.1.2.3:0")]
        [TestCase(":7777")]
        public void Address_RejectsNonsense(string input)
        {
            Assert.IsFalse(AddressParser.TryParse(input, 7777, out _, out _));
        }

        [Test]
        public void Args_ReadServerPortAndConnect()
        {
            var args = CommandLineArgs.Parse(new[] { "GraceRun.exe", "-server", "-port", "7778" });
            Assert.IsTrue(args.StartServer);
            Assert.AreEqual((ushort)7778, args.Port);
            Assert.IsFalse(args.HasConnectAddress);

            var client = CommandLineArgs.Parse(new[] { "GraceRun.exe", "-connect", "127.0.0.1:7778" });
            Assert.IsFalse(client.StartServer);
            Assert.AreEqual("127.0.0.1:7778", client.ConnectAddress);
            Assert.IsNull(client.Port);
        }
    }
}
