using GraceRun.UI.Splash;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class SplashRulesTests
    {
        [Test]
        public void CoverLiftsOverTheFadeTime()
        {
            Assert.AreEqual(1f, SplashRules.CoverAlpha(0f, 0.5f), 1e-4f);
            Assert.AreEqual(0.5f, SplashRules.CoverAlpha(0.25f, 0.5f), 1e-4f);
            Assert.AreEqual(0f, SplashRules.CoverAlpha(2f, 0.5f), 1e-4f);
        }

        [Test]
        public void PanelFadesOnlyAtTheEnd()
        {
            Assert.AreEqual(1f, SplashRules.PanelAlpha(1f, 3f, 0.5f), 1e-4f);
            Assert.AreEqual(0.5f, SplashRules.PanelAlpha(2.75f, 3f, 0.5f), 1e-4f);
            Assert.AreEqual(0f, SplashRules.PanelAlpha(3.5f, 3f, 0.5f), 1e-4f);
        }

        [Test]
        public void ZeroFadeCutsHard()
        {
            Assert.AreEqual(0f, SplashRules.CoverAlpha(0f, 0f));
            Assert.AreEqual(1f, SplashRules.PanelAlpha(2.9f, 3f, 0f));
            Assert.AreEqual(0f, SplashRules.PanelAlpha(3f, 3f, 0f));
        }

        [Test]
        public void SkipWaitsForTheGracePeriod()
        {
            Assert.IsFalse(SplashRules.CanSkip(0.1f, 0.4f));
            Assert.IsTrue(SplashRules.CanSkip(0.4f, 0.4f));
        }

        [Test]
        public void IsDoneAtTheTotal()
        {
            Assert.IsFalse(SplashRules.IsDone(2.9f, 3f));
            Assert.IsTrue(SplashRules.IsDone(3f, 3f));
        }
    }
}
