using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class JoinCodeRulesTests
    {
        [TestCase("K7M2QX", true)]
        [TestCase("  k7m2qx ", true)]
        [TestCase("123456", true)]
        [TestCase("25.1.2.3", false)]
        [TestCase("25.1.2.3:7777", false)]
        [TestCase("K7M2Q", false)]
        [TestCase("K7M2QXZ", false)]
        [TestCase("K7M-QX", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void LooksLikeCode(string input, bool expected) =>
            Assert.AreEqual(expected, JoinCodeRules.LooksLikeCode(input));

        [Test]
        public void NormalizeTrimsAndUppercases() =>
            Assert.AreEqual("K7M2QX", JoinCodeRules.Normalize(" k7m2qx "));
    }
}
