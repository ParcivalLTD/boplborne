using GraceRun.Core.Audio;
using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class VolumeMathTests
    {
        [Test]
        public void FullSliderIsZeroDb() => Assert.AreEqual(0f, VolumeMath.SliderToDb(1f), 0.001f);

        [Test]
        public void ZeroSliderIsTheFloor() => Assert.AreEqual(VolumeMath.MinDb, VolumeMath.SliderToDb(0f));

        [Test]
        public void HalfSliderIsAboutMinusSixDb() => Assert.AreEqual(-6.02f, VolumeMath.SliderToDb(0.5f), 0.05f);

        [Test]
        public void SliderIsClamped()
        {
            Assert.AreEqual(0f, VolumeMath.SliderToDb(3f), 0.001f);
            Assert.AreEqual(VolumeMath.MinDb, VolumeMath.SliderToDb(-1f));
        }

        [Test]
        public void DbToSliderRoundTrips()
        {
            foreach (var slider in new[] { 0.05f, 0.25f, 0.5f, 0.8f, 1f })
                Assert.AreEqual(slider, VolumeMath.DbToSlider(VolumeMath.SliderToDb(slider)), 0.001f);
        }

        [Test]
        public void FloorAndLouderMapToTheEnds()
        {
            Assert.AreEqual(0f, VolumeMath.DbToSlider(VolumeMath.MinDb));
            Assert.AreEqual(1f, VolumeMath.DbToSlider(6f));
        }

        [Test]
        public void SliderMovesTheLevelUp()
        {
            Assert.Less(VolumeMath.SliderToDb(0.3f), VolumeMath.SliderToDb(0.4f));
        }
    }

    public class MusicRulesTests
    {
        [Test]
        public void MenuAndHubUseTheHubTrack()
        {
            Assert.AreEqual(MusicCue.Hub, MusicRules.Pick(false, FightPhase.Fighting, false));
            Assert.AreEqual(MusicCue.Hub, MusicRules.Pick(true, FightPhase.Waiting, true));
            Assert.AreEqual(MusicCue.Hub, MusicRules.Pick(true, FightPhase.Draft, true));
        }

        [Test]
        public void FightsUseTheBossTrackFromTheCountdown()
        {
            Assert.AreEqual(MusicCue.Boss, MusicRules.Pick(true, FightPhase.Countdown, false));
            Assert.AreEqual(MusicCue.Boss, MusicRules.Pick(true, FightPhase.Fighting, false));
        }

        [Test]
        public void VictoryDraftRestAndDefeatEachHaveTheirMusic()
        {
            Assert.AreEqual(MusicCue.Victory, MusicRules.Pick(true, FightPhase.Victory, false));
            Assert.AreEqual(MusicCue.Victory, MusicRules.Pick(true, FightPhase.Draft, false));
            Assert.AreEqual(MusicCue.Rest, MusicRules.Pick(true, FightPhase.RestStop, false));
            Assert.AreEqual(MusicCue.Defeat, MusicRules.Pick(true, FightPhase.Defeat, false));
        }
    }
}
