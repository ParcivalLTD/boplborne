using System;
using System.Collections.Generic;
using GraceRun.Core;
using GraceRun.Net;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Which of the four player slots this player holds, and the nickname they joined with. The
    /// server assigns both on spawn; every client turns the slot into the player's color and hat, and
    /// shows <see cref="DisplayName"/> (the nickname, or P1-P4 when there is none).
    /// </summary>
    [RequireComponent(typeof(PlayerVisuals))]
    public class PlayerIdentity : NetworkBehaviour
    {
        /// <summary>Slot value before the server has assigned one.</summary>
        public const int Unassigned = -1;

        [SerializeField] private GameConfig _config;

        private static readonly List<PlayerIdentity> _all = new();

        private readonly NetworkVariable<int> _playerIndex = new(Unassigned);
        private readonly NetworkVariable<FixedString64Bytes> _nickname = new();

        private PlayerVisuals _visuals;

        /// <summary>
        /// Every spawned player on this PC. Clients can't read the server's connected-client list,
        /// so the HUD finds players here.
        /// </summary>
        public static IReadOnlyList<PlayerIdentity> All => _all;

        /// <summary>Raised on every PC when a player's slot or nickname changes.</summary>
        public static event Action Renamed;

        public int PlayerIndex => _playerIndex.Value;
        public GameConfig Config => _config;

        /// <summary>The nickname, or "P1".."P4" when the player has none; empty before the slot is assigned.</summary>
        public string DisplayName => NicknameRules.Display(_nickname.Value.ToString(), _playerIndex.Value);

        /// <summary>The display name of whoever holds <paramref name="slot"/> (0-3), or "P1".."P4" if nobody does yet.</summary>
        public static string NameOfSlot(int slot)
        {
            foreach (var player in _all)
                if (player != null && player.IsSpawned && player.PlayerIndex == slot) return player.DisplayName;
            return NicknameRules.Display(null, slot);
        }

        private void Awake()
        {
            _visuals = GetComponent<PlayerVisuals>();
        }

        public override void OnNetworkSpawn()
        {
            _all.Add(this);
            _playerIndex.OnValueChanged += OnPlayerIndexChanged;
            _nickname.OnValueChanged += OnNicknameChanged;
            if (_playerIndex.Value != Unassigned) _visuals.Apply(_config, _playerIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            _all.Remove(this);
            _playerIndex.OnValueChanged -= OnPlayerIndexChanged;
            _nickname.OnValueChanged -= OnNicknameChanged;
        }

        /// <summary>Server only: gives this player its slot, and with it its color and hat.</summary>
        public void AssignIndex(int playerIndex)
        {
            _playerIndex.Value = playerIndex;
        }

        /// <summary>Server only: sets the cleaned nickname (empty to be shown as P1-P4).</summary>
        public void AssignNickname(string nickname)
        {
            _nickname.Value = new FixedString64Bytes(NicknameRules.Sanitize(nickname));
        }

        private void OnPlayerIndexChanged(int previous, int current)
        {
            if (current != Unassigned) _visuals.Apply(_config, current);
            Renamed?.Invoke();
        }

        private void OnNicknameChanged(FixedString64Bytes previous, FixedString64Bytes current) => Renamed?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _all.Clear();
            Renamed = null;
        }
    }
}
