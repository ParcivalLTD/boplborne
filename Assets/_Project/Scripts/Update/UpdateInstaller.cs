using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace GraceRun.Update
{
    /// <summary>
    /// The disk side of the self-updater: verifies a downloaded zip, unpacks it next to the older versions
    /// (<c>%LOCALAPPDATA%\Boplborne\Runtime_&lt;version&gt;</c>, the same place the single-file launcher uses) and removes old ones.
    /// Blocking; call it from a worker thread.
    /// </summary>
    public static class UpdateInstaller
    {
        public const string ExeName = "Boplborne.exe";

        /// <summary>Where every installed version lives.</summary>
        public static string InstallRoot =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Boplborne");

        /// <summary>Folder for the zip being downloaded.</summary>
        public static string DownloadFolder => Path.Combine(InstallRoot, "Downloads");

        public static string FolderFor(Version version) => Path.Combine(InstallRoot, UpdateRules.RuntimeFolder(version));

        /// <summary>True when <paramref name="version"/> is fully installed.</summary>
        public static bool IsInstalled(Version version) =>
            File.Exists(Path.Combine(FolderFor(version), UpdateRules.ReadyMarker)) &&
            File.Exists(Path.Combine(FolderFor(version), ExeName));

        /// <summary>Checks the zip, unpacks it and marks the version ready. Returns the new exe path.</summary>
        /// <param name="progress">Called with 0..1 while unpacking.</param>
        public static string Install(string zipPath, Version version, string expectedSha256, long expectedSize, Action<float> progress)
        {
            if (expectedSize > 0 && new FileInfo(zipPath).Length != expectedSize)
                throw new IOException("The download is incomplete.");
            if (!string.IsNullOrEmpty(expectedSha256) && !string.Equals(Sha256Of(zipPath), expectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new IOException("The download is damaged (checksum mismatch).");

            var target = FolderFor(version);
            var staging = target + ".tmp";
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            Directory.CreateDirectory(staging);

            using (var archive = ZipFile.OpenRead(zipPath))
            {
                var done = 0;
                foreach (var entry in archive.Entries)
                {
                    var destination = Path.Combine(staging, entry.FullName);
                    if (!UpdateRules.IsInside(staging, destination)) throw new IOException("The download has an unsafe path.");
                    if (string.IsNullOrEmpty(entry.Name)) Directory.CreateDirectory(destination);
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destination));
                        entry.ExtractToFile(destination, true);
                    }

                    progress?.Invoke(++done / (float)archive.Entries.Count);
                }
            }

            if (!File.Exists(Path.Combine(staging, ExeName))) throw new IOException("The download has no game inside.");
            File.WriteAllText(Path.Combine(staging, UpdateRules.ReadyMarker), version.ToString());
            if (Directory.Exists(target)) Directory.Delete(target, true);
            Directory.Move(staging, target);
            File.Delete(zipPath);
            return Path.Combine(target, ExeName);
        }

        /// <summary>Best-effort removal of installed versions older than <paramref name="keep"/>, never the running one.</summary>
        public static void RemoveOlderThan(Version keep, string runningFolder)
        {
            try
            {
                if (!Directory.Exists(InstallRoot)) return;
                var running = string.IsNullOrEmpty(runningFolder) ? null : Path.GetFullPath(runningFolder).TrimEnd('\\', '/');
                foreach (var folder in Directory.GetDirectories(InstallRoot))
                {
                    if (!UpdateRules.TryParseRuntimeFolder(Path.GetFileName(folder), out var version) || version >= keep) continue;
                    if (string.Equals(Path.GetFullPath(folder).TrimEnd('\\', '/'), running, StringComparison.OrdinalIgnoreCase)) continue;
                    try { Directory.Delete(folder, true); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (Exception)
            {
                // Cleanup is a nicety; a locked folder is tried again next launch.
            }
        }

        private static string Sha256Of(string path)
        {
            using var stream = File.OpenRead(path);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
