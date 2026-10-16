using System;
using GraceRun.Abilities;
using GraceRun.Abilities.Effects;
using GraceRun.Bosses;
using GraceRun.Combat;
using GraceRun.Net;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Server-authoritative run loop across floors (docs/ARCHITECTURE.md, docs/GDD.md):
    /// hub (pick a starter ability) → floor fight → draft → rest stop after configured floors →
    /// next floor → final boss → run victory (or defeat on any floor) → back to hub.
    /// Also swaps arena prefabs as NetworkObjects on the server and owns the per-run random seed.
    /// </summary>
    public class RunManager : NetworkBehaviour
    {
        [SerializeField] private RunConfig _config;
        [SerializeField] private FightLoop _fightLoop;
        [SerializeField] private DraftManager _draft;
        [SerializeField] private BossSpawner _bossSpawner;
        [SerializeField] private PlayerSpawner _playerSpawner;

        private readonly NetworkVariable<int> _floor = new(
            RunRules.HubFloor, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _seed = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _bossPoolIndex = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private NetworkObject _activeArena;
        private bool _hubInitialized;

        /// <summary>The run manager in the loaded Game scene on this PC, if any.</summary>
        public static RunManager Instance { get; private set; }

        /// <summary>Raised on every PC when the floor changes (0 = hub).</summary>
        public event Action<int> FloorChanged;

        public RunConfig Config => _config;
        public int Floor => _floor.Value;
        public int TotalFloors => _config != null && _config.Floors != null ? _config.Floors.Count : 0;
        public int Seed => _seed.Value;
        public int FloorSeed => RunRules.FloorSeed(_seed.Value, _floor.Value);
        public bool InHub => _floor.Value == RunRules.HubFloor;
        public bool IsFinalFloor => RunRules.IsFinalFloor(_floor.Value, TotalFloors);

        /// <summary>The boss chosen for the current floor, or null in the hub.</summary>
        public BossData CurrentBoss
        {
            get
            {
                var floor = _floor.Value;
                if (floor <= RunRules.HubFloor || floor > TotalFloors) return null;
                var pool = _config.Floors[floor - 1].Bosses;
                if (pool == null || pool.Length == 0) return null;
                return pool[Mathf.Clamp(_bossPoolIndex.Value, 0, pool.Length - 1)];
            }
        }

        public override void OnNetworkSpawn()
        {
            Instance = this;
            _floor.OnValueChanged += OnFloorValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            _floor.OnValueChanged -= OnFloorValueChanged;
            if (Instance == this) Instance = null;
            _hubInitialized = false;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            if (!_hubInitialized)
            {
                _hubInitialized = true;
                ServerEnterHub();
            }
        }

        /// <summary>
        /// Server only: called when the leader presses the main action button (Start run in the hub,
        /// Next floor at a rest stop, or Return to Hub after a final victory or defeat).
        /// </summary>
        public void ServerAdvance()
        {
            if (!IsServer || !RunRules.CanAdvance(_fightLoop.Phase, _floor.Value, TotalFloors)) return;

            if (_fightLoop.Phase == FightPhase.Waiting) ServerStartFloor(1);
            else if (_fightLoop.Phase == FightPhase.RestStop) ServerStartFloor(_floor.Value + 1);
            else ServerEnterHub();
        }

        /// <summary>Server only: called by <see cref="DraftManager"/> when every player has picked and the close delay ends.</summary>
        public void ServerOnDraftClosed()
        {
            if (!IsServer) return;
            if (InHub)
            {
                _fightLoop.ServerSetPhase(FightPhase.Waiting);
            }
            else if (RunRules.HasRestStopAfter(_floor.Value, TotalFloors, _config.RestAfterFloors))
            {
                ServerEnterRestStop();
            }
            else
            {
                ServerStartFloor(_floor.Value + 1);
            }
        }

        /// <summary>Server only: everyone disconnected mid-fight; reset back to the hub so newcomers start fresh.</summary>
        public void ServerAbandonToHub()
        {
            if (IsServer) ServerEnterHub();
        }

        /// <summary>Server only: enters the hub for a new run, resets loadouts, HP and flasks, and deals starter cards.</summary>
        public void ServerEnterHub()
        {
            if (!IsServer) return;
            _seed.Value = Environment.TickCount;
            _floor.Value = RunRules.HubFloor;
            _bossPoolIndex.Value = 0;

            _bossSpawner.ServerDespawnCurrent();
            AbilityEffect.ServerDespawnAll();
            SwapArena(_config.HubArenaPrefab);
            if (TrainingDummySpawner.Instance != null) TrainingDummySpawner.Instance.ServerSpawn();

            ResetPlayersForRun();
            _draft.ServerStartRun(_seed.Value);
            if (NetworkManager.ConnectedClientsIds.Count > 0)
                _draft.ServerOpen(RunRules.HubFloor);
            else
                _fightLoop.ServerSetPhase(FightPhase.Waiting);
        }

        private void ServerEnterRestStop()
        {
            _bossSpawner.ServerDespawnCurrent();
            AbilityEffect.ServerDespawnAll();
            SwapArena(_config.HubArenaPrefab);
            if (TrainingDummySpawner.Instance != null) TrainingDummySpawner.Instance.ServerSpawn();

            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (player.TryGetComponent<ReviveSystem>(out var revive)) revive.ServerReviveNow();
                if (player.TryGetComponent<PlayerFlask>(out var flask)) flask.ServerRefill();
                TeleportToSpawn(player);
            }
            _fightLoop.ServerSetPhase(FightPhase.RestStop);
        }

        private void ServerStartFloor(int floor)
        {
            if (floor < 1 || floor > TotalFloors) return;
            var firstFloor = InHub;
            _floor.Value = floor;

            var pool = _config.Floors[floor - 1].Bosses;
            var rng = new System.Random(FloorSeed);
            _bossPoolIndex.Value = Mathf.Max(0, RunRules.PickBossIndex(pool != null ? pool.Length : 0, rng.NextDouble()));

            _bossSpawner.ServerDespawnCurrent();
            AbilityEffect.ServerDespawnAll();
            if (TrainingDummySpawner.Instance != null) TrainingDummySpawner.Instance.ServerDespawn();

            var boss = CurrentBoss;
            if (boss != null && boss.ArenaPrefab != null) SwapArena(boss.ArenaPrefab);

            PreparePlayersForFloor(firstFloor);
            _fightLoop.ServerBeginCountdown(boss);
        }

        private void ResetPlayersForRun()
        {
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (player.TryGetComponent<Health>(out var health)) health.ServerRestoreFull();
                if (player.TryGetComponent<ReviveSystem>(out var revive)) revive.ServerResetForFloor();
                if (player.TryGetComponent<PlayerFlask>(out var flask)) flask.ServerRefill();
                if (player.TryGetComponent<AbilityLoadout>(out var loadout)) loadout.ServerClear();
                TeleportToSpawn(player);
            }
        }

        private void PreparePlayersForFloor(bool restoreFull)
        {
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (restoreFull && player.TryGetComponent<Health>(out var health)) health.ServerRestoreFull();
                if (restoreFull && player.TryGetComponent<PlayerFlask>(out var flask)) flask.ServerRefill();
                if (player.TryGetComponent<ReviveSystem>(out var revive))
                {
                    revive.ServerReviveNow();
                    revive.ServerResetForFloor();
                }
                TeleportToSpawn(player);
            }
        }

        private void TeleportToSpawn(NetworkObject player)
        {
            var slot = player.GetComponent<PlayerIdentity>().PlayerIndex;
            player.GetComponent<PlayerRespawn>().TeleportRpc(_playerSpawner.SpawnPointFor(slot));
        }

        private void SwapArena(GameObject arenaPrefab)
        {
            if (_activeArena != null && _activeArena.IsSpawned) _activeArena.Despawn();
            _activeArena = null;
            if (arenaPrefab == null) return;

            var instance = Instantiate(arenaPrefab, Vector3.zero, Quaternion.identity);
            if (instance.TryGetComponent<NetworkObject>(out var netObj))
            {
                netObj.Spawn(true);
                _activeArena = netObj;
            }
        }

        private void OnFloorValueChanged(int previous, int current) => FloorChanged?.Invoke(current);
    }
}
