using GraceRun.Core.Settings;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class SettingsRulesTests
    {
        [Test]
        public void StepMovesByOneStepAndSnapsToTheGrid()
        {
            Assert.AreEqual(0.9f, SettingsRules.Step(0.8f, 1, 0.1f, 0f, 1f), 0.0001f);
            Assert.AreEqual(0.3f, SettingsRules.Step(0.33f, 0, 0.1f, 0f, 1f), 0.0001f);
        }

        [Test]
        public void StepStaysInsideTheLimits()
        {
            Assert.AreEqual(1f, SettingsRules.Step(1f, 1, 0.1f, 0f, 1f), 0.0001f);
            Assert.AreEqual(0f, SettingsRules.Step(0.05f, -3, 0.1f, 0f, 1f), 0.0001f);
            Assert.AreEqual(2f, SettingsRules.Step(1.95f, 5, 0.1f, 0f, 2f), 0.0001f);
        }

        [Test]
        public void CycleWrapsBothWays()
        {
            Assert.AreEqual(0, SettingsRules.Cycle(2, 1, 3));
            Assert.AreEqual(2, SettingsRules.Cycle(0, -1, 3));
            Assert.AreEqual(0, SettingsRules.Cycle(5, 1, 0));
        }

        [Test]
        public void LabelsReadNaturally()
        {
            Assert.AreEqual("80%", SettingsRules.Percent(0.8f));
            Assert.AreEqual("On", SettingsRules.OnOff(true));
            Assert.AreEqual("1920 × 1080", SettingsRules.ResolutionLabel(1920, 1080));
        }
    }
}
