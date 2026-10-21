using GraceRun.UI.Splash;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class LoadingRulesTests
    {
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void ShowsOnlyWhileBusyOutsideTheGameScene(bool busy, bool inGame, bool expected) =>
            Assert.AreEqual(expected, LoadingRules.ShouldShow(busy, inGame));

        [Test]
        public void MinimumTimeKeepsAQuickConnectOnScreen()
        {
            Assert.IsFalse(LoadingRules.CanHide(0.3f, 0.8f));
            Assert.IsTrue(LoadingRules.CanHide(0.8f, 0.8f));
        }

        [Test]
        public void AlphaMovesTowardTheTargetAndStops()
        {
            Assert.AreEqual(0.5f, LoadingRules.NextAlpha(0f, 1f, 0.125f, 0.25f), 1e-4f);
            Assert.AreEqual(1f, LoadingRules.NextAlpha(0.9f, 1f, 1f, 0.25f), 1e-4f);
            Assert.AreEqual(0f, LoadingRules.NextAlpha(0.1f, 0f, 1f, 0.25f), 1e-4f);
            Assert.AreEqual(1f, LoadingRules.NextAlpha(0f, 1f, 0.01f, 0f), 1e-4f);
        }

        [Test]
        public void TipsCycleAndWrap()
        {
            Assert.AreEqual(0, LoadingRules.TipIndex(0f, 3, 4f));
            Assert.AreEqual(1, LoadingRules.TipIndex(4.1f, 3, 4f));
            Assert.AreEqual(0, LoadingRules.TipIndex(12.5f, 3, 4f));
            Assert.AreEqual(0, LoadingRules.TipIndex(5f, 0, 4f));
        }

        [Test]
        public void DotsCycleOneToThree()
        {
            Assert.AreEqual("Opening.", LoadingRules.Dots("Opening...", 0f));
            Assert.AreEqual("Opening..", LoadingRules.Dots("Opening...", 0.45f));
            Assert.AreEqual("Opening...", LoadingRules.Dots("Opening", 0.85f));
            Assert.AreEqual("Opening.", LoadingRules.Dots("Opening", 1.25f));
        }
    }
}
