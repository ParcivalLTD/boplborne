using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Puts a player back at a given spot (fight restart). Movement is owner-authoritative, so the
    /// server asks the owner, who stops the body and teleports without interpolation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(NetworkTransform))]
    public class PlayerRespawn : NetworkBehaviour
    {
        private Rigidbody2D _body;
        private NetworkTransform _networkTransform;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _networkTransform = GetComponent<NetworkTransform>();
        }

        /// <summary>Server to owner: move this player to <paramref name="position"/> and stop it.</summary>
        [Rpc(SendTo.Owner)]
        public void TeleportRpc(Vector2 position)
        {
            _body.linearVelocity = Vector2.zero;
            _body.position = position;
            _networkTransform.Teleport(position, transform.rotation, transform.localScale);
        }
    }
}
