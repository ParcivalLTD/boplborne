using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class NicknameRulesTests
    {
        [Test]
        public void SanitizeTrimsAndCollapsesSpaces()
        {
            Assert.AreEqual("Ada Lovelace", NicknameRules.Sanitize("  Ada    Lovelace  "));
        }

        [Test]
        public void SanitizeDropsRichTextAndControlCharacters()
        {
            var result = NicknameRules.Sanitize("<size=99>Big</size>");
            Assert.IsFalse(result.Contains("<"));
            Assert.IsFalse(result.Contains(">"));
            Assert.AreEqual("Hi", NicknameRules.Sanitize("H\ni\t\u0007"));
        }

        [Test]
        public void SanitizeCutsToTheMaximumLength()
        {
            var result = NicknameRules.Sanitize("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Assert.AreEqual(NicknameRules.MaxLength, result.Length);
            Assert.AreEqual("ABCDEFGHIJKLMNOP", result);
        }

        [Test]
        public void SanitizeOfNothingIsEmpty()
        {
            Assert.AreEqual(string.Empty, NicknameRules.Sanitize(null));
            Assert.AreEqual(string.Empty, NicknameRules.Sanitize("   \n "));
            Assert.AreEqual(string.Empty, NicknameRules.Sanitize("<>"));
        }

        [Test]
        public void UniqueLeavesAFreeNameAlone()
        {
            Assert.AreEqual("Ada", NicknameRules.Unique("Ada", new[] { "Bob" }));
        }

        [Test]
        public void UniqueNumbersATakenNameIgnoringCase()
        {
            Assert.AreEqual("Ada 2", NicknameRules.Unique("Ada", new[] { "ada" }));
            Assert.AreEqual("Ada 3", NicknameRules.Unique("Ada", new[] { "Ada", "Ada 2" }));
        }

        [Test]
        public void UniqueStaysWithinTheMaximumLength()
        {
            var full = "ABCDEFGHIJKLMNOP";
            var result = NicknameRules.Unique(full, new[] { full });
            Assert.LessOrEqual(result.Length, NicknameRules.MaxLength);
            Assert.AreNotEqual(full, result);
            Assert.IsTrue(result.EndsWith(" 2"));
        }

        [Test]
        public void UniqueKeepsEmptyEmpty()
        {
            Assert.AreEqual(string.Empty, NicknameRules.Unique(string.Empty, new[] { string.Empty }));
        }

        [Test]
        public void DisplayFallsBackToTheSlotNumber()
        {
            Assert.AreEqual("Ada", NicknameRules.Display("Ada", 2));
            Assert.AreEqual("P3", NicknameRules.Display("", 2));
            Assert.AreEqual(string.Empty, NicknameRules.Display(null, -1));
        }
    }
}
