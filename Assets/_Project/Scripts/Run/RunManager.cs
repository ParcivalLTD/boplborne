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

        private readonly NetworkVariable<int> _currency = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<PartyBuff> _activeBuff = new(
            PartyBuff.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<RunResultData> _runResult = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly int[] _damageBySlot = new int[4];
        private readonly int[] _knockbacksBySlot = new int[4];
        private double _runStartTime;
        private bool _isRunActive;

        private NetworkObject _activeArena;
        private bool _hubInitialized;

        /// <summary>The run manager in the loaded Game scene on this PC, if any.</summary>
        public static RunManager Instance { get; private set; }

        /// <summary>Raised on every PC when the floor changes (0 = hub).</summary>
        public event Action<int> FloorChanged;

        /// <summary>Raised on every PC when the shared currency total changes.</summary>
        public event Action<int> CurrencyChanged;

        /// <summary>Raised on every PC when the active party buff changes.</summary>
        public event Action<PartyBuff> ActiveBuffChanged;

        /// <summary>Raised on every PC when the run results change (e.g. at run victory or defeat).</summary>
        public event Action<RunResultData> RunResultChanged;

        public RunConfig Config => _config;
        public int Floor => _floor.Value;
        public int TotalFloors => _config != null && _config.Floors != null ? _config.Floors.Count : 0;
        public int Seed => _seed.Value;
        public int FloorSeed => RunRules.FloorSeed(_seed.Value, _floor.Value);
        public bool InHub => _floor.Value == RunRules.HubFloor;
        public bool IsFinalFloor => RunRules.IsFinalFloor(_floor.Value, TotalFloors);
        public int SharedCurrency => _currency.Value;
        public PartyBuff ActiveBuff => _activeBuff.Value;
        public RunResultData RunResult => _runResult.Value;

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
            _currency.OnValueChanged += OnCurrencyValueChanged;
            _activeBuff.OnValueChanged += OnBuffValueChanged;
            _runResult.OnValueChanged += OnRunResultValueChanged;
            if (IsServer && _bossSpawner != null) _bossSpawner.BossSkipped += OnBossSkipped;
            if (!Application.isBatchMode)
            {
                UI.ResultScreen.EnsureCanvas();
                UI.RestStopScreen.EnsureCanvas();
            }
        }

        public override void OnNetworkDespawn()
        {
            _floor.OnValueChanged -= OnFloorValueChanged;
            _currency.OnValueChanged -= OnCurrencyValueChanged;
            _activeBuff.OnValueChanged -= OnBuffValueChanged;
            _runResult.OnValueChanged -= OnRunResultValueChanged;
            if (_bossSpawner != null) _bossSpawner.BossSkipped -= OnBossSkipped;
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
            _isRunActive = false;
            _runResult.Value = default;

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
            if (firstFloor)
            {
                _runStartTime = NetworkManager.ServerTime.Time;
                _isRunActive = true;
                Array.Clear(_damageBySlot, 0, 4);
                Array.Clear(_knockbacksBySlot, 0, 4);
                _runResult.Value = default;
            }

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

        /// <summary>Server only: awards currency to the group total (e.g. for defeating a boss).</summary>
        public void ServerAwardCurrency(int amount)
        {
            if (!IsServer || amount <= 0) return;
            _currency.Value += amount;
        }

        /// <summary>Server only: attempts to spend <paramref name="cost"/> from shared currency.</summary>
        public bool ServerTrySpendCurrency(int cost)
        {
            if (!IsServer || !CurrencyRules.CanAfford(_currency.Value, cost)) return false;
            _currency.Value = CurrencyRules.Spend(_currency.Value, cost);
            return true;
        }

        /// <summary>Server only: consumes the active one-off party buff after a boss fight.</summary>
        public void ServerConsumeBuff()
        {
            if (IsServer) _activeBuff.Value = PartyBuff.None;
        }

        /// <summary>Client buys a team reroll for shared currency at a rest stop.</summary>
        [Rpc(SendTo.Server)]
        public void BuyRerollRpc(RpcParams rpcParams = default)
        {
            if (!IsServer || _fightLoop.Phase != FightPhase.RestStop) return;
            if (!RestStopRules.CanBuyReroll(_currency.Value, _config.RerollCost)) return;
            if (!ServerTrySpendCurrency(_config.RerollCost)) return;
            _draft.ServerAddRerolls(1);
        }

        /// <summary>Client upgrades one ability in <paramref name="slot"/> to Level 2 at a rest stop.</summary>
        [Rpc(SendTo.Server)]
        public void BuyUpgradeRpc(byte slot, RpcParams rpcParams = default)
        {
            if (!IsServer || _fightLoop.Phase != FightPhase.RestStop) return;
            var clientId = rpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client) || client.PlayerObject == null) return;
            var loadout = client.PlayerObject.GetComponent<AbilityLoadout>();
            if (loadout == null || !loadout.CanUpgrade(slot)) return;
            if (!RestStopRules.CanUpgradeAbility(loadout.Get(slot) != null, loadout.Level(slot), _currency.Value, _config.UpgradeCost)) return;
            if (!ServerTrySpendCurrency(_config.UpgradeCost)) return;
            loadout.ServerUpgrade(slot);
        }

        /// <summary>Client purchases a party buff for the next boss fight at a rest stop.</summary>
        [Rpc(SendTo.Server)]
        public void BuyBuffRpc(PartyBuff buff, RpcParams rpcParams = default)
        {
            if (!IsServer || _fightLoop.Phase != FightPhase.RestStop) return;
            if (!RestStopRules.CanBuyBuff(buff, _activeBuff.Value, _currency.Value, _config.BuffCost)) return;
            if (!ServerTrySpendCurrency(_config.BuffCost)) return;
            _activeBuff.Value = buff;
        }

        private void ResetPlayersForRun()
        {
            _currency.Value = 0;
            _activeBuff.Value = PartyBuff.None;
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (player.TryGetComponent<Health>(out var health))
                {
                    health.ServerSetMax(0);
                    health.ServerRestoreFull();
                }
                if (player.TryGetComponent<ReviveSystem>(out var revive)) revive.ServerResetForFloor();
                if (player.TryGetComponent<PlayerFlask>(out var flask)) flask.ServerRefill();
                if (player.TryGetComponent<AbilityLoadout>(out var loadout)) loadout.ServerClear();
                if (player.TryGetComponent<PlayerWeapon>(out var weapon)) weapon.ServerReset();
                TeleportToSpawn(player);
            }
        }

        private void PreparePlayersForFloor(bool restoreFull)
        {
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (restoreFull && player.TryGetComponent<Health>(out var health))
                {
                    health.ServerSetMax(0);
                    health.ServerRestoreFull();
                }
                if (_activeBuff.Value == PartyBuff.IronSkin && player.TryGetComponent<Health>(out var ironHealth))
                {
                    ironHealth.ServerSetMax(ironHealth.Max + 25);
                }
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

        private void OnBossSkipped(BossData boss)
        {
            if (!IsServer || boss == null || boss.ArenaPrefab == null) return;
            SwapArena(boss.ArenaPrefab);
        }

        /// <summary>Server only: records damage dealt to enemies by player <paramref name="attackerId"/>.</summary>
        public void ServerRecordDamage(ulong attackerId, int amount)
        {
            if (!IsServer || amount <= 0 || !_isRunActive) return;
            var slot = ResolvePlayerSlot(attackerId);
            if (slot >= 0 && slot < 4) _damageBySlot[slot] += amount;
        }

        /// <summary>Server only: records a friendly-fire knockback caused by player <paramref name="attackerId"/>.</summary>
        public void ServerRecordFriendlyKnockback(ulong attackerId)
        {
            if (!IsServer || !_isRunActive) return;
            var slot = ResolvePlayerSlot(attackerId);
            if (slot >= 0 && slot < 4) _knockbacksBySlot[slot]++;
        }

        /// <summary>Server only: finalizes the run stats and publishes <see cref="RunResultData"/> to all PCs.</summary>
        public void ServerFinalizeRunResult(bool isVictory)
        {
            if (!IsServer) return;
            var elapsed = _runStartTime > 0d ? (float)(NetworkManager.ServerTime.Time - _runStartTime) : 0f;
            _runResult.Value = new RunResultData
            {
                IsValid = true,
                IsVictory = isVictory,
                FloorReached = _floor.Value,
                TotalFloors = TotalFloors,
                ElapsedSeconds = elapsed,
                DamageP1 = _damageBySlot[0],
                DamageP2 = _damageBySlot[1],
                DamageP3 = _damageBySlot[2],
                DamageP4 = _damageBySlot[3],
                KnockbacksP1 = _knockbacksBySlot[0],
                KnockbacksP2 = _knockbacksBySlot[1],
                KnockbacksP3 = _knockbacksBySlot[2],
                KnockbacksP4 = _knockbacksBySlot[3],
            };
            _isRunActive = false;
        }

        private int ResolvePlayerSlot(ulong networkId)
        {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var obj))
            {
                if (obj.TryGetComponent<PlayerIdentity>(out var identity))
                    return identity.PlayerIndex;
            }
            if (NetworkManager.ConnectedClients.TryGetValue(networkId, out var client) && client.PlayerObject != null)
            {
                if (client.PlayerObject.TryGetComponent<PlayerIdentity>(out var identity))
                    return identity.PlayerIndex;
            }
            return -1;
        }

        private void OnFloorValueChanged(int previous, int current) => FloorChanged?.Invoke(current);

        private void OnCurrencyValueChanged(int previous, int current)
        {
            CurrencyChanged?.Invoke(current);
            Currency.NotifyAmountChanged(current);
        }

        private void OnBuffValueChanged(PartyBuff previous, PartyBuff current) => ActiveBuffChanged?.Invoke(current);

        private void OnRunResultValueChanged(RunResultData previous, RunResultData current)
        {
            if (current.IsValid)
            {
                MetaProgress.RecordRun(current);
            }
            RunResultChanged?.Invoke(current);
        }
    }
}
