using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The owner's stamina. Only the owner spends and refills it, because it only gates the
    /// owner's own actions; the value is mirrored in an owner-write NetworkVariable so other
    /// players' HUDs can show it.
    /// </summary>
    public class PlayerStamina : NetworkBehaviour
    {
        [SerializeField] private PlayerStatsData _stats;

        private readonly NetworkVariable<float> _current = new(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private StaminaPool _pool;

        public float Current => _current.Value;
        public float Max => _stats.MaxStamina;

        /// <summary>Stamina from 0 to 1, readable on every client.</summary>
        public float Normalized => _current.Value / _stats.MaxStamina;

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (!IsOwner) return;

            _pool = new StaminaPool(_stats.MaxStamina);
            _current.Value = _pool.Current;
        }

        private void Update()
        {
            if (_pool == null) return;
            _pool.Tick(Time.deltaTime, _stats.StaminaRegenPerSecond);
            Publish();
        }

        /// <summary>Owner only: takes the cost if there is enough. False means the action must not happen.</summary>
        public bool TrySpend(float amount)
        {
            if (!IsOwner || !_pool.TrySpend(amount, _stats.RegenDelay)) return false;
            Publish();
            return true;
        }

        private void Publish()
        {
            if (!Mathf.Approximately(_current.Value, _pool.Current)) _current.Value = _pool.Current;
        }
    }
}
