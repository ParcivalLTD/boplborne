using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Owner only: when a knockback lands (boss or friendly fire), input stops steering for a moment
    /// so the push actually carries the player instead of being braked away by the run deceleration.
    /// </summary>
    public class PlayerKnockbackLock : NetworkBehaviour
    {
        [SerializeField] private Knockback _knockback;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerMovementData _data;

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _knockback.Pushed += OnPushed;
        }

        public override void OnNetworkDespawn()
        {
            if (_knockback != null) _knockback.Pushed -= OnPushed;
        }

        private void OnPushed(Vector2 velocity)
        {
            if (Mathf.Abs(velocity.x) > 0.01f) _movement.LockControl(_data.KnockbackControlLock);
        }
    }
}
