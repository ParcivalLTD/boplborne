using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Root of every ability effect prefab (a NetworkObject the server spawns). Knows its ability
    /// (and so its tags, for combos), who cast it and where it was aimed, and despawns itself on the
    /// server when its lifetime runs out. Effect behaviours sit next to it on the prefab.
    /// </summary>
    public class AbilityEffect : NetworkBehaviour
    {
        private static readonly List<AbilityEffect> _active = new();

        [SerializeField] private AbilityData _ability;
        [SerializeField, Tooltip("Asked when this effect meets another tagged thing. Empty = no combos.")]
        private ComboRules _combos;

        private readonly NetworkVariable<ulong> _casterId = new();
        private readonly NetworkVariable<Vector2> _aim = new();
        private readonly NetworkVariable<float> _powerMultiplier = new(
            1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float _age;
        private ulong _pendingCasterId;
        private Vector2 _pendingAim;
        private float _pendingPowerMultiplier = 1f;

        /// <summary>Server only: raised when the effect runs out its lifetime, just before it despawns (a collapse, a last blast).</summary>
        public event System.Action<AbilityEffect> Expired;

        public AbilityData Ability => _ability;
        public AbilityTags Tags => _ability.Tags;
        public ComboRules Combos => _combos;

        /// <summary>NetworkObjectId of the player who cast it.</summary>
        /// <remarks>On the server this is valid from the moment the effect spawns.</remarks>
        public ulong CasterId => IsServer ? _pendingCasterId : _casterId.Value;

        /// <summary>Unit direction the effect was launched in (projectiles fly along it).</summary>
        public Vector2 Aim => IsServer ? _pendingAim : _aim.Value;

        /// <summary>Power scale from ability level: 1 for level 1, Level2PowerMultiplier for level 2.</summary>
        public float PowerMultiplier => IsServer ? _pendingPowerMultiplier : _powerMultiplier.Value;

        /// <summary>Seconds since this PC spawned the effect.</summary>
        public float Age => _age;

        /// <summary>0 at spawn to 1 at the end of its lifetime (0 when it has none).</summary>
        public float Age01 => _ability.EffectLifetime > 0f ? Mathf.Clamp01(_age / _ability.EffectLifetime) : 0f;

        public override void OnNetworkSpawn()
        {
            _active.Add(this);
            _age = 0f;
            if (!IsServer) return;
            _casterId.Value = _pendingCasterId;
            _aim.Value = _pendingAim;
            _powerMultiplier.Value = _pendingPowerMultiplier;
        }

        public override void OnNetworkDespawn()
        {
            _active.Remove(this);
        }

        private void Update()
        {
            if (!IsSpawned) return;
            _age += Time.deltaTime;
            if (IsServer && _ability.EffectLifetime > 0f && _age >= _ability.EffectLifetime)
            {
                Expired?.Invoke(this);
                NetworkObject.Despawn();
            }
        }

        /// <summary>Server only, before Spawn: who cast it (player NetworkObjectId), where it points and its power scale.</summary>
        public void ServerSetup(ulong casterId, Vector2 aim, float powerMultiplier = 1f)
        {
            _pendingCasterId = casterId;
            _pendingAim = aim;
            _pendingPowerMultiplier = powerMultiplier;
        }

        /// <summary>Server only: despawns every active ability effect (used when swapping arenas).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var effect = _active[i];
                if (effect != null && effect.IsServer && effect.IsSpawned) effect.NetworkObject.Despawn();
            }
        }
    }
}
