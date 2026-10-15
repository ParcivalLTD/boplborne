using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Splits a spawned player into "mine" and "someone else's": the owner simulates physics and
    /// reads input, everyone else only follows the owner-authoritative NetworkTransform.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerOwnership : NetworkBehaviour
    {
        [SerializeField] private PlayerInputReader _input;

        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _input.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            _body.bodyType = IsOwner ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            _input.enabled = IsOwner;
        }

        public override void OnNetworkDespawn()
        {
            if (_input != null) _input.enabled = false;
        }
    }
}
