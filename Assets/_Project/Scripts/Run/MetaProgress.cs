using System;
using System.Collections.Generic;
using System.IO;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Runtime manager for local player meta-progression (docs/GDD.md, docs/ARCHITECTURE.md).
    /// Loads from and saves to local JSON via <see cref="SaveSystem"/>.
    /// In batchmode (dedicated server), saving and tracking are bypassed.
    /// </summary>
    public static class MetaProgress
    {
        private const string SaveFileName = "save_metaprogress.json";
        private static MetaProgressData _current;

        /// <summary>Raised whenever a run awards new unlocks.</summary>
        public static event Action<MetaProgressData, IReadOnlyList<string>> UnlocksEarned;

        /// <summary>Raised after meta-progression is saved to disk.</summary>
        public static event Action<MetaProgressData> ProgressSaved;

        /// <summary>The active local player meta progression data.</summary>
        public static MetaProgressData Current
        {
            get
            {
                if (_current == null) EnsureLoaded();
                return _current;
            }
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureLoaded()
        {
            if (_current != null) return;

            if (Application.isBatchMode)
            {
                _current = MetaProgressRules.CreateDefault();
                return;
            }

            if (SaveSystem.TryLoadJson<MetaProgressData>(SavePath, out var loaded) && loaded != null)
            {
                _current = loaded;
                MetaProgressRules.EvaluateMilestones(_current);
            }
            else
            {
                _current = MetaProgressRules.CreateDefault();
                Save();
            }
        }

        /// <summary>
        /// Records a completed run's stats, unlocks any earned milestones, and writes to disk.
        /// Returns the list of newly unlocked items (empty if none).
        /// </summary>
        public static IReadOnlyList<string> RecordRun(RunResultData result)
        {
            if (Application.isBatchMode) return Array.Empty<string>();

            EnsureLoaded();
            MetaProgressRules.RecordRunResult(_current, result, out var newUnlocks);
            Save();

            if (newUnlocks.Count > 0)
            {
                UnlocksEarned?.Invoke(_current, newUnlocks);
            }

            return newUnlocks;
        }

        public static bool IsAbilityUnlocked(string abilityName) =>
            MetaProgressRules.IsAbilityUnlocked(Current, abilityName);

        public static bool IsHatUnlocked(string hatName) =>
            MetaProgressRules.IsHatUnlocked(Current, hatName);

        public static bool IsBossPoolUnlocked(string poolName) =>
            MetaProgressRules.IsBossPoolUnlocked(Current, poolName);

        public static void Save()
        {
            if (Application.isBatchMode || _current == null) return;
            SaveSystem.SaveJson(SavePath, _current);
            ProgressSaved?.Invoke(_current);
        }

        /// <summary>Resets local meta-progression back to default and saves.</summary>
        public static void ResetToDefault()
        {
            _current = MetaProgressRules.CreateDefault();
            Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _current = null;
            UnlocksEarned = null;
            ProgressSaved = null;
        }
    }
}
