using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GraceRun.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GraceRun.Editor
{
    /// <summary>
    /// Editor build tools (docs/ARCHITECTURE.md, docs/PROMPTS.md 6.5 / 9.1):
    /// builds the client to Builds/Windows with the version from GameConfig in the folder name,
    /// and can package a single-file executable into Builds/Release.
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
        /// CLI entrypoint for CI/CD batchmode builds (-executeMethod GraceRun.Editor.BuildMenu.BuildWindowsCli).
        /// Exits with code 0 on success or 1 on failure.
        /// </summary>
        public static void BuildWindowsCli()
        {
            var report = BuildWindows();
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        /// <summary>
        /// Menu item "Grace Run → Build Windows": builds the client to Builds/Windows with
        /// the version from GameConfig in the folder name.
        /// </summary>
        [MenuItem("Grace Run/Build Windows")]
        public static BuildReport BuildWindows()
        {
            var version = ReadGameVersion();
            // The exe's file properties (Details tab) show this version.
            PlayerSettings.bundleVersion = version;
            var folderName = $"Boplborne_v{version}";
            var outputDir = Path.Combine(BuildFolderRoot, folderName);
            Directory.CreateDirectory(outputDir);
            var exePath = Path.Combine(outputDir, "Boplborne.exe");

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

        /// <summary>
        /// Menu item "Grace Run → Build Windows (Single EXE)": builds the Windows client and packages it
        /// into a single self-extracting executable at Builds/Release/Boplborne_v{version}_SingleFile.exe.
        /// </summary>
        [MenuItem("Grace Run/Build Windows (Single EXE)")]
        public static void BuildWindowsSingleExe()
        {
            var report = BuildWindows();
            if (report.summary.result != BuildResult.Succeeded) return;

            var version = ReadGameVersion();
            var scriptPath = Path.GetFullPath(".github/scripts/Pack-SingleExe.ps1");
            if (!File.Exists(scriptPath))
            {
                Debug.LogError($"[BuildMenu] Packaging script not found at '{scriptPath}'.");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" -Version \"{version}\"",
                WorkingDirectory = Path.GetFullPath("."),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return;
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode == 0)
            {
                Debug.Log($"[BuildMenu] Single-EXE packaged successfully:\n{stdout.Trim()}");
                EditorUtility.RevealInFinder(Path.GetFullPath($"Builds/Release/Boplborne_v{version}_SingleFile.exe"));
            }
            else
            {
                Debug.LogError($"[BuildMenu] Single-EXE packaging failed (exit {proc.ExitCode}):\n{stderr}\n{stdout}");
            }
        }

        private static string ReadGameVersion()
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

            return config != null && !string.IsNullOrWhiteSpace(config.GameVersion)
                ? config.GameVersion.Trim()
                : DefaultVersion;
        }
    }
}
