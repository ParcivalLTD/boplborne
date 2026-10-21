using GraceRun.UI.Story;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class StoryRulesTests
    {
        private static readonly string[] Captions = { "one", "two", "three", "four", "five", "six", "seven" };

        [Test]
        public void CaptionFollowsTheFloorAndTheFinalFloorHasItsOwn()
        {
            Assert.AreEqual("one", StoryRules.AscentCaption(1, 8, Captions, "top"));
            Assert.AreEqual("seven", StoryRules.AscentCaption(7, 8, Captions, "top"));
            Assert.AreEqual("top", StoryRules.AscentCaption(8, 8, Captions, "top"));
            Assert.AreEqual("", StoryRules.AscentCaption(0, 8, Captions, "top"));
            Assert.AreEqual("", StoryRules.AscentCaption(3, 8, null, "top"));
        }

        [Test]
        public void OnlyUpwardMovesToRealFloorsAreAscents()
        {
            Assert.IsTrue(StoryRules.IsAscent(0, 1));
            Assert.IsTrue(StoryRules.IsAscent(3, 4));
            Assert.IsFalse(StoryRules.IsAscent(5, 0));
            Assert.IsFalse(StoryRules.IsAscent(2, 2));
        }

        [Test]
        public void LanternWaitsThenClimbsInHeldFramesAndArrives()
        {
            Assert.AreEqual(0f, StoryRules.LanternProgress(0.3f, 0.5f, 1.5f, 12f), 0.0001f);
            var mid = StoryRules.LanternProgress(1.25f, 0.5f, 1.5f, 12f);
            Assert.That(mid, Is.GreaterThan(0.3f).And.LessThan(0.7f));
            Assert.AreEqual(1f, StoryRules.LanternProgress(3f, 0.5f, 1.5f, 12f), 0.0001f);
            Assert.AreEqual(StoryRules.LanternProgress(1.0f, 0.5f, 1.5f, 12f), StoryRules.LanternProgress(1.04f, 0.5f, 1.5f, 12f), 0.0001f);
        }

        [Test]
        public void LanternPositionBlendsBetweenWindows() =>
            Assert.AreEqual(new Vector2(10f, 50f), StoryRules.LanternPosition(new Vector2(10f, 100f), new Vector2(10f, 0f), 0.5f));

        [Test]
        public void WindowsGoDarkWithTrophiesAndTheNewOneLightsOnArrival()
        {
            Assert.AreEqual(WindowState.Unlit, StoryRules.StateOf(0, 0, 1, 0f));
            Assert.AreEqual(WindowState.Lit, StoryRules.StateOf(2, 2, 3, 0f));
            Assert.AreEqual(WindowState.Beaten, StoryRules.StateOf(2, 2, 3, 0.2f));
            Assert.AreEqual(WindowState.Beaten, StoryRules.StateOf(1, 2, 3, 0f));
            Assert.AreEqual(WindowState.Unlit, StoryRules.StateOf(3, 2, 3, 0.9f));
            Assert.AreEqual(WindowState.Lit, StoryRules.StateOf(3, 2, 3, 1f));
            Assert.AreEqual(WindowState.Unlit, StoryRules.StateOf(5, 2, 3, 1f));
        }

        [Test]
        public void DefaultWindowsRunFromTheBottomUp()
        {
            var windows = StoryRules.DefaultWindows(8, 160f, 480f);
            Assert.AreEqual(9, windows.Length);
            Assert.Greater(windows[0].y, windows[8].y);
            Assert.AreEqual(80f, windows[4].x);
            Assert.AreEqual(windows[2], StoryRules.WindowCentre(null, 2, 8, 160f, 480f));
            Assert.AreEqual(new Vector2(1f, 2f), StoryRules.WindowCentre(new[] { Vector2.zero, Vector2.zero, new Vector2(1f, 2f) }, 2, 8, 160f, 480f));
        }

        [Test]
        public void WindowFileParsesPlainPairsAndFloorPrefixedLines()
        {
            var parsed = StoryRules.ParseWindows("# hub first\n80 450\n\n1, 80, 400\n2;60;350.5\nnonsense\n");
            Assert.AreEqual(3, parsed.Length);
            Assert.AreEqual(new Vector2(80f, 450f), parsed[0]);
            Assert.AreEqual(new Vector2(80f, 400f), parsed[1]);
            Assert.AreEqual(new Vector2(60f, 350.5f), parsed[2]);
            Assert.AreEqual(0, StoryRules.ParseWindows(null).Length);
        }

        [Test]
        public void OpeningStepsThroughCardsAndEnds()
        {
            const float each = 2f;
            var first = StoryRules.OpeningProgress(0f, 5, 0.3f, 1.4f, 0.3f, 12f);
            Assert.AreEqual(0, first.card);
            Assert.IsFalse(first.done);
            var held = StoryRules.OpeningProgress(each + 1f, 5, 0.3f, 1.4f, 0.3f, 12f);
            Assert.AreEqual(1, held.card);
            Assert.AreEqual(1f, held.enter, 0.0001f);
            Assert.AreEqual(4, StoryRules.OpeningProgress(each * 4.5f, 5, 0.3f, 1.4f, 0.3f, 12f).card);
            Assert.IsTrue(StoryRules.OpeningProgress(each * 5f, 5, 0.3f, 1.4f, 0.3f, 12f).done);
            Assert.IsTrue(StoryRules.OpeningProgress(0f, 0, 0.3f, 1.4f, 0.3f, 12f).done);
        }

        [Test]
        public void SkipLeavesTheCardThenMovesToTheNext()
        {
            Assert.AreEqual(1.7f, StoryRules.SkipTime(0.5f, 5, 0.3f, 1.4f, 0.3f), 0.0001f);
            Assert.AreEqual(2f, StoryRules.SkipTime(1.8f, 5, 0.3f, 1.4f, 0.3f), 0.0001f);
            Assert.AreEqual(10f, StoryRules.SkipTime(9.9f, 5, 0.3f, 1.4f, 0.3f), 0.0001f);
        }
    }
}
