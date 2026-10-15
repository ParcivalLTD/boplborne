using System;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Applies knockback on the PC that simulates the body: the owning player for a player, the
    /// server for a boss or dummy. The server decides the push, the owner applies it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Knockback : NetworkBehaviour
    {
        private Rigidbody2D _body;

        /// <summary>Raised on the owner when a push is applied.</summary>
        public event Action<Vector2> Pushed;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>Replaces the body's velocity with <paramref name="velocity"/> (units per second).</summary>
        [Rpc(SendTo.Owner)]
        public void PushRpc(Vector2 velocity)
        {
            _body.linearVelocity = velocity;
            Pushed?.Invoke(velocity);
        }
    }
}
