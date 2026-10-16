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
        [SerializeField] private AbilityData _ability;

        private readonly NetworkVariable<ulong> _casterId = new();
        private readonly NetworkVariable<Vector2> _aim = new();

        private float _age;
        private ulong _pendingCasterId;
        private Vector2 _pendingAim;

        public AbilityData Ability => _ability;
        public AbilityTags Tags => _ability.Tags;

        /// <summary>NetworkObjectId of the player who cast it.</summary>
        /// <remarks>On the server this is valid from the moment the effect spawns.</remarks>
        public ulong CasterId => IsServer ? _pendingCasterId : _casterId.Value;

        /// <summary>Unit direction the effect was launched in (projectiles fly along it).</summary>
        public Vector2 Aim => IsServer ? _pendingAim : _aim.Value;

        /// <summary>Seconds since this PC spawned the effect.</summary>
        public float Age => _age;

        /// <summary>0 at spawn to 1 at the end of its lifetime (0 when it has none).</summary>
        public float Age01 => _ability.EffectLifetime > 0f ? Mathf.Clamp01(_age / _ability.EffectLifetime) : 0f;

        public override void OnNetworkSpawn()
        {
            _age = 0f;
            if (!IsServer) return;
            _casterId.Value = _pendingCasterId;
            _aim.Value = _pendingAim;
        }

        private void Update()
        {
            if (!IsSpawned) return;
            _age += Time.deltaTime;
            if (IsServer && _ability.EffectLifetime > 0f && _age >= _ability.EffectLifetime) NetworkObject.Despawn();
        }

        /// <summary>Server only, before Spawn: who cast it (player NetworkObjectId) and where it points.</summary>
        public void ServerSetup(ulong casterId, Vector2 aim)
        {
            _pendingCasterId = casterId;
            _pendingAim = aim;
        }
    }
}
