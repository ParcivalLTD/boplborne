using System;
using System.IO;
using System.Linq;
using GraceRun.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GraceRun.Editor
{
    /// <summary>
    /// Editor build tools (docs/ARCHITECTURE.md, docs/PROMPTS.md 6.5):
    /// builds the client to Builds/Windows with the version from GameConfig in the folder name.
    /// Accessible via the editor menu "Grace Run → Build Windows".
    /// </summary>
    [InitializeOnLoad]
    public static class BuildMenu
    {
        public const string DefaultVersion = "0.1.0";
        public const string BuildFolderRoot = "Builds/Windows";
        private const string TriggerFile = "Temp/trigger_build.flag";
        private const string StatusFile = "Temp/build_status.txt";

        static BuildMenu()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (!File.Exists(TriggerFile)) return;
            try { File.Delete(TriggerFile); } catch { }

            try
            {
                File.WriteAllText(StatusFile, "BUILDING");
                var report = BuildWindows();
                var success = report.summary.result == BuildResult.Succeeded;
                File.WriteAllText(StatusFile, success ? $"SUCCESS:{report.summary.outputPath}" : $"FAILED:{report.summary.totalErrors}");
            }
            catch (Exception ex)
            {
                File.WriteAllText(StatusFile, $"ERROR:{ex.Message}");
            }
        }

        /// <summary>
        /// Menu item "Grace Run → Build Windows": builds the client to Builds/Windows with
        /// the version from GameConfig in the folder name.
        /// </summary>
        [MenuItem("Grace Run/Build Windows")]
        public static BuildReport BuildWindows()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/Data/Config/GameConfig.asset");
            if (config == null)
            {
                var guids = AssetDatabase.FindAssets("t:GameConfig");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    config = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
                }
            }

            var version = config != null && !string.IsNullOrWhiteSpace(config.GameVersion)
                ? config.GameVersion.Trim()
                : DefaultVersion;

            var folderName = $"GraceRun_v{version}";
            var outputDir = Path.Combine(BuildFolderRoot, folderName);
            Directory.CreateDirectory(outputDir);
            var exePath = Path.Combine(outputDir, "GraceRun.exe");

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                scenes = new[]
                {
                    "Assets/_Project/Scenes/Bootstrap.unity",
                    "Assets/_Project/Scenes/MainMenu.unity",
                    "Assets/_Project/Scenes/Game.unity"
                };
            }

            Debug.Log($"[BuildMenu] Building StandaloneWindows64 client to '{exePath}' (version {version}) with {scenes.Length} scenes...");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildMenu] Build SUCCEEDED: '{exePath}' ({summary.totalSize} bytes in {summary.totalTime.TotalSeconds:F1}s)");
            }
            else
            {
                Debug.LogError($"[BuildMenu] Build FAILED with result {summary.result} ({summary.totalErrors} errors).");
            }

            return report;
        }
    }
}
