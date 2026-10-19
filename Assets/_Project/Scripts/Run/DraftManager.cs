using System;
using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Combat;
using GraceRun.Player;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// The draft after a won fight (docs/GDD.md, Draft). The server deals every player a hand into
    /// a synced list, takes picks and rerolls through RPCs, puts each pick into the player's
    /// loadout at once, and ends the draft when everyone has picked. Players who join during the
    /// draft get a hand too. Lives in the Game scene as an in-scene NetworkObject next to <see cref="FightLoop"/>.
    /// </summary>
    public class DraftManager : NetworkBehaviour
    {
        [SerializeField] private RunConfig _config;
        [SerializeField] private AbilityCatalog _catalog;
        [SerializeField] private FightLoop _fightLoop;

        private readonly NetworkList<DraftHand> _hands = new(
            null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _extraRerolls = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly Dictionary<ulong, int> _rerollsUsed = new();
        private readonly List<ulong> _pendingJoins = new();
        private DraftDealer _dealer;
        private double _openAt = -1d;
        private double _closeAt = -1d;
        private int _floor = 1;

        /// <summary>The draft in the loaded Game scene on this PC, if any.</summary>
        public static DraftManager Instance { get; private set; }

        /// <summary>Raised on every PC when any hand changes (dealt, rerolled, picked, removed).</summary>
        public event Action HandsChanged;

        public AbilityCatalog Catalog => _catalog;
        public bool IsOpen => IsSpawned && _fightLoop.Phase == FightPhase.Draft;
        public int HandCount => _hands.Count;
        public DraftHand GetHand(int index) => _hands[index];
        public int ExtraRerolls => _extraRerolls.Value;
        public int TotalRerollsAllowed => _config != null ? _config.RerollsPerRun + _extraRerolls.Value : 0;

        private double Now => NetworkManager.ServerTime.Time;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            _hands.OnListChanged += OnHandsChanged;
            if (!IsServer) return;
            _dealer = new DraftDealer(_config, _catalog, Environment.TickCount);
            _fightLoop.PhaseChanged += OnServerPhaseChanged;
            NetworkManager.OnClientConnectedCallback += OnServerClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnServerClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            _hands.OnListChanged -= OnHandsChanged;
            if (Instance == this) Instance = null;
            if (!IsServer) return;
            _fightLoop.PhaseChanged -= OnServerPhaseChanged;
            NetworkManager.OnClientConnectedCallback -= OnServerClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnServerClientDisconnected;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned) return;
            var phase = _fightLoop.Phase;
            if (phase == FightPhase.Victory && _openAt >= 0d && Now >= _openAt) Open();
            else if (phase == FightPhase.Draft) UpdateOpenDraft();
        }

        /// <summary>True and the hand of <paramref name="clientId"/> while that player is in the draft.</summary>
        public bool TryGetHand(ulong clientId, out DraftHand hand)
        {
            var index = IndexOf(clientId);
            hand = index >= 0 ? _hands[index] : default;
            return index >= 0;
        }

        /// <summary>Picks card <paramref name="card"/>; it goes into the first empty slot, or replaces <paramref name="slot"/> when all are full.</summary>
        [Rpc(SendTo.Server)]
        public void PickRpc(byte card, byte slot, RpcParams rpcParams = default)
        {
            var index = IndexOf(rpcParams.Receive.SenderClientId);
            if (!IsOpen || index < 0) return;
            var hand = _hands[index];
            var loadout = LoadoutOf(hand.ClientId);
            if (hand.HasPicked || card >= hand.Cards.Length || loadout == null) return;

            var target = DraftRules.ResolveSlot(slot, EmptySlots(loadout));
            if (target == DraftRules.NoSlot) return;
            loadout.ServerSet(target, _catalog.Get(hand.Cards[card]));
            hand.Picked = (sbyte)card;
            _hands[index] = hand;
        }

        /// <summary>Spends one of this run's rerolls on a new hand.</summary>
        [Rpc(SendTo.Server)]
        public void RerollRpc(RpcParams rpcParams = default)
        {
            var index = IndexOf(rpcParams.Receive.SenderClientId);
            if (!IsOpen || index < 0) return;
            var hand = _hands[index];
            if (hand.HasPicked || hand.RerollsLeft == 0) return;

            _rerollsUsed.TryGetValue(hand.ClientId, out var used);
            _rerollsUsed[hand.ClientId] = used + 1;
            var previous = hand.Cards;
            hand.Cards = Deal(LoadoutOf(hand.ClientId), previous);
            if (hand.Cards.Length < previous.Length) hand.Cards = Deal(LoadoutOf(hand.ClientId), default);
            hand.RerollsLeft = (byte)DraftRules.RerollsLeft(TotalRerollsAllowed, used + 1);
            _hands[index] = hand;
        }

        /// <summary>Server only: grants extra team rerolls (purchased at a rest stop).</summary>
        public void ServerAddRerolls(int count = 1)
        {
            if (!IsServer || count <= 0) return;
            _extraRerolls.Value += count;
            for (var i = 0; i < _hands.Count; i++)
            {
                var hand = _hands[i];
                _rerollsUsed.TryGetValue(hand.ClientId, out var used);
                hand.RerollsLeft = (byte)DraftRules.RerollsLeft(TotalRerollsAllowed, used);
                _hands[i] = hand;
            }
        }

        /// <summary>Server only: resets the dealer and reroll counts for a new run with <paramref name="seed"/>.</summary>
        public void ServerStartRun(int seed)
        {
            if (!IsServer) return;
            _dealer = new DraftDealer(_config, _catalog, seed);
            _extraRerolls.Value = 0;
            _rerollsUsed.Clear();
            _hands.Clear();
            _pendingJoins.Clear();
            _openAt = -1d;
            _closeAt = -1d;
        }

        /// <summary>Server only: opens a draft on <paramref name="floor"/> (0 = hub starter draft, dealt with floor 1 rules).</summary>
        public void ServerOpen(int floor)
        {
            if (!IsServer) return;
            _openAt = -1d;
            _closeAt = -1d;
            _floor = Mathf.Max(1, floor);
            _hands.Clear();
            _pendingJoins.Clear();
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                if (client.PlayerObject == null || !client.PlayerObject.IsSpawned)
                {
                    _pendingJoins.Add(client.ClientId);
                    continue;
                }
                if (client.PlayerObject.TryGetComponent<ReviveSystem>(out var revive)) revive.ServerReviveNow();
                AddHand(client.PlayerObject);
            }
            _fightLoop.ServerSetDraftPhase(true);
        }

        private void OnServerPhaseChanged(FightPhase phase)
        {
            var run = RunManager.Instance;
            var shouldDraft = phase == FightPhase.Victory && (run == null || !run.IsFinalFloor);
            _openAt = shouldDraft ? Now + _config.DraftOpenDelay : -1d;
            if (phase != FightPhase.Draft && _hands.Count > 0) _hands.Clear();
        }

        private void Open()
        {
            var run = RunManager.Instance;
            var boss = run != null && run.CurrentBoss != null ? run.CurrentBoss : _fightLoop.Data.Boss;
            var floor = run != null && run.Floor > 0 ? run.Floor : (boss != null ? boss.Floor : 1);
            ServerOpen(floor);
        }

        private void UpdateOpenDraft()
        {
            AddPendingJoins();
            if (!AllPicked())
            {
                _closeAt = -1d;
                return;
            }
            if (_closeAt < 0d) _closeAt = Now + _config.DraftCloseDelay;
            else if (Now >= _closeAt)
            {
                _closeAt = -1d;
                if (RunManager.Instance != null && RunManager.Instance.IsSpawned)
                    RunManager.Instance.ServerOnDraftClosed();
                else
                    _fightLoop.ServerSetDraftPhase(false);
            }
        }

        private void AddHand(NetworkObject player)
        {
            var clientId = player.OwnerClientId;
            _rerollsUsed.TryGetValue(clientId, out var used);
            _hands.Add(new DraftHand
            {
                ClientId = clientId,
                PlayerIndex = player.GetComponent<PlayerIdentity>().PlayerIndex,
                Cards = Deal(player.GetComponent<AbilityLoadout>(), default),
                Picked = DraftHand.NoPick,
                RerollsLeft = (byte)DraftRules.RerollsLeft(TotalRerollsAllowed, used),
            });
        }

        private FixedList32Bytes<short> Deal(AbilityLoadout loadout, FixedList32Bytes<short> alsoExclude) =>
            _dealer.Deal(loadout, alsoExclude, _floor);

        private void AddPendingJoins()
        {
            for (var i = _pendingJoins.Count - 1; i >= 0; i--)
            {
                var clientId = _pendingJoins[i];
                if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
                    _pendingJoins.RemoveAt(i);
                else if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
                {
                    _pendingJoins.RemoveAt(i);
                    if (IndexOf(clientId) < 0) AddHand(client.PlayerObject);
                }
            }
        }

        private bool AllPicked()
        {
            if (_pendingJoins.Count > 0) return false;
            for (var i = 0; i < _hands.Count; i++)
                if (!_hands[i].HasPicked && _hands[i].Cards.Length > 0) return false;
            return true;
        }

        private void OnServerClientConnected(ulong clientId)
        {
            if (_fightLoop.Phase == FightPhase.Draft)
            {
                _pendingJoins.Add(clientId);
            }
            else if (RunManager.Instance != null && RunManager.Instance.InHub && _fightLoop.Phase == FightPhase.Waiting)
            {
                _floor = 1;
                _closeAt = -1d;
                _pendingJoins.Add(clientId);
                _fightLoop.ServerSetDraftPhase(true);
            }
        }

        private void OnServerClientDisconnected(ulong clientId)
        {
            _pendingJoins.Remove(clientId);
            _rerollsUsed.Remove(clientId);
            var index = IndexOf(clientId);
            if (index >= 0) _hands.RemoveAt(index);
        }

        private int IndexOf(ulong clientId)
        {
            for (var i = 0; i < _hands.Count; i++)
                if (_hands[i].ClientId == clientId) return i;
            return -1;
        }

        private AbilityLoadout LoadoutOf(ulong clientId) =>
            NetworkManager.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null
                ? client.PlayerObject.GetComponent<AbilityLoadout>()
                : null;

        private static bool[] EmptySlots(AbilityLoadout loadout)
        {
            var empty = new bool[AbilityLoadout.SlotCount];
            for (var i = 0; i < empty.Length; i++) empty[i] = loadout.Get(i) == null;
            return empty;
        }

        private void OnHandsChanged(NetworkListEvent<DraftHand> change) => HandsChanged?.Invoke();
    }
}
