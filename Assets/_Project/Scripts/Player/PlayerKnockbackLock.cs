using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Owner only: when a knockback lands (boss, friendly fire, abilities) or a pull field acts, input stops steering for a moment
    /// so the push actually carries the player instead of being braked away by the run deceleration.
    /// </summary>
    public class PlayerKnockbackLock : NetworkBehaviour
    {
        [SerializeField] private Knockback _knockback;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerMovementData _data;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _knockback.Pushed += OnPushed;
            _knockback.Steered += OnSteered;
        }

        public override void OnNetworkDespawn()
        {
            if (_knockback == null) return;
            _knockback.Pushed -= OnPushed;
            _knockback.Steered -= OnSteered;
        }

        private void OnPushed(Vector2 velocity)
        {
            if (Mathf.Abs(velocity.x) > 0.01f) _movement.LockControl(_data.KnockbackControlLock);
        }

        /// <summary>A pull field holds the player: no steering while it acts, and the lock time after.</summary>
        private void OnSteered() => _movement.LockControl(_data.KnockbackControlLock);
    }
}
