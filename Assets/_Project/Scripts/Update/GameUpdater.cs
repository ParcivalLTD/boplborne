using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GraceRun.Core;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace GraceRun.Update
{
    /// <summary>What the updater is doing; the update window reads this.</summary>
    public enum UpdateState { Idle, Checking, Downloading, Installing, Restarting, Failed }

    /// <summary>
    /// Self-updater. At start-up it asks GitHub for the latest release; when that is newer than this build it
    /// downloads the portable zip, unpacks it beside the old version and restarts into it, all inside the game
    /// window (see <see cref="UpdateScreen"/>). A failed check stays silent; a failed download shows a message and lets the game start.
    /// Skipped in the Editor, on servers, off Windows and with <c>-noupdate</c>.
    /// </summary>
    public class GameUpdater : MonoBehaviour
    {
        private const string NoUpdateFlag = "-noupdate";

        private GameConfig _config;
        private UpdateScreen _screen;
        private volatile float _unpack;
        private bool _skipped;

        public UpdateState State { get; private set; } = UpdateState.Idle;
        public string TargetVersion { get; private set; }
        public float Progress { get; private set; }
        public string Detail { get; private set; } = string.Empty;

        /// <summary>The user chose to play on the old version for now (only while downloading).</summary>
        public void Skip() => _skipped = true;

        /// <summary>Starts the check unless updating makes no sense here.</summary>
        public static void CheckOnce(GameConfig config)
        {
            if (config == null || Application.isEditor || Application.isBatchMode ||
                Application.platform != RuntimePlatform.WindowsPlayer || string.IsNullOrEmpty(config.UpdateRepository) ||
                Array.IndexOf(Environment.GetCommandLineArgs(), NoUpdateFlag) >= 0) return;
            var go = new GameObject("GameUpdater");
            DontDestroyOnLoad(go);
            go.AddComponent<GameUpdater>().Begin(config);
        }

        private void Begin(GameConfig config)
        {
            _config = config;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            State = UpdateState.Checking;
            var running = Directory.GetParent(Application.dataPath)?.FullName;
            var url = $"https://api.github.com/repos/{_config.UpdateRepository}/releases/latest";
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("User-Agent", "Boplborne-updater");
                request.SetRequestHeader("Accept", "application/vnd.github+json");
                request.timeout = Mathf.CeilToInt(_config.UpdateCheckTimeoutSeconds);
                yield return request.SendWebRequest();
                var release = request.result == UnityWebRequest.Result.Success ? UpdateRules.ParseRelease(request.downloadHandler.text) : null;
                var asset = UpdateRules.PickAsset(release);
                if (release == null || asset == null || !UpdateRules.IsNewer(_config.GameVersion, release.tag_name))
                {
                    if (UpdateRules.TryParseVersion(_config.GameVersion, out var current))
                        Task.Run(() => UpdateInstaller.RemoveOlderThan(current, running));
                    Finish();
                    yield break;
                }

                UpdateRules.TryParseVersion(release.tag_name, out var latest);
                TargetVersion = latest.ToString();
                _screen = UpdateScreen.Create(this);
                yield return Apply(latest, asset);
            }
        }

        private IEnumerator Apply(Version latest, ReleaseAsset asset)
        {
            string exe = Path.Combine(UpdateInstaller.FolderFor(latest), UpdateInstaller.ExeName);
            if (!UpdateInstaller.IsInstalled(latest))
            {
                Directory.CreateDirectory(UpdateInstaller.DownloadFolder);
                var zip = Path.Combine(UpdateInstaller.DownloadFolder, $"{UpdateRules.RuntimeFolder(latest)}.zip");
                State = UpdateState.Downloading;
                using (var download = new UnityWebRequest(asset.browser_download_url, UnityWebRequest.kHttpVerbGET))
                {
                    download.downloadHandler = new DownloadHandlerFile(zip) { removeFileOnAbort = true };
                    download.SetRequestHeader("User-Agent", "Boplborne-updater");
                    download.SendWebRequest();
                    while (!download.isDone)
                    {
                        if (_skipped) { download.Abort(); Finish(); yield break; }
                        var downloaded = (long)download.downloadedBytes;
                        var share = _config.UpdateDownloadShare;
                        Progress = UpdateRules.OverallProgress(asset.size > 0 ? downloaded / (float)asset.size : 0f, 0f, share);
                        Detail = UpdateRules.ProgressText(downloaded, asset.size);
                        yield return null;
                    }

                    if (download.result != UnityWebRequest.Result.Success) { Fail($"Download failed: {download.error}"); yield break; }
                }

                State = UpdateState.Installing;
                Detail = "Unpacking";
                var sha = UpdateRules.ExpectedSha256(asset.digest);
                var task = Task.Run(() => UpdateInstaller.Install(zip, latest, sha, asset.size, p => _unpack = p));
                while (!task.IsCompleted)
                {
                    Progress = UpdateRules.OverallProgress(1f, _unpack, _config.UpdateDownloadShare);
                    yield return null;
                }

                if (task.IsFaulted) { Fail(task.Exception?.GetBaseException().Message ?? "Install failed."); yield break; }
            }

            Restart(exe);
        }

        private void Restart(string exe)
        {
            State = UpdateState.Restarting;
            Progress = 1f;
            Detail = "Restarting";
            try
            {
                var args = Environment.GetCommandLineArgs();
                var info = new ProcessStartInfo(exe) { WorkingDirectory = Path.GetDirectoryName(exe), UseShellExecute = false };
                for (var i = 1; i < args.Length; i++) info.ArgumentList.Add(args[i]);
                Process.Start(info);
                Application.Quit();
            }
            catch (Exception ex)
            {
                Fail($"Could not restart: {ex.Message}");
            }
        }

        private void Fail(string message)
        {
            Debug.LogWarning($"[Updater] {message}");
            State = UpdateState.Failed;
            Detail = message;
            StartCoroutine(FinishLater(4f));
        }

        private IEnumerator FinishLater(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            Finish();
        }

        private void Finish()
        {
            State = UpdateState.Idle;
            if (_screen != null) _screen.Close();
            Destroy(gameObject, 1f);
        }
    }
}
