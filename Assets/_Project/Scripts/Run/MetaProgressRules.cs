using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Pure rules for meta-progression milestones and unlocks (docs/GDD.md, docs/PROMPTS.md 7.2).
    /// Evaluates ability, hat, and boss alternate pool unlocks based on run results and floors reached.
    /// Never includes permanent stat boosts. Pure logic, tested in EditMode.
    /// </summary>
    public static class MetaProgressRules
    {
        public static readonly string[] BaseAbilities = { "GrappleHook", "BlackHole", "ArrowVolley", "StoneWall", "Bomb", "Gust" };
        public static readonly string[] BaseHats = { "Plume", "Horns", "Crown", "WizardHat" };
        public static readonly string[] BaseBossPools = { "Standard" };

        /// <summary>Creates a default <see cref="MetaProgressData"/> with starter unlocks.</summary>
        public static MetaProgressData CreateDefault()
        {
            var data = new MetaProgressData
            {
                HighestFloorReached = 1,
                TotalRuns = 0,
                TotalVictories = 0
            };

            foreach (var ability in BaseAbilities) UnlockItem(data.UnlockedAbilities, ability);
            foreach (var hat in BaseHats) UnlockItem(data.UnlockedHats, hat);
            foreach (var pool in BaseBossPools) UnlockItem(data.UnlockedBossPools, pool);

            return data;
        }

        /// <summary>
        /// Records a run's result into <paramref name="data"/>, advancing stats and awarding milestone unlocks.
        /// Outputs any newly earned unlock names into <paramref name="newUnlocks"/>.
        /// </summary>
        public static void RecordRunResult(MetaProgressData data, RunResultData result, out List<string> newUnlocks)
        {
            newUnlocks = new List<string>();
            if (data == null) return;

            data.TotalRuns++;
            if (result.IsVictory) data.TotalVictories++;

            var floor = Mathf.Max(1, result.FloorReached);
            if (floor > data.HighestFloorReached) data.HighestFloorReached = floor;

            EvaluateMilestones(data, newUnlocks);
        }

        /// <summary>Evaluates floor-based and victory-based unlock milestones.</summary>
        public static void EvaluateMilestones(MetaProgressData data, List<string> newUnlocks = null)
        {
            if (data == null) return;

            var floor = data.HighestFloorReached;
            var victories = data.TotalVictories;

            // Floor 2 unlocks
            if (floor >= 2)
            {
                TryUnlock(data.UnlockedAbilities, "Blink", newUnlocks);
                TryUnlock(data.UnlockedHats, "Bandana", newUnlocks);
            }

            // Floor 3 unlocks
            if (floor >= 3)
            {
                TryUnlock(data.UnlockedAbilities, "BouncePad", newUnlocks);
                TryUnlock(data.UnlockedHats, "Viking", newUnlocks);
            }

            // Floor 4 unlocks
            if (floor >= 4)
            {
                TryUnlock(data.UnlockedAbilities, "Meteor", newUnlocks);
                TryUnlock(data.UnlockedHats, "Halo", newUnlocks);
                TryUnlock(data.UnlockedBossPools, "StormWyrm_Alternate", newUnlocks);
            }

            // Floor 5 unlocks
            if (floor >= 5)
            {
                TryUnlock(data.UnlockedAbilities, "Decoy", newUnlocks);
                TryUnlock(data.UnlockedHats, "Beret", newUnlocks);
            }

            // Floor 6 unlocks
            if (floor >= 6)
            {
                TryUnlock(data.UnlockedAbilities, "ShrinkRay", newUnlocks);
                TryUnlock(data.UnlockedHats, "Chef", newUnlocks);
                TryUnlock(data.UnlockedBossPools, "Clockwork_Overclocked", newUnlocks);
            }

            // Floor 7 unlocks
            if (floor >= 7)
            {
                TryUnlock(data.UnlockedAbilities, "SpikeTrap", newUnlocks);
                TryUnlock(data.UnlockedHats, "Spartan", newUnlocks);
            }

            // Victory (Floor 8 Cleared) unlocks
            if (victories >= 1)
            {
                TryUnlock(data.UnlockedAbilities, "CometDash", newUnlocks);
                TryUnlock(data.UnlockedHats, "GoldenCrown", newUnlocks);
                TryUnlock(data.UnlockedBossPools, "CrownedHollow_Enraged", newUnlocks);
            }

            // Repeat victory milestones
            if (victories >= 2)
            {
                TryUnlock(data.UnlockedAbilities, "BladeRing", newUnlocks);
                TryUnlock(data.UnlockedHats, "ReaperMask", newUnlocks);
            }
        }

        public static bool IsAbilityUnlocked(MetaProgressData data, string abilityName) =>
            data != null && data.UnlockedAbilities.Contains(abilityName);

        public static bool IsHatUnlocked(MetaProgressData data, string hatName) =>
            data != null && data.UnlockedHats.Contains(hatName);

        public static bool IsBossPoolUnlocked(MetaProgressData data, string poolName) =>
            data != null && data.UnlockedBossPools.Contains(poolName);

        public static string ToJson(MetaProgressData data, bool prettyPrint = true) =>
            data != null ? JsonUtility.ToJson(data, prettyPrint) : string.Empty;

        public static MetaProgressData FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return CreateDefault();
            try
            {
                var data = JsonUtility.FromJson<MetaProgressData>(json);
                return data ?? CreateDefault();
            }
            catch
            {
                return CreateDefault();
            }
        }

        private static bool TryUnlock(List<string> list, string item, List<string> newUnlocks)
        {
            if (list == null || list.Contains(item)) return false;
            list.Add(item);
            newUnlocks?.Add(item);
            return true;
        }

        private static void UnlockItem(List<string> list, string item)
        {
            if (list != null && !list.Contains(item)) list.Add(item);
        }
    }
}
