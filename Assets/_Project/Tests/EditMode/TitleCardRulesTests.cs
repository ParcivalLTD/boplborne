using GraceRun.UI.TitleCards;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class TitleCardRulesTests
    {
        [Test]
        public void IntroTitleNamesTheFloorAndBoss() =>
            Assert.AreEqual("Floor 1: The Gatekeeper", TitleCardRules.IntroTitle(1, false, "The Gatekeeper"));

        [Test]
        public void IntroTitleMarksTheFinalFloor() =>
            Assert.AreEqual("Final Floor: The Bell Twins", TitleCardRules.IntroTitle(3, true, "The Bell Twins"));

        [Test]
        public void IntroTitleWithoutFloorIsJustTheName() =>
            Assert.AreEqual("The Gatekeeper", TitleCardRules.IntroTitle(0, false, "The Gatekeeper"));

        [Test]
        public void QuantizeHoldsFramesAt12Fps() =>
            Assert.AreEqual(2f / 12f, TitleCardRules.Quantize(0.2f, 12f), 0.0001f);

        [Test]
        public void ProgressStartsAtZeroAndEntersToOne()
        {
            var start = TitleCardRules.Progress(0f, 0.35f, 1.3f, 0.3f, 12f);
            Assert.AreEqual(0f, start.enter, 0.0001f);
            Assert.IsFalse(start.done);
            var held = TitleCardRules.Progress(1f, 0.35f, 1.3f, 0.3f, 12f);
            Assert.AreEqual(1f, held.enter, 0.0001f);
            Assert.AreEqual(0f, held.exit, 0.0001f);
        }

        [Test]
        public void ProgressLeavesAndEndsAfterTheTotalTime()
        {
            var leaving = TitleCardRules.Progress(1.8f, 0.35f, 1.3f, 0.3f, 12f);
            Assert.Greater(leaving.exit, 0f);
            Assert.IsFalse(leaving.done);
            Assert.IsTrue(TitleCardRules.Progress(2f, 0.35f, 1.3f, 0.3f, 12f).done);
        }

        [Test]
        public void EaseOutBackOvershootsThenSettles()
        {
            Assert.Greater(TitleCardRules.EaseOutBack(0.7f), 1f);
            Assert.AreEqual(1f, TitleCardRules.EaseOutBack(1f), 0.0001f);
        }

        [Test]
        public void SkipJumpsToTheExitAndNeverBackwards()
        {
            Assert.AreEqual(1.65f, TitleCardRules.SkipTime(0.2f, 0.35f, 1.3f), 0.0001f);
            Assert.AreEqual(1.9f, TitleCardRules.SkipTime(1.9f, 0.35f, 1.3f), 0.0001f);
        }
    }
}
