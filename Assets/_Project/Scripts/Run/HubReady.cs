using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Hub ready-up. While the fight loop waits in the hub every player toggles Ready (Interact key
    /// or the hub button). When all connected players are ready a short timer starts the run; anyone
    /// un-readying, or a newcomer joining, cancels it. The leader may start early with
    /// <see cref="ForceStartRpc"/>. The server owns the ready flags.
    /// </summary>
    [RequireComponent(typeof(FightLoop))]
    public class HubReady : NetworkBehaviour
    {
        private readonly NetworkList<ulong> _ready = new(
            null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<double> _startAt = new(
            -1d, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private FightLoop _loop;
        private PlayerInputReader _input;

        /// <summary>The ready-up in the loaded Game scene on this PC, if any.</summary>
        public static HubReady Instance { get; private set; }

        /// <summary>Raised on every PC when the ready flags or the start timer change.</summary>
        public event System.Action Changed;

        /// <summary>True while the ready-up applies (hub, waiting for the run to start).</summary>
        public bool IsActive => IsSpawned && _loop.Phase == FightPhase.Waiting;

        /// <summary>True when the start timer is running.</summary>
        public bool IsStarting => _startAt.Value >= 0d;

        /// <summary>Seconds left on the start timer, from the shared server clock.</summary>
        public double StartRemaining => _startAt.Value - NetworkManager.ServerTime.Time;

        /// <summary>True when this PC's player is ready.</summary>
        public bool LocalReady => IsSpawned && _ready.Contains(NetworkManager.LocalClientId);

        public int ReadyCount
        {
            get
            {
                var count = 0;
                foreach (var id in NetworkManager.ConnectedClientsIds)
                    if (_ready.Contains(id)) count++;
                return count;
            }
        }

        public int PlayerCount => NetworkManager.ConnectedClientsIds.Count;

        private void Awake() => _loop = GetComponent<FightLoop>();

        public override void OnNetworkSpawn()
        {
            Instance = this;
            _ready.OnListChanged += OnListChanged;
            _startAt.OnValueChanged += OnStartChanged;
            if (IsServer) NetworkManager.OnClientDisconnectCallback += OnServerClientGone;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
            _ready.OnListChanged -= OnListChanged;
            _startAt.OnValueChanged -= OnStartChanged;
            if (_input != null)
            {
                _input.InteractPressed -= OnInteract;
                _input.FlaskPressed -= OnFlask;
            }
            _input = null;
            if (IsServer && NetworkManager != null) NetworkManager.OnClientDisconnectCallback -= OnServerClientGone;
        }

        private void Update()
        {
            if (!IsSpawned) return;
            if (IsClient) BindInput();
            if (IsServer) ServerTick();
        }

        /// <summary>Any client: sets this player's ready flag. Ignored outside the hub wait.</summary>
        [Rpc(SendTo.Server)]
        public void SetReadyRpc(bool ready, RpcParams rpcParams = default)
        {
            if (_loop.Phase != FightPhase.Waiting) return;
            var id = rpcParams.Receive.SenderClientId;
            if (ready && !_ready.Contains(id)) _ready.Add(id);
            else if (!ready) _ready.Remove(id);
        }

        /// <summary>Leader only: starts the run now even though not everyone is ready.</summary>
        [Rpc(SendTo.Server)]
        public void ForceStartRpc(RpcParams rpcParams = default)
        {
            if (_loop.Phase != FightPhase.Waiting || !_loop.IsLeader(rpcParams.Receive.SenderClientId)) return;
            ServerStartRun();
        }

        /// <summary>Flips this PC's ready flag.</summary>
        public void ToggleLocal()
        {
            if (IsActive) SetReadyRpc(!LocalReady);
        }

        private void ServerTick()
        {
            if (_loop.Phase != FightPhase.Waiting)
            {
                if (_ready.Count > 0) _ready.Clear();
                if (_startAt.Value >= 0d) _startAt.Value = -1d;
                return;
            }

            var all = ReadyRules.AllReady(PlayerCount, ReadyCount);

            if (!all)
            {
                if (_startAt.Value >= 0d) _startAt.Value = -1d;
                return;
            }

            var now = NetworkManager.ServerTime.Time;
            if (_startAt.Value < 0d) _startAt.Value = now + _loop.Data.ReadyStartDelaySeconds;
            else if (now >= _startAt.Value) ServerStartRun();
        }

        private void ServerStartRun()
        {
            _startAt.Value = -1d;
            _ready.Clear();
            if (RunManager.Instance != null && RunManager.Instance.IsSpawned) RunManager.Instance.ServerAdvance();
        }

        private void OnServerClientGone(ulong clientId) => _ready.Remove(clientId);

        private void BindInput()
        {
            if (_input != null) return;
            var local = NetworkManager.LocalClient;
            var player = local != null ? local.PlayerObject : null;
            if (player == null || !player.TryGetComponent(out _input)) return;
            _input.InteractPressed += OnInteract;
            _input.FlaskPressed += OnFlask;
        }

        private void OnInteract()
        {
            var run = RunManager.Instance;
            var over = _loop.Phase == FightPhase.Defeat || _loop.Phase == FightPhase.Victory;
            if (run != null && run.IsSpawned && run.IsPractice && over)
            {
                run.LeavePracticeRpc();
                return;
            }
            ToggleLocal();
        }

        /// <summary>The leader changes the hub mode with the flask key; everyone else keeps the key for the flask.</summary>
        private void OnFlask()
        {
            var run = RunManager.Instance;
            if (IsActive && run != null && run.IsSpawned && _loop.IsLocalLeader) run.CycleSelectionRpc(1);
        }

        private void OnListChanged(NetworkListEvent<ulong> _) => Changed?.Invoke();

        private void OnStartChanged(double _, double __) => Changed?.Invoke();
    }
}
