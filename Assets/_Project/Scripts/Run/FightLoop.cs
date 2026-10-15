using System;
using System.Collections.Generic;
using GraceRun.Bosses;
using GraceRun.Combat;
using GraceRun.Net;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Single-fight loop (until RunManager, M6): waiting → countdown → fight → victory or defeat,
    /// then restart. The server owns the phase; the leader (host, or the first client on a
    /// server-only build) starts and restarts through <see cref="StartFightRpc"/>.
    /// Lives in the Game scene as an in-scene NetworkObject.
    /// </summary>
    public class FightLoop : NetworkBehaviour
    {
        [SerializeField] private FightData _data;
        [SerializeField] private BossSpawner _bossSpawner;
        [SerializeField] private PlayerSpawner _playerSpawner;

        private readonly NetworkVariable<FightPhase> _phase = new(
            FightPhase.Waiting, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<double> _countdownEnd = new(
            0d, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<ulong> _leader = new(
            FightRules.NoLeader, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly Dictionary<ulong, Health> _healthById = new();
        private readonly Dictionary<ulong, ReviveSystem> _reviveById = new();

        /// <summary>The fight loop in the loaded Game scene on this PC, if any.</summary>
        public static FightLoop Instance { get; private set; }

        /// <summary>Raised on every PC when the phase changes.</summary>
        public event Action<FightPhase> PhaseChanged;

        public FightPhase Phase => _phase.Value;
        public FightData Data => _data;

        /// <summary>Seconds until the countdown ends, from the shared server clock.</summary>
        public double CountdownRemaining => _countdownEnd.Value - NetworkManager.ServerTime.Time;

        /// <summary>True on the PC whose player may press Start / Restart.</summary>
        public bool IsLocalLeader => IsSpawned && IsClient && _leader.Value == NetworkManager.LocalClientId;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            _phase.OnValueChanged += OnPhaseValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            _phase.OnValueChanged -= OnPhaseValueChanged;
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            UpdateLeader();
            if (_phase.Value == FightPhase.Countdown && CountdownRemaining <= 0d) BeginFight();
            else if (_phase.Value == FightPhase.Fighting) CheckOutcome();
        }

        /// <summary>
        /// Leader only: starts the fight from the waiting room, or restarts it after a victory or
        /// defeat. Every player is healed and moved back to their spawn point, then the countdown runs.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void StartFightRpc(RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != _leader.Value) return;
            var phase = _phase.Value;
            if (phase == FightPhase.Countdown || phase == FightPhase.Fighting) return;

            _bossSpawner.ServerDespawnCurrent();
            ResetPlayers();
            _countdownEnd.Value = NetworkManager.ServerTime.Time + _data.CountdownSeconds;
            _phase.Value = FightPhase.Countdown;
        }

        private void BeginFight()
        {
            if (_data.Boss == null) Debug.LogError("FightData has no boss to fight.", _data);
            _bossSpawner.ServerSpawn(_data.Boss);
            _phase.Value = FightPhase.Fighting;
        }

        private void CheckOutcome()
        {
            var boss = BossBrain.Active;
            var bossDefeated = boss == null || boss.Health.IsDead;
            CountPlayers(out var players, out var living);
            var outcome = FightRules.Outcome(bossDefeated, players, living);
            if (outcome != FightPhase.Fighting) _phase.Value = outcome;
        }

        private void ResetPlayers()
        {
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (TryGetHealth(player, out var health)) health.ServerRestoreFull();
                if (TryGetRevive(player, out var revive)) revive.ServerResetForFloor();
                var slot = player.GetComponent<PlayerIdentity>().PlayerIndex;
                player.GetComponent<PlayerRespawn>().TeleportRpc(_playerSpawner.SpawnPointFor(slot));
            }
        }

        private void CountPlayers(out int players, out int living)
        {
            players = 0;
            living = 0;
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null || !TryGetHealth(player, out var health)) continue;
                players++;
                // A solo extra life already bringing the player back does not count as down.
                if (!health.IsDead || (TryGetRevive(player, out var revive) && revive.ServerWillRevive)) living++;
            }
        }

        private bool TryGetHealth(NetworkObject player, out Health health)
        {
            var id = player.NetworkObjectId;
            if (!_healthById.TryGetValue(id, out health) || health == null)
            {
                health = player.GetComponent<Health>();
                _healthById[id] = health;
            }
            return health != null;
        }

        private bool TryGetRevive(NetworkObject player, out ReviveSystem revive)
        {
            var id = player.NetworkObjectId;
            if (!_reviveById.TryGetValue(id, out revive) || revive == null)
            {
                revive = player.GetComponent<ReviveSystem>();
                _reviveById[id] = revive;
            }
            return revive != null;
        }

        private void UpdateLeader()
        {
            var leader = FightRules.Leader(IsHost, NetworkManager.LocalClientId, NetworkManager.ConnectedClientsIds);
            if (_leader.Value != leader) _leader.Value = leader;
        }

        private void OnPhaseValueChanged(FightPhase previous, FightPhase current) => PhaseChanged?.Invoke(current);
    }
}
