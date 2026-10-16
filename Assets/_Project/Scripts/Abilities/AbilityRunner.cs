using System;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Uses the abilities in the <see cref="AbilityLoadout"/>. The owner checks the cooldown at once
    /// (and the HUD shows it straight away), then asks the server; the server re-checks, starts the
    /// synced cooldown and spawns the effect. Disabled while the player is a ghost.
    /// </summary>
    [RequireComponent(typeof(AbilityLoadout))]
    public class AbilityRunner : NetworkBehaviour
    {
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerDodge _dodge;

        private readonly double[] _predictedReadyAt = new double[AbilityLoadout.SlotCount];
        private AbilityLoadout _loadout;

        private double Now => NetworkManager.ServerTime.Time;

        private void Awake()
        {
            _loadout = GetComponent<AbilityLoadout>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _input.AbilityPressed += OnAbilityPressed;
            _loadout.SlotReplaced += ClearPrediction;
        }

        public override void OnNetworkDespawn()
        {
            if (_input != null) _input.AbilityPressed -= OnAbilityPressed;
            if (_loadout != null) _loadout.SlotReplaced -= ClearPrediction;
        }

        /// <summary>Cooldown left in <paramref name="slot"/>, 1 (just used) to 0 (ready); includes the owner's own prediction.</summary>
        public float CooldownRemaining01(int slot)
        {
            var ability = _loadout.Get(slot);
            if (ability == null || !IsSpawned) return 0f;
            return AbilityCooldown.Remaining01(ReadyAt(slot), ability.Cooldown, Now);
        }

        private double ReadyAt(int slot)
        {
            var synced = _loadout.ReadyAt(slot);
            return IsOwner ? Math.Max(synced, _predictedReadyAt[slot]) : synced;
        }

        private void OnAbilityPressed(int slot)
        {
            if (!enabled || _dodge.IsDodging) return;
            var ability = _loadout.Get(slot);
            if (ability == null || !AbilityCooldown.IsReady(ReadyAt(slot), Now)) return;

            var aim = AbilityMath.CasterAim(_movement.Facing, _input.Move.y);
            if (ability.CasterAction != null && !ability.CasterAction.OwnerUse(this, ability, aim)) return;

            _predictedReadyAt[slot] = AbilityCooldown.NextReadyAt(Now, ability.Cooldown);
            UseAbilityRpc((byte)slot, aim);
        }

        [Rpc(SendTo.Server)]
        private void UseAbilityRpc(byte slot, Vector2 aim)
        {
            var ability = _loadout.Get(slot);
            var tolerance = _loadout.Catalog.CooldownTolerance;
            if (!enabled || ability == null || !AbilityCooldown.IsReady(_loadout.ReadyAt(slot), Now, tolerance))
            {
                RejectedRpc(slot);
                return;
            }

            _loadout.ServerStartCooldown(slot, AbilityCooldown.NextReadyAt(Now, ability.Cooldown));
            if (ability.EffectPrefab != null) SpawnEffect(ability, aim);
        }

        /// <summary>Server to owner: the use was refused, so drop the predicted cooldown.</summary>
        [Rpc(SendTo.Owner)]
        private void RejectedRpc(byte slot) => _predictedReadyAt[slot] = 0d;

        private void ClearPrediction(int slot)
        {
            if (slot >= 0 && slot < _predictedReadyAt.Length) _predictedReadyAt[slot] = 0d;
        }

        private void SpawnEffect(AbilityData ability, Vector2 aim)
        {
            var side = aim.x < 0f ? -1f : 1f;
            var offset = new Vector2(ability.SpawnOffset.x * side, ability.SpawnOffset.y);
            var position = (Vector2)transform.position + offset;
            for (var i = 0; i < ability.Count; i++)
            {
                var direction = AbilityMath.LaunchDirection(side, ability.LaunchAngle, ability.Spread, i, ability.Count);
                var effect = Instantiate(ability.EffectPrefab, position, Quaternion.identity);
                effect.ServerSetup(NetworkObjectId, direction);
                effect.NetworkObject.Spawn(true);
            }
        }
    }
}
