using System;
using System.Collections.Generic;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Applies knockback on the PC that simulates the body: the owning player for a player, the
    /// server for a boss or dummy. The server decides the push, the owner applies it. While a
    /// <see cref="Hitstop"/> holds the body still, the push waits until it thaws.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Knockback : NetworkBehaviour
    {
        [SerializeField, Tooltip("Optional. Delays the push until hitstop ends.")]
        private Hitstop _hitstop;

        private static readonly Dictionary<Rigidbody2D, Knockback> ByBody = new();

        private Rigidbody2D _body;

        /// <summary>The body's current velocity as this PC simulates it.</summary>
        public Vector2 Velocity => _body.linearVelocity;

        /// <summary>Raised on the owner when a push is applied.</summary>
        public event Action<Vector2> Pushed;

        /// <summary>Raised on the owner on every step a force steers the body (see <see cref="Steer"/>).</summary>
        public event Action Steered;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            ByBody[_body] = this;
        }

        public override void OnDestroy()
        {
            ByBody.Remove(_body);
            base.OnDestroy();
        }

        /// <summary>The Knockback on <paramref name="body"/>, without a GetComponent call (safe in FixedUpdate).</summary>
        public static bool TryFind(Rigidbody2D body, out Knockback knockback)
        {
            knockback = null;
            return body != null && ByBody.TryGetValue(body, out knockback) && knockback != null;
        }

        /// <summary>Replaces the body's velocity with <paramref name="velocity"/> (units per second).</summary>
        [Rpc(SendTo.Owner)]
        public void PushRpc(Vector2 velocity) => ApplyLocal(velocity);

        /// <summary>Owner only: the same push as <see cref="PushRpc"/>, for effects detected on this PC.</summary>
        public void ApplyLocal(Vector2 velocity)
        {
            if (!IsOwner) return;
            if (_hitstop == null || !_hitstop.DeferVelocity(velocity)) _body.linearVelocity = velocity;
            Pushed?.Invoke(velocity);
        }

        /// <summary>
        /// Owner only, every physics step a force acts (pull fields): turns the velocity toward
        /// <paramref name="target"/> by at most <paramref name="maxChange"/>. Raises <see cref="Steered"/>.
        /// </summary>
        public void Steer(Vector2 target, float maxChange)
        {
            if (!IsOwner) return;
            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity, target, maxChange);
            Steered?.Invoke();
        }
    }
}
