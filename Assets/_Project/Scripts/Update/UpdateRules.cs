using System;
using UnityEngine;

namespace GraceRun.Update
{
    /// <summary>Pure rules for the self-updater: version compare, which asset to download, folder names, progress text.</summary>
    public static class UpdateRules
    {
        public const string RuntimeFolderPrefix = "Runtime_";
        public const string PortableSuffix = "_Portable.zip";
        public const string ReadyMarker = ".ready";
        private const string Sha256Prefix = "sha256:";

        /// <summary>Parses "v0.1.57" or "0.1.57" into a <see cref="Version"/>; false when it is not a version.</summary>
        public static bool TryParseVersion(string text, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(text)) return false;
            var trimmed = text.Trim().TrimStart('v', 'V');
            if (!Version.TryParse(trimmed, out var parsed)) return false;
            version = parsed;
            return true;
        }

        /// <summary>True when <paramref name="latest"/> is a higher version than <paramref name="current"/>; unparsable input never updates.</summary>
        public static bool IsNewer(string current, string latest) =>
            TryParseVersion(current, out var have) && TryParseVersion(latest, out var want) && want > have;

        /// <summary>The release's portable zip, or null if the release has none.</summary>
        public static ReleaseAsset PickAsset(ReleaseInfo release)
        {
            if (release?.assets == null) return null;
            foreach (var asset in release.assets)
            {
                if (asset?.name != null && !string.IsNullOrEmpty(asset.browser_download_url) &&
                    asset.name.EndsWith(PortableSuffix, StringComparison.OrdinalIgnoreCase)) return asset;
            }

            return null;
        }

        /// <summary>Parses a GitHub release JSON body; null when it is not a release.</summary>
        public static ReleaseInfo ParseRelease(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var release = JsonUtility.FromJson<ReleaseInfo>(json);
                return release != null && !string.IsNullOrEmpty(release.tag_name) ? release : null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>Install folder name for a version, shared with the single-exe launcher.</summary>
        public static string RuntimeFolder(Version version) => RuntimeFolderPrefix + version;

        /// <summary>Version encoded in a runtime folder name, or false for any other folder.</summary>
        public static bool TryParseRuntimeFolder(string folderName, out Version version)
        {
            version = null;
            return folderName != null &&
                   folderName.StartsWith(RuntimeFolderPrefix, StringComparison.OrdinalIgnoreCase) &&
                   Version.TryParse(folderName.Substring(RuntimeFolderPrefix.Length), out version);
        }

        /// <summary>The lowercase hex hash from a GitHub "sha256:..." digest, or null if there is none.</summary>
        public static string ExpectedSha256(string digest) =>
            !string.IsNullOrEmpty(digest) && digest.StartsWith(Sha256Prefix, StringComparison.OrdinalIgnoreCase)
                ? digest.Substring(Sha256Prefix.Length).Trim().ToLowerInvariant()
                : null;

        /// <summary>False for a zip entry that would land outside <paramref name="root"/> (a "zip slip").</summary>
        public static bool IsInside(string root, string path)
        {
            var fullRoot = System.IO.Path.GetFullPath(root).TrimEnd('\\', '/') + System.IO.Path.DirectorySeparatorChar;
            return System.IO.Path.GetFullPath(path).StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>"12.3 / 85.0 MB" style text for a download.</summary>
        public static string ProgressText(long done, long total)
        {
            const double Mb = 1024d * 1024d;
            var c = System.Globalization.CultureInfo.InvariantCulture;
            return total > 0 ? string.Format(c, "{0:F1} / {1:F1} MB", done / Mb, total / Mb) : string.Format(c, "{0:F1} MB", done / Mb);
        }

        /// <summary>Overall bar fill: the download is the first 80%, unpacking the last 20%.</summary>
        public static float OverallProgress(float download01, float unpack01, float downloadShare)
        {
            var share = Mathf.Clamp01(downloadShare);
            return Mathf.Clamp01(download01) * share + Mathf.Clamp01(unpack01) * (1f - share);
        }
    }
}
