using System;
using System.Collections;
using System.Collections.Generic;
using GraceRun.Core;
using GraceRun.Core.Settings;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraceRun.Net
{
    /// <summary>
    /// Owns the connection: reads the launch options, hosts or joins by IP, approves clients,
    /// moves everyone into the Game scene and sends them back to the menu when the host leaves.
    /// Lives in Bootstrap and survives scene loads.
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        private const string HostLeftMessage = "Host left the game.";
        private const string TimeoutMessage = "Could not reach the host (timed out).";
        private const string RefusedMessage = "Connection refused.";
        private const string ListenAddress = "0.0.0.0";

        [SerializeField] private GameConfig _config;
        [SerializeField] private NetworkManager _networkManager;

        private Coroutine _timeout;
        private bool _leaving;
        private bool _starting;
        private int _attempt;
        private readonly Dictionary<ulong, string> _nicknames = new();

        public static ConnectionManager Instance { get; private set; }

        /// <summary>Why the last session ended; shown once by the main menu.</summary>
        public static string LastMessage { get; set; }

        /// <summary>Set by the fight loop on the server; while true the server refuses new players.</summary>
        public bool FightInProgress { get; set; }

        public GameConfig Config => _config;
        public bool IsBusy => _starting || (_networkManager != null && _networkManager.IsListening);

        /// <summary>The Relay code friends type to join this host, or null when hosting directly or not hosting.</summary>
        public string JoinCode { get; private set; }

        /// <summary>True while this host's room is on the public list.</summary>
        public bool IsPublic { get; private set; }

        /// <summary>Status line for the menu (connecting, refused, timed out).</summary>
        public event Action<string> StatusChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _networkManager.ConnectionApprovalCallback = ApprovalCheck;
            _networkManager.OnServerStarted += OnServerStarted;
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;

            var args = CommandLineArgs.Current();
            if (args.StartServer)
            {
                StartDedicatedServer(args.Port ?? _config.Port, args.StatusFile);
                return;
            }

            SceneManager.LoadScene(Scenes.MainMenu);
            if (args.HasConnectAddress) Join(args.ConnectAddress);
            else GraceRun.UI.Splash.SplashScreen.PlayOnce(_config);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            if (_networkManager == null) return;
            _networkManager.ConnectionApprovalCallback = null;
            _networkManager.OnServerStarted -= OnServerStarted;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        /// <summary>Server only: the cleaned, unique nickname <paramref name="clientId"/> joined with, or empty for none (shown as P1-P4).</summary>
        public string NicknameOf(ulong clientId) => _nicknames.TryGetValue(clientId, out var name) ? name : string.Empty;

        /// <summary>
        /// Starts a server that also plays. Opens an online Relay room (friends join with a code);
        /// if Relay is unreachable it hosts directly by IP (LAN / Hamachi) instead.
        /// </summary>
        public async void Host(bool isPublic = false)
        {
            if (IsBusy) return;
            var attempt = ++_attempt;
            _starting = true;
            SetStatus("Opening an online room...");

            RelayRoom? room = null;
            try
            {
                room = await RelayConnector.CreateRoomAsync(_config.MaxPlayers);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Relay unavailable, hosting directly: {e.Message}");
            }

            if (attempt != _attempt) return;
            _starting = false;
            Configure(ListenAddress, _config.Port);
            if (room.HasValue)
            {
                _networkManager.GetComponent<UnityTransport>().SetRelayServerData(room.Value.Data);
                JoinCode = room.Value.Code;
                GUIUtility.systemCopyBuffer = JoinCode;
            }

            SetStatus("Starting host...");
            if (_networkManager.StartHost())
            {
                // A public room needs a Relay code to be joinable; without one it stays unlisted.
                IsPublic = isPublic && JoinCode != null;
                if (IsPublic) PublisherComponent().Begin(RoomSnapshot, _config.RoomRefreshSeconds);
                return;
            }

            JoinCode = null;
            SetStatus("Could not start the host (port in use?).");
        }

        private RoomListing RoomSnapshot() => new(
            string.IsNullOrEmpty(GameSettings.Nickname) ? "Host" : GameSettings.Nickname, JoinCode,
            _networkManager.IsListening ? _networkManager.ConnectedClientsIds.Count : 1,
            _config.MaxPlayers, _config.GameVersion, FightInProgress);

        private PublicRoomPublisher PublisherComponent()
        {
            var publisher = GetComponent<PublicRoomPublisher>();
            return publisher != null ? publisher : gameObject.AddComponent<PublicRoomPublisher>();
        }

        /// <summary>Starts a server with no local player (command line -server).</summary>
        public void StartDedicatedServer(ushort port, string statusFile = null)
        {
            if (IsBusy) return;
            Configure(ListenAddress, port);
            if (!_networkManager.StartServer())
            {
                Debug.LogError($"Could not start the server on port {port}.");
                return;
            }
            var keeper = GetComponent<DedicatedServerKeeper>();
            if (keeper == null) keeper = gameObject.AddComponent<DedicatedServerKeeper>();
            keeper.Init(_config.Server, _config.MaxPlayers, statusFile);
            Debug.Log($"[Server] Listening on port {port}, game version {_config.GameVersion}.");
        }

        /// <summary>Joins a host typed as a Relay code ("K7M2QX"), "ip" or "ip:port".</summary>
        public void Join(string addressText)
        {
            if (IsBusy) return;
            if (JoinCodeRules.LooksLikeCode(addressText))
            {
                JoinByCode(addressText);
                return;
            }

            if (!AddressParser.TryParse(addressText, _config.Port, out var address, out var port))
            {
                SetStatus("That address does not look right.");
                return;
            }

            Configure(address, port);
            SetStatus($"Connecting to {address}:{port}...");
            if (!_networkManager.StartClient())
            {
                SetStatus("Could not start the client.");
                return;
            }

            _timeout = StartCoroutine(FailAfterTimeout());
        }

        private async void JoinByCode(string code)
        {
            var attempt = ++_attempt;
            _starting = true;
            SetStatus($"Looking for room {JoinCodeRules.Normalize(code)}...");

            Unity.Networking.Transport.Relay.RelayServerData data;
            try
            {
                data = await RelayConnector.JoinRoomAsync(code);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not join room {code}: {e.Message}");
                if (attempt != _attempt) return;
                _starting = false;
                SetStatus("Room not found. Check the code and that the host is still online.");
                return;
            }

            if (attempt != _attempt) return;
            _starting = false;
            Configure(ListenAddress, _config.Port);
            _networkManager.GetComponent<UnityTransport>().SetRelayServerData(data);
            SetStatus("Connecting...");
            if (!_networkManager.StartClient())
            {
                SetStatus("Could not start the client.");
                return;
            }

            _timeout = StartCoroutine(FailAfterTimeout());
        }

        /// <summary>Leaves the session and returns to the main menu.</summary>
        public void Leave(string message = null)
        {
            _attempt++;
            _starting = false;
            JoinCode = null;
            if (IsPublic) PublisherComponent().Stop();
            IsPublic = false;
            StopTimeout();
            FightInProgress = false;
            // Shutdown finishes a frame later and can report our own disconnect, which is not
            // "Host left the game". Ignore disconnects until the next session starts.
            _leaving = true;
            if (_networkManager.IsListening) _networkManager.Shutdown();
            LastMessage = message;
            if (SceneManager.GetActiveScene().name != Scenes.MainMenu) SceneManager.LoadScene(Scenes.MainMenu);
            else SetStatus(message);
        }

        private void Configure(string address, ushort port)
        {
            _leaving = false;
            _nicknames.Clear();
            var transport = _networkManager.GetComponent<UnityTransport>();
            transport.SetConnectionData(address, port, ListenAddress);
            _networkManager.NetworkConfig.ConnectionData = ConnectionPayload.Serialize(_config.GameVersion, GameSettings.Nickname);
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var readPayload = ConnectionPayload.TryDeserialize(request.Payload, out var payload);
            var result = readPayload
                ? ConnectionApproval.Evaluate(
                    payload.GameVersion, _config.GameVersion,
                    _networkManager.ConnectedClientsIds.Count, _config.MaxPlayers, FightInProgress)
                : ApprovalResult.BadPayload;

            response.Approved = result == ApprovalResult.Approved;
            if (response.Approved)
            {
                _nicknames[request.ClientNetworkId] = NicknameRules.Unique(NicknameRules.Sanitize(payload.Nickname), _nicknames.Values);
                Debug.Log($"[Server] {Describe(request.ClientNetworkId)} joined ({_networkManager.ConnectedClientsIds.Count + 1}/{_config.MaxPlayers}).");
            }
            response.CreatePlayerObject = false;
            response.Pending = false;
            if (response.Approved) return;

            response.Reason = ConnectionApproval.ReasonText(result, _config.GameVersion, payload.GameVersion);
            Debug.Log($"Refused client {request.ClientNetworkId}: {response.Reason}");
        }

        private void OnServerStarted()
        {
            _networkManager.SceneManager.LoadScene(Scenes.Game, LoadSceneMode.Single);
        }

        private string Describe(ulong clientId) =>
            _nicknames.TryGetValue(clientId, out var name) && !string.IsNullOrEmpty(name)
                ? $"'{name}' (client {clientId})"
                : $"Client {clientId}";

        private void OnClientDisconnect(ulong clientId)
        {
            if (_networkManager.IsServer)
            {
                if (clientId != _networkManager.LocalClientId)
                    Debug.Log($"[Server] {Describe(clientId)} left ({Mathf.Max(0, _networkManager.ConnectedClientsIds.Count - 1)}/{_config.MaxPlayers}).");
                _nicknames.Remove(clientId);
                return;
            }
            _nicknames.Remove(clientId);

            if (_leaving) return;
            StopTimeout();
            var reason = _networkManager.DisconnectReason;
            Leave(string.IsNullOrEmpty(reason) ? HostLeftMessage : reason);
        }

        private IEnumerator FailAfterTimeout()
        {
            yield return new WaitForSecondsRealtime(_config.ConnectTimeoutSeconds);
            _timeout = null;
            if (_networkManager.IsConnectedClient) yield break;
            var reason = _networkManager.DisconnectReason;
            Leave(string.IsNullOrEmpty(reason) ? TimeoutMessage : reason);
        }

        private void StopTimeout()
        {
            if (_timeout == null) return;
            StopCoroutine(_timeout);
            _timeout = null;
        }

        private void SetStatus(string message) => StatusChanged?.Invoke(message ?? RefusedMessage);
    }
}
