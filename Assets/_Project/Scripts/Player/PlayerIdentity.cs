using System.Collections.Generic;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Which of the four player slots this player holds. The server assigns it on spawn;
    /// every client turns it into the player's color and hat.
    /// </summary>
    [RequireComponent(typeof(PlayerVisuals))]
    public class PlayerIdentity : NetworkBehaviour
    {
        /// <summary>Slot value before the server has assigned one.</summary>
        public const int Unassigned = -1;

        [SerializeField] private GameConfig _config;

        private static readonly List<PlayerIdentity> _all = new();

        private readonly NetworkVariable<int> _playerIndex = new(Unassigned);

        private PlayerVisuals _visuals;

        /// <summary>
        /// Every spawned player on this PC. Clients can't read the server's connected-client list,
        /// so the HUD finds players here.
        /// </summary>
        public static IReadOnlyList<PlayerIdentity> All => _all;

        public int PlayerIndex => _playerIndex.Value;
        public GameConfig Config => _config;

        private void Awake()
        {
            _visuals = GetComponent<PlayerVisuals>();
        }

        public override void OnNetworkSpawn()
        {
            _all.Add(this);
            _playerIndex.OnValueChanged += OnPlayerIndexChanged;
            if (_playerIndex.Value != Unassigned) _visuals.Apply(_config, _playerIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            _all.Remove(this);
            _playerIndex.OnValueChanged -= OnPlayerIndexChanged;
        }

        /// <summary>Server only: gives this player its slot, and with it its color and hat.</summary>
        public void AssignIndex(int playerIndex)
        {
            _playerIndex.Value = playerIndex;
        }

        private void OnPlayerIndexChanged(int previous, int current)
        {
            if (current != Unassigned) _visuals.Apply(_config, current);
        }
    }
}
