using System;
using GraceRun.Run;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// A player's 3 ability slots and their cooldowns, as a server-write NetworkList so every PC
    /// (and late joiners) see the same loadout. <see cref="AbilityRunner"/> uses it; the HUD shows it.
    /// </summary>
    public class AbilityLoadout : NetworkBehaviour
    {
        public const int SlotCount = 3;

        [SerializeField] private AbilityCatalog _catalog;

        private readonly NetworkList<AbilitySlotState> _slots = new(
            null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>Raised on every PC when a slot gets a different ability (or is cleared).</summary>
        public event Action<int> SlotReplaced;

        public AbilityCatalog Catalog => _catalog;

        public override void OnNetworkSpawn()
        {
            _slots.OnListChanged += OnListChanged;
            if (!IsServer) return;
            while (_slots.Count < SlotCount) _slots.Add(AbilitySlotState.Empty);
        }

        public override void OnNetworkDespawn()
        {
            _slots.OnListChanged -= OnListChanged;
        }

        /// <summary>The ability in <paramref name="slot"/>, or null while it is empty.</summary>
        public AbilityData Get(int slot) => IsValid(slot) ? _catalog.Get(_slots[slot].AbilityId) : null;

        /// <summary>Server time the ability in <paramref name="slot"/> is ready again.</summary>
        public double ReadyAt(int slot) => IsValid(slot) ? _slots[slot].ReadyAt : 0d;

        /// <summary>Upgrade level of ability in <paramref name="slot"/> (1 = normal, 2 = upgraded).</summary>
        public int Level(int slot) => IsValid(slot) ? _slots[slot].Level : 1;

        /// <summary>True when ability in <paramref name="slot"/> can be upgraded to Level 2.</summary>
        public bool CanUpgrade(int slot) =>
            IsValid(slot) && _slots[slot].AbilityId != AbilityCatalog.None && _slots[slot].Level < RestStopRules.MaxAbilityLevel;

        /// <summary>Server only: puts <paramref name="ability"/> (null = empty) in a slot, ready at once at level 1.</summary>
        public void ServerSet(int slot, AbilityData ability)
        {
            if (!IsServer || !IsValid(slot)) return;
            _slots[slot] = new AbilitySlotState(_catalog.IdOf(ability), 0d, 1);
        }

        /// <summary>Server only: upgrades ability in <paramref name="slot"/> to Level 2.</summary>
        public void ServerUpgrade(int slot)
        {
            if (!IsServer || !CanUpgrade(slot)) return;
            _slots[slot] = new AbilitySlotState(_slots[slot].AbilityId, _slots[slot].ReadyAt, 2);
            SlotReplaced?.Invoke(slot);
        }

        /// <summary>Server only: empties all slots (new run in the hub).</summary>
        public void ServerClear()
        {
            if (!IsServer) return;
            for (var i = 0; i < _slots.Count; i++) _slots[i] = AbilitySlotState.Empty;
        }

        /// <summary>Server only: the ability in <paramref name="slot"/> is next usable at <paramref name="readyAt"/>.</summary>
        public void ServerStartCooldown(int slot, double readyAt)
        {
            if (!IsServer || !IsValid(slot)) return;
            _slots[slot] = new AbilitySlotState(_slots[slot].AbilityId, readyAt, _slots[slot].Level);
        }

        /// <summary>Debug menu: put ability <paramref name="abilityId"/> (catalog id, -1 clears) in a slot.</summary>
        [Rpc(SendTo.Server)]
        public void DebugSetRpc(int slot, int abilityId) => ServerSet(slot, _catalog.Get(abilityId));

        private bool IsValid(int slot) => slot >= 0 && slot < _slots.Count;

        private void OnListChanged(NetworkListEvent<AbilitySlotState> change)
        {
            if (change.Type == NetworkListEvent<AbilitySlotState>.EventType.Value &&
                change.PreviousValue.AbilityId == change.Value.AbilityId &&
                change.PreviousValue.Level == change.Value.Level) return;
            SlotReplaced?.Invoke(change.Index);
        }
    }
}
