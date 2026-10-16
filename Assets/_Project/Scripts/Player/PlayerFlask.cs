using System;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The healing flask. The owner presses Flask; the server checks <see cref="FlaskRules"/>,
    /// heals and spends a charge. Charges are a server-write NetworkVariable so every HUD shows
    /// the pips. Refilled by <see cref="ServerRefill"/> (fight start until rest stops exist).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerFlask : NetworkBehaviour
    {
        [SerializeField] private PlayerStatsData _stats;
        [SerializeField] private PlayerInputReader _input;

        private readonly NetworkVariable<int> _charges = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Health _health;

        /// <summary>Raised on every PC when this player drinks, for cosmetics.</summary>
        public event Action Drank;

        public int Charges => _charges.Value;
        public int MaxCharges => _stats.FlaskCharges;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) _charges.Value = _stats.FlaskCharges;
            if (IsOwner) _input.FlaskPressed += OnFlaskPressed;
        }

        public override void OnNetworkDespawn()
        {
            if (_input != null) _input.FlaskPressed -= OnFlaskPressed;
        }

        /// <summary>Server only: back to full charges.</summary>
        public void ServerRefill()
        {
            if (IsServer) _charges.Value = _stats.FlaskCharges;
        }

        private void OnFlaskPressed() => DrinkRpc();

        [Rpc(SendTo.Server)]
        private void DrinkRpc()
        {
            if (!FlaskRules.CanDrink(_charges.Value, _health.Current, _health.Max)) return;
            _charges.Value--;
            _health.ServerSetCurrent(FlaskRules.HealedHP(_health.Current, _health.Max, _stats.FlaskHeal));
            DrankRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void DrankRpc() => Drank?.Invoke();
    }
}
