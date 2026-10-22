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

        private readonly NetworkVariable<BuffStacks> _buffs = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _selection = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<ArenaState> _arena = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private double _arenaNextRoundAt = -1d;

        private readonly NetworkVariable<bool> _sessionActive = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        public event Action<BuffStacks> BuffsChanged;

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
        public BuffStacks Buffs => _buffs.Value;
        public RunResultData RunResult => _runResult.Value;

        /// <summary>The hub's mode choice: an index into the list Run, each practice boss, Arena (see <see cref="ModeRules"/>).</summary>
        public int Selection => _selection.Value;

        /// <summary>The mode the leader has picked in the hub.</summary>
        public GameMode SelectedMode => ModeRules.ModeOf(_selection.Value, PracticeBossCount);

        /// <summary>True from the start of a Boss practice session until it returns to the hub.</summary>
        public bool IsPractice => _sessionActive.Value && SelectedMode == GameMode.Practice;

        /// <summary>True from the start of an Arena match until it returns to the hub.</summary>
        public bool IsArena => _sessionActive.Value && SelectedMode == GameMode.Arena;

        /// <summary>The Arena match: round wins, round number and last round's winner.</summary>
        public ArenaState Arena => _arena.Value;

        private int PracticeBossCount => _config != null ? _config.PracticeBosses.Length : 0;

        /// <summary>The boss the leader has picked for practice, or null.</summary>
        public BossData PracticeBoss
        {
            get
            {
                var index = ModeRules.BossIndexOf(_selection.Value, PracticeBossCount);
                return index >= 0 ? _config.PracticeBosses[index] : null;
            }
        }

        /// <summary>The boss chosen for the current floor (the practice boss in Boss practice), or null in the hub.</summary>
        public BossData CurrentBoss
        {
            get
            {
                if (IsPractice) return PracticeBoss;
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
            _buffs.OnValueChanged += OnBuffValueChanged;
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
            _buffs.OnValueChanged -= OnBuffValueChanged;
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
            if (IsArena) ServerTickArena();
        }

        /// <summary>
        /// Server only: called when the leader presses the main action button (Start run in the hub,
        /// Next floor at a rest stop, or Return to Hub after a final victory or defeat).
        /// </summary>
        public void ServerAdvance()
        {
            if (!IsServer) return;
            var phase = _fightLoop.Phase;
            if (_sessionActive.Value && IsPractice)
            {
                if (phase == FightPhase.Defeat) ServerStartPractice();
                else if (phase == FightPhase.Victory) ServerEnterHub(true);
                return;
            }
            if (_sessionActive.Value && IsArena)
            {
                if (phase == FightPhase.Victory && _arena.Value.MatchOver) ServerEnterHub(true);
                return;
            }
            if (phase == FightPhase.Waiting && SelectedMode == GameMode.Arena)
            {
                ServerStartArena();
                return;
            }
            if (!RunRules.CanAdvance(phase, _floor.Value, TotalFloors)) return;

            if (phase == FightPhase.Waiting && SelectedMode == GameMode.Practice) ServerStartPractice();
            else if (phase == FightPhase.Waiting) ServerStartFloor(1);
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
        public void ServerEnterHub(bool keepKit = false)
        {
            if (!IsServer) return;
            _sessionActive.Value = false;
            _seed.Value = Environment.TickCount;
            _floor.Value = RunRules.HubFloor;
            _bossPoolIndex.Value = 0;
            _isRunActive = false;
            _runResult.Value = default;

            _bossSpawner.ServerDespawnCurrent();
            AbilityEffect.ServerDespawnAll();
            SwapArena(_config.HubArenaPrefab);
            if (TrainingDummySpawner.Instance != null) TrainingDummySpawner.Instance.ServerSpawn();

            if (keepKit)
            {
                RestorePlayersForHub();
                _fightLoop.ServerSetPhase(FightPhase.Waiting);
                return;
            }
            ResetPlayersForRun();
            _draft.ServerStartRun(_seed.Value);
            if (NetworkManager.ConnectedClientsIds.Count > 0)
                _draft.ServerOpen(RunRules.HubFloor);
            else
                _fightLoop.ServerSetPhase(FightPhase.Waiting);
        }

        /// <summary>Leader only, in the hub: moves the mode choice <paramref name="delta"/> entries along the list.</summary>
        [Rpc(SendTo.Server)]
        public void CycleSelectionRpc(int delta, RpcParams rpcParams = default)
        {
            if (_fightLoop.Phase != FightPhase.Waiting || !_fightLoop.IsLeader(rpcParams.Receive.SenderClientId)) return;
            _selection.Value = ModeRules.Step(_selection.Value, delta, PracticeBossCount);
        }

        /// <summary>Leader only, after a Boss practice fight ends: back to the hub with the same abilities.</summary>
        [Rpc(SendTo.Server)]
        public void LeavePracticeRpc(RpcParams rpcParams = default)
        {
            var phase = _fightLoop.Phase;
            if (!IsPractice || (phase != FightPhase.Defeat && phase != FightPhase.Victory)) return;
            if (!_fightLoop.IsLeader(rpcParams.Receive.SenderClientId)) return;
            ServerEnterHub(true);
        }

        /// <summary>
        /// Server only: starts (or restarts after a defeat) a Boss practice fight: the chosen boss in its arena,
        /// everyone at full health with a filled kit, straight into the countdown. No story, draft or run progress.
        /// </summary>
        private void ServerStartPractice()
        {
            var boss = PracticeBoss;
            if (boss == null) return;
            _sessionActive.Value = true;
            _floor.Value = 1;
            _isRunActive = false;
            _runResult.Value = default;

            _bossSpawner.ServerDespawnCurrent();
            AbilityEffect.ServerDespawnAll();
            if (TrainingDummySpawner.Instance != null) TrainingDummySpawner.Instance.ServerDespawn();
            if (boss.ArenaPrefab != null) SwapArena(boss.ArenaPrefab);

            PreparePlayersForFloor(true);
            FillModeKit();
            _fightLoop.ServerBeginCountdown(boss);
        }

        /// <summary>Server only: starts an Arena match. Needs at least one player on each team.</summary>
        private void ServerStartArena()
        {
            if (!ArenaRules.HasBothTeams(ConnectedSlots())) return;
            _sessionActive.Value = true;
            _floor.Value = 1;
            _isRunActive = false;
            _runResult.Value = default;
            _arena.Value = new ArenaState { Round = 1 };
            _bossSpawner.ServerDespawnCurrent();
            if (TrainingDummySpawner.Instance != null) TrainingDummySpawner.Instance.ServerDespawn();
            ServerBeginArenaRound();
        }

        /// <summary>Server only: resets the arena and everyone's health, puts the teams on opposite sides and starts the countdown.</summary>
        private void ServerBeginArenaRound()
        {
            _arenaNextRoundAt = -1d;
            AbilityEffect.ServerDespawnAll();
            SwapArena(_config.HubArenaPrefab);
            RestorePlayersForHub(true);
            FillModeKit();
            _fightLoop.ServerBeginCountdown(null);
        }

        /// <summary>Server only, each frame during an Arena match: ends the round when a team has nobody standing, and starts the next one after the intermission.</summary>
        private void ServerTickArena()
        {
            var state = _arena.Value;
            if (_fightLoop.Phase == FightPhase.Fighting)
            {
                CountArenaTeams(out var livingA, out var livingB);
                var result = ArenaRules.RoundResult(livingA, livingB);
                if (result == ArenaRound.Undecided) return;
                if (result == ArenaRound.TeamA) state.ScoreA++;
                else if (result == ArenaRound.TeamB) state.ScoreB++;
                state.LastRound = result;
                state.MatchOver = ArenaRules.MatchWinner(state.ScoreA, state.ScoreB, _config.ArenaWinsNeeded) >= 0;
                _arena.Value = state;
                _arenaNextRoundAt = state.MatchOver ? -1d : NetworkManager.ServerTime.Time + _config.ArenaIntermission;
                _fightLoop.ServerSetPhase(FightPhase.Victory);
            }
            else if (_fightLoop.Phase == FightPhase.Victory && !state.MatchOver && _arenaNextRoundAt > 0d &&
                     NetworkManager.ServerTime.Time >= _arenaNextRoundAt)
            {
                state.Round++;
                _arena.Value = state;
                ServerBeginArenaRound();
            }
        }

        private void CountArenaTeams(out int livingA, out int livingB)
        {
            livingA = 0;
            livingB = 0;
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null || !player.TryGetComponent<Health>(out var health) || health.IsDead) continue;
                if (ArenaRules.TeamOf(player.GetComponent<PlayerIdentity>().PlayerIndex) == 0) livingA++;
                else livingB++;
            }
        }

        private System.Collections.Generic.List<int> ConnectedSlots()
        {
            var slots = new System.Collections.Generic.List<int>();
            foreach (var client in NetworkManager.ConnectedClientsList)
                if (client.PlayerObject != null) slots.Add(client.PlayerObject.GetComponent<PlayerIdentity>().PlayerIndex);
            return slots;
        }

        /// <summary>Server only: puts each configured kit ability into the player's empty slots.</summary>
        private void FillModeKit()
        {
            var kit = _config.ModeKit;
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null || !player.TryGetComponent<AbilityLoadout>(out var loadout)) continue;
                for (var slot = 0; slot < AbilityLoadout.SlotCount && slot < kit.Length; slot++)
                    if (loadout.Get(slot) == null && kit[slot] != null) loadout.ServerSet(slot, kit[slot]);
            }
        }

        /// <summary>Server only: back in the hub with the same abilities: full health, revived, flasks refilled, at the spawn points.</summary>
        private void RestorePlayersForHub(bool arenaSpawns = false)
        {
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                if (player.TryGetComponent<Health>(out var health))
                {
                    health.ServerSetMax(0);
                    health.ServerRestoreFull();
                }
                if (player.TryGetComponent<ReviveSystem>(out var revive))
                {
                    revive.ServerReviveNow();
                    revive.ServerResetForFloor();
                }
                if (player.TryGetComponent<PlayerFlask>(out var flask)) flask.ServerRefill();
                if (arenaSpawns) TeleportToArenaSpawn(player);
                else TeleportToSpawn(player);
            }
        }

        /// <summary>Team A starts on the left, team B on the right, at the height of the normal spawn points.</summary>
        private void TeleportToArenaSpawn(NetworkObject player)
        {
            var slot = player.GetComponent<PlayerIdentity>().PlayerIndex;
            var at = _playerSpawner.SpawnPointFor(0);
            at.x = ArenaRules.SpawnX(slot, _config.ArenaSpawnEdge, 2.5f);
            player.GetComponent<PlayerRespawn>().TeleportRpc(at);
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
            _fightLoop.ServerBeginStory(boss);
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
            if (IsServer) _buffs.Value = default;
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
            var tuning = _config.PartyBuffs;
            var cost = tuning.CostOfNext(buff, _buffs.Value, _config.BuffCost);
            if (!RestStopRules.CanBuyBuff(buff, _buffs.Value, tuning.MaxStacks, _currency.Value, cost)) return;
            if (!ServerTrySpendCurrency(cost)) return;
            _buffs.Value = _buffs.Value.Plus(buff);
        }

        private void ResetPlayersForRun()
        {
            _currency.Value = 0;
            _buffs.Value = default;
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
                var bonusMaxHp = _config.PartyBuffs.BonusMaxHp(_buffs.Value);
                if (bonusMaxHp > 0 && player.TryGetComponent<Health>(out var ironHealth))
                {
                    ironHealth.ServerSetMax(ironHealth.Max + bonusMaxHp);
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

        private void OnBuffValueChanged(BuffStacks previous, BuffStacks current) => BuffsChanged?.Invoke(current);

        private void OnRunResultValueChanged(RunResultData previous, RunResultData current) => RunResultChanged?.Invoke(current);
    }
}
