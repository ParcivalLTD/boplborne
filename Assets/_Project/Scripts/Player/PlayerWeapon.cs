using System;
using GraceRun.Run;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The weapon a player carries, as a server-write NetworkVariable (a <see cref="WeaponCatalog"/>
    /// id) so every PC, and late joiners, swing the same attacks. A player picks through
    /// <see cref="SelectRpc"/>; the server only accepts it where <see cref="WeaponRules.CanChange"/> says so.
    /// </summary>
    public class PlayerWeapon : NetworkBehaviour
    {
        [SerializeField] private WeaponCatalog _catalog;

        private readonly NetworkVariable<byte> _id = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>Raised on every PC when the weapon changes.</summary>
        public event Action<WeaponData> Changed;

        public WeaponCatalog Catalog => _catalog;
        public int Id => _id.Value;
        public WeaponData Current => _catalog.Get(_id.Value);

        public override void OnNetworkSpawn() => _id.OnValueChanged += OnIdChanged;

        public override void OnNetworkDespawn() => _id.OnValueChanged -= OnIdChanged;

        /// <summary>Asks the server to equip catalog weapon <paramref name="id"/>.</summary>
        [Rpc(SendTo.Server)]
        public void SelectRpc(byte id)
        {
            var loop = FightLoop.Instance;
            var run = RunManager.Instance;
            if (loop == null || run == null || !_catalog.IsValid(id)) return;
            if (WeaponRules.CanChange(loop.Phase, run.InHub)) _id.Value = id;
        }

        /// <summary>Server only: back to Fists (new run in the hub).</summary>
        public void ServerReset()
        {
            if (IsServer) _id.Value = 0;
        }

        private void OnIdChanged(byte previous, byte current) => Changed?.Invoke(_catalog.Get(current));
    }
}
