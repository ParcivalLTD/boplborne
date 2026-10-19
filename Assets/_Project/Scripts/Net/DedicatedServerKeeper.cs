using System;
using System.IO;
using GraceRun.Run;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Net
{
    /// <summary>
    /// Keeps a dedicated (server-only) build healthy when nobody is watching: caps the frame rate,
    /// drops an abandoned run and returns to the hub after <see cref="ServerData.IdleResetSeconds"/>
    /// with nobody connected, logs a status line now and then, and can write a small status file
    /// (<c>-statusfile</c>) that a monitor can read. Added by <see cref="ConnectionManager"/> only
    /// when it starts a server with no local player.
    /// </summary>
    public class DedicatedServerKeeper : MonoBehaviour
    {
        [Serializable]
        private struct StatusFile
        {
            public int Players;
            public int MaxPlayers;
            public string Phase;
            public int Floor;
            public int UptimeSeconds;
            public string Updated;
        }

        private ServerData _data;
        private int _maxPlayers;
        private string _statusPath;
        private float _emptySince = -1f;
        private float _nextLog;
        private float _nextFile;

        /// <summary>Applies the server settings; call once, right after adding the component.</summary>
        public void Init(ServerData data, int maxPlayers, string statusPath)
        {
            _data = data;
            _maxPlayers = maxPlayers;
            _statusPath = statusPath;
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = data != null ? data.TargetFrameRate : 60;
            _nextLog = data != null && data.StatusLogSeconds > 0f ? data.StatusLogSeconds : float.MaxValue;
            _nextFile = 0f;
            Debug.Log($"[Server] Dedicated server started, {Application.targetFrameRate} fps cap, idle reset after {(data != null ? data.IdleResetSeconds : 30f):0}s.");
        }

        private void Update()
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsServer || net.IsHost || _data == null) return;

            var players = net.ConnectedClientsIds.Count;
            ResetWhenAbandoned(players);
            if (Time.realtimeSinceStartup >= _nextLog) LogStatus(players);
            if (!string.IsNullOrEmpty(_statusPath) && Time.realtimeSinceStartup >= _nextFile) WriteStatusFile(players);
        }

        private void ResetWhenAbandoned(int players)
        {
            var run = RunManager.Instance;
            var fight = FightLoop.Instance;
            if (run == null || !run.IsSpawned || fight == null || !fight.IsSpawned) return;

            if (players > 0)
            {
                _emptySince = -1f;
                return;
            }
            if (_emptySince < 0f) _emptySince = Time.realtimeSinceStartup;

            var empty = Time.realtimeSinceStartup - _emptySince;
            var clean = run.InHub && fight.Phase == FightPhase.Waiting;
            if (!ServerRules.ShouldResetToHub(players, empty, _data.IdleResetSeconds, clean)) return;

            Debug.Log($"[Server] Nobody connected for {empty:0}s; dropping the unfinished run and returning to the hub.");
            run.ServerEnterHub();
            _emptySince = Time.realtimeSinceStartup;
        }

        private void LogStatus(int players)
        {
            _nextLog = Time.realtimeSinceStartup + _data.StatusLogSeconds;
            Debug.Log(ServerRules.FormatStatus(players, _maxPlayers, CurrentPhase(), CurrentFloor(), Time.realtimeSinceStartup));
        }

        private void WriteStatusFile(int players)
        {
            _nextFile = Time.realtimeSinceStartup + Mathf.Max(1f, _data.StatusFileSeconds);
            var status = new StatusFile
            {
                Players = players,
                MaxPlayers = _maxPlayers,
                Phase = CurrentPhase(),
                Floor = CurrentFloor(),
                UptimeSeconds = (int)Time.realtimeSinceStartup,
                Updated = DateTime.UtcNow.ToString("o"),
            };
            try
            {
                File.WriteAllText(_statusPath, JsonUtility.ToJson(status));
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning($"[Server] Could not write the status file '{_statusPath}': {e.Message}");
                _statusPath = null;
            }
        }

        private static string CurrentPhase() => FightLoop.Instance != null && FightLoop.Instance.IsSpawned ? FightLoop.Instance.Phase.ToString() : "Starting";

        private static int CurrentFloor() => RunManager.Instance != null && RunManager.Instance.IsSpawned ? RunManager.Instance.Floor : 0;
    }
}
