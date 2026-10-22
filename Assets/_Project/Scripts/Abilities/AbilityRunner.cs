using System;
using GraceRun.Core.Settings;
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
            _input.AbilityHoldStarted += OnAbilityHoldStarted;
            _input.AbilityHoldReleased += OnAbilityHoldReleased;
            _loadout.SlotReplaced += ClearPrediction;
            if (!Application.isBatchMode) gameObject.AddComponent<AimPreviewView>().Setup(this, _loadout);
        }

        public override void OnNetworkDespawn()
        {
            if (_input != null)
            {
                _input.AbilityPressed -= OnAbilityPressed;
                _input.AbilityHoldStarted -= OnAbilityHoldStarted;
                _input.AbilityHoldReleased -= OnAbilityHoldReleased;
            }
            if (_loadout != null) _loadout.SlotReplaced -= ClearPrediction;
        }

        /// <summary>Cooldown left in <paramref name="slot"/>, 1 (just used) to 0 (ready); includes the owner's own prediction.</summary>
        public float CooldownRemaining01(int slot)
        {
            var ability = _loadout.Get(slot);
            if (ability == null || !IsSpawned) return 0f;
            return AbilityCooldown.Remaining01(ReadyAt(slot), CooldownFor(slot, ability), Now);
        }

        /// <summary>Seconds of cooldown left in <paramref name="slot"/> (0 when ready).</summary>
        public float CooldownSecondsRemaining(int slot)
        {
            var ability = _loadout.Get(slot);
            if (ability == null || !IsSpawned) return 0f;
            return CooldownRemaining01(slot) * CooldownFor(slot, ability);
        }

        private float CooldownFor(int slot, AbilityData ability)
        {
            var level = _loadout != null ? _loadout.Level(slot) : 1;
            return level >= 2 ? ability.Cooldown * ability.Level2CooldownMultiplier : ability.Cooldown;
        }

        private double ReadyAt(int slot)
        {
            var synced = _loadout.ReadyAt(slot);
            return IsOwner ? Math.Max(synced, _predictedReadyAt[slot]) : synced;
        }

        /// <summary>The slot being held to aim (Hold to aim on), or -1.</summary>
        public int HoldingSlot { get; private set; } = -1;

        /// <summary>The aim a key or stick press would use right now (for the aim preview).</summary>
        public KeyAimResult PreviewAim() => ResolveKeyAim();

        private void Update()
        {
            if (HoldingSlot >= 0 && (!enabled || _dodge.IsDodging)) HoldingSlot = -1;
        }

        private KeyAimResult ResolveKeyAim() =>
            KeyAim.Resolve(transform.position, _input.Move, _movement.Facing, _loadout.Catalog.AimAssist, GameSettings.AimAssist);

        private void OnAbilityHoldStarted(int slot)
        {
            if (!enabled || _dodge.IsDodging || _loadout.Get(slot) == null) return;
            if (AbilityCooldown.IsReady(ReadyAt(slot), Now)) HoldingSlot = slot;
        }

        private void OnAbilityHoldReleased(int slot)
        {
            if (HoldingSlot != slot) return;
            HoldingSlot = -1;
            OnAbilityPressed(slot, false);
        }

        private void OnAbilityPressed(int slot, bool atCursor)
        {
            if (!enabled || _dodge.IsDodging) return;
            var ability = _loadout.Get(slot);
            if (ability == null || !AbilityCooldown.IsReady(ReadyAt(slot), Now)) return;

            var cursor = Vector2.zero;
            var cursorAimed = atCursor && _input.TryGetCursorWorld(out cursor);
            var aimDistance = cursorAimed ? Vector2.Distance(transform.position, cursor) : 0f;
            Vector2 aim;
            if (cursorAimed)
            {
                aim = AbilityMath.CursorAim(transform.position, cursor, _movement.Facing);
            }
            else
            {
                var keyAim = ResolveKeyAim();
                aim = keyAim.Aim;
                cursorAimed = keyAim.Directed;
                aimDistance = keyAim.Distance;
            }
            if (ability.CasterAction != null && !ability.CasterAction.OwnerUse(this, ability, aim)) return;

            _predictedReadyAt[slot] = AbilityCooldown.NextReadyAt(Now, CooldownFor(slot, ability));
            UseAbilityRpc((byte)slot, aim, cursorAimed, aimDistance);
        }

        /// <summary>
        /// Owner to server. <paramref name="cursorAimed"/>: the effect goes straight along
        /// <paramref name="aim"/>; otherwise only its side counts and the ability's launch angle applies.
        /// <paramref name="aimDistance"/> is how far away the cursor was (0 for keys), for aim-point abilities.
        /// </summary>
        [Rpc(SendTo.Server)]
        private void UseAbilityRpc(byte slot, Vector2 aim, bool cursorAimed, float aimDistance)
        {
            var ability = _loadout.Get(slot);
            var tolerance = _loadout.Catalog.CooldownTolerance;
            if (!enabled || ability == null || !AbilityCooldown.IsReady(_loadout.ReadyAt(slot), Now, tolerance))
            {
                RejectedRpc(slot);
                return;
            }

            _loadout.ServerStartCooldown(slot, AbilityCooldown.NextReadyAt(Now, CooldownFor(slot, ability)));
            var power = _loadout.Level(slot) >= 2 ? ability.Level2PowerMultiplier : 1f;
            if (ability.EffectPrefab != null) SpawnEffect(ability, AbilityMath.SafeAim(aim), cursorAimed, aimDistance, power);
        }

        /// <summary>Server to owner: the use was refused, so drop the predicted cooldown.</summary>
        [Rpc(SendTo.Owner)]
        private void RejectedRpc(byte slot) => _predictedReadyAt[slot] = 0d;

        private void ClearPrediction(int slot)
        {
            if (slot >= 0 && slot < _predictedReadyAt.Length) _predictedReadyAt[slot] = 0d;
        }

        private void SpawnEffect(AbilityData ability, Vector2 aim, bool cursorAimed, float aimDistance, float powerMultiplier)
        {
            var side = AbilityMath.SideOf(aim.x);
            var elevation = cursorAimed ? AbilityMath.Elevation(aim) : 0f;
            var launchAngle = cursorAimed ? elevation : ability.LaunchAngle;
            var caster = (Vector2)transform.position;
            var position = caster + AbilityMath.AimedOffset(ability.SpawnOffset, side, elevation);
            if (ability.SpawnMode != AbilitySpawnMode.AtCaster)
            {
                position = AbilityMath.AimPoint(caster, aim, aimDistance, ability.Range);
                // A cursor inside the floor would bury the effect: keep it at the caster's height.
                if (Physics2D.OverlapPoint(position, Core.Layers.Mask(Core.Layers.GroundIndex)) != null) position.y = caster.y;
            }
            if (ability.SpawnMode == AbilitySpawnMode.AboveAimPoint) position.y += ability.DropHeight;

            for (var i = 0; i < ability.Count; i++)
            {
                var direction = ability.SpawnMode == AbilitySpawnMode.AboveAimPoint
                    ? AbilityMath.FanDirection(Vector2.down, ability.Spread, i, ability.Count)
                    : AbilityMath.LaunchDirection(side, launchAngle, ability.Spread, i, ability.Count);
                var effect = Instantiate(ability.EffectPrefab, position, Quaternion.identity);
                effect.ServerSetup(NetworkObjectId, direction, powerMultiplier);
                effect.NetworkObject.Spawn(true);
            }
        }
    }
}
