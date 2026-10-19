using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class StatusFileArgTests
    {
        [Test]
        public void ParsesTheStatusFilePath()
        {
            var args = CommandLineArgs.Parse(new[] { "game.exe", "-server", "-statusfile", "C:\\srv\\status.json", "-port", "9000" });
            Assert.IsTrue(args.StartServer);
            Assert.AreEqual("C:\\srv\\status.json", args.StatusFile);
            Assert.AreEqual((ushort)9000, args.Port);
        }

        [Test]
        public void StatusFileIsNullWhenNotGiven()
        {
            Assert.IsNull(CommandLineArgs.Parse(new[] { "game.exe", "-server" }).StatusFile);
            Assert.IsNull(CommandLineArgs.Parse(new[] { "game.exe", "-statusfile" }).StatusFile);
        }
    }
}
