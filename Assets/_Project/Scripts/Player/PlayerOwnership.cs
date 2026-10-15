using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Only the player you own reads input; everyone else's player is driven by the
    /// owner-authoritative NetworkTransform. The rigidbody side is handled by NetworkRigidbody2D.
    /// </summary>
    public class PlayerOwnership : NetworkBehaviour
    {
        [SerializeField] private PlayerInputReader _input;

        private void Awake()
        {
            _input.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            _input.enabled = IsOwner;
        }

        public override void OnNetworkDespawn()
        {
            if (_input != null) _input.enabled = false;
        }
    }
}
