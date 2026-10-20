using System.Collections.Generic;
using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class MetaProgressRulesTests
    {
        [Test]
        public void CreateDefaultReturnsExpectedBaseUnlocks()
        {
            var data = MetaProgressRules.CreateDefault();

            Assert.AreEqual(1, data.HighestFloorReached);
            Assert.AreEqual(0, data.TotalRuns);
            Assert.AreEqual(0, data.TotalVictories);

            foreach (var ability in MetaProgressRules.BaseAbilities)
            {
                Assert.IsTrue(MetaProgressRules.IsAbilityUnlocked(data, ability));
            }

            Assert.IsFalse(MetaProgressRules.IsAbilityUnlocked(data, "Blink"));
            Assert.IsFalse(MetaProgressRules.IsAbilityUnlocked(data, "Meteor"));

            Assert.IsTrue(MetaProgressRules.IsHatUnlocked(data, "Plume"));
            Assert.IsFalse(MetaProgressRules.IsHatUnlocked(data, "Halo"));

            Assert.IsTrue(MetaProgressRules.IsBossPoolUnlocked(data, "Standard"));
            Assert.IsFalse(MetaProgressRules.IsBossPoolUnlocked(data, "StormWyrm_Alternate"));
        }

        [Test]
        public void DefeatOnFloor3AwardsFloor2And3Milestones()
        {
            var data = MetaProgressRules.CreateDefault();
            var result = new RunResultData
            {
                IsValid = true,
                IsVictory = false,
                FloorReached = 3,
                TotalFloors = 8
            };

            MetaProgressRules.RecordRunResult(data, result, out var newUnlocks);

            Assert.AreEqual(1, data.TotalRuns);
            Assert.AreEqual(0, data.TotalVictories);
            Assert.AreEqual(3, data.HighestFloorReached);

            Assert.IsTrue(MetaProgressRules.IsAbilityUnlocked(data, "Blink"));
            Assert.IsTrue(MetaProgressRules.IsAbilityUnlocked(data, "BouncePad"));
            Assert.IsTrue(MetaProgressRules.IsHatUnlocked(data, "Bandana"));
            Assert.IsTrue(MetaProgressRules.IsHatUnlocked(data, "Viking"));

            Assert.IsFalse(MetaProgressRules.IsAbilityUnlocked(data, "Meteor"));
            Assert.Contains("Blink", newUnlocks);
            Assert.Contains("BouncePad", newUnlocks);
            Assert.Contains("Bandana", newUnlocks);
            Assert.Contains("Viking", newUnlocks);
        }

        [Test]
        public void VictoryAwardsAllMilestonesAndVictoryUnlocks()
        {
            var data = MetaProgressRules.CreateDefault();
            var result = new RunResultData
            {
                IsValid = true,
                IsVictory = true,
                FloorReached = 8,
                TotalFloors = 8
            };

            MetaProgressRules.RecordRunResult(data, result, out var newUnlocks);

            Assert.AreEqual(1, data.TotalRuns);
            Assert.AreEqual(1, data.TotalVictories);
            Assert.AreEqual(8, data.HighestFloorReached);

            Assert.IsTrue(MetaProgressRules.IsAbilityUnlocked(data, "CometDash"));
            Assert.IsTrue(MetaProgressRules.IsHatUnlocked(data, "GoldenCrown"));
            Assert.IsTrue(MetaProgressRules.IsBossPoolUnlocked(data, "CrownedHollow_Enraged"));
            Assert.IsTrue(MetaProgressRules.IsBossPoolUnlocked(data, "StormWyrm_Alternate"));
            Assert.IsTrue(MetaProgressRules.IsBossPoolUnlocked(data, "Clockwork_Overclocked"));

            Assert.Contains("CometDash", newUnlocks);
            Assert.Contains("GoldenCrown", newUnlocks);
            Assert.Contains("CrownedHollow_Enraged", newUnlocks);
        }

        [Test]
        public void SubsequentRunsDoNotDuplicateUnlocks()
        {
            var data = MetaProgressRules.CreateDefault();
            var run1 = new RunResultData { IsValid = true, FloorReached = 3, TotalFloors = 8 };
            var run2 = new RunResultData { IsValid = true, FloorReached = 3, TotalFloors = 8 };

            MetaProgressRules.RecordRunResult(data, run1, out var unlocks1);
            Assert.IsTrue(unlocks1.Count > 0);

            MetaProgressRules.RecordRunResult(data, run2, out var unlocks2);
            Assert.AreEqual(0, unlocks2.Count);
            Assert.AreEqual(2, data.TotalRuns);
        }

        [Test]
        public void JsonSerializationRoundtripPreservesData()
        {
            var original = MetaProgressRules.CreateDefault();
            original.HighestFloorReached = 6;
            original.TotalRuns = 15;
            original.TotalVictories = 3;
            original.UnlockedAbilities.Add("Meteor");
            original.UnlockedHats.Add("Halo");
            original.UnlockedBossPools.Add("StormWyrm_Alternate");

            var json = MetaProgressRules.ToJson(original);
            var loaded = MetaProgressRules.FromJson(json);

            Assert.IsNotNull(loaded);
            Assert.AreEqual(original.HighestFloorReached, loaded.HighestFloorReached);
            Assert.AreEqual(original.TotalRuns, loaded.TotalRuns);
            Assert.AreEqual(original.TotalVictories, loaded.TotalVictories);
            Assert.IsTrue(loaded.UnlockedAbilities.Contains("Meteor"));
            Assert.IsTrue(loaded.UnlockedHats.Contains("Halo"));
            Assert.IsTrue(loaded.UnlockedBossPools.Contains("StormWyrm_Alternate"));
        }

        [Test]
        public void FromJsonHandlesEmptyOrInvalidGracefully()
        {
            var fromNull = MetaProgressRules.FromJson(null);
            Assert.IsNotNull(fromNull);
            Assert.AreEqual(1, fromNull.HighestFloorReached);

            var fromGarbage = MetaProgressRules.FromJson("{ invalid json: [[ ]");
            Assert.IsNotNull(fromGarbage);
            Assert.AreEqual(1, fromGarbage.HighestFloorReached);
        }
    }
}
