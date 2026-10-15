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

        private readonly NetworkVariable<int> _playerIndex = new(Unassigned);

        private PlayerVisuals _visuals;

        public int PlayerIndex => _playerIndex.Value;

        private void Awake()
        {
            _visuals = GetComponent<PlayerVisuals>();
        }

        public override void OnNetworkSpawn()
        {
            _playerIndex.OnValueChanged += OnPlayerIndexChanged;
            if (_playerIndex.Value != Unassigned) _visuals.Apply(_config, _playerIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
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
