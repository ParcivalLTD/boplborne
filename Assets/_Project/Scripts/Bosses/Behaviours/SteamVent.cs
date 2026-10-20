using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core.Audio;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Scalding steam vent eruption summoned by The Clockwork Colossus.
    /// Hisses with steam during wind-up, erupts high into the air with vertical lift knockback,
    /// arms a victim-side Hitbox, and despawns.
    /// </summary>
    public class SteamVent : NetworkBehaviour
    {
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private AudioCue _steamCue;

        private static readonly List<SteamVent> Active = new();

        private readonly NetworkVariable<float> _windup = new(
            0.85f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _activeTime = new(
            0.25f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _damage = new(
            20, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _isErupting = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Vector2 _knockback = new(0f, 12f);
        private float _age;

        public static int ActiveCount => Active.Count;

        public void ServerConfigure(float windup, float activeTime, int damage, Vector2 knockback)
        {
            _windup.Value = windup;
            _activeTime.Value = activeTime;
            _damage.Value = damage;
            _knockback = knockback;
        }

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _age = 0f;
            _isErupting.OnValueChanged += OnEruptingChanged;
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            _isErupting.OnValueChanged -= OnEruptingChanged;
            if (_hitbox != null) _hitbox.Disarm();
        }

        private void Update()
        {
            if (!IsSpawned) return;
            _age += Time.deltaTime;
            UpdateVisuals();

            if (!IsServer) return;

            if (_age >= _windup.Value && !_isErupting.Value)
            {
                _isErupting.Value = true;
                if (_hitbox != null) _hitbox.Arm(_damage.Value, _knockback);
            }

            if (_age >= _windup.Value + _activeTime.Value || BossBrain.AreAllDefeated)
            {
                NetworkObject.Despawn();
            }
        }

        private void OnEruptingChanged(bool previous, bool current)
        {
            if (current && _steamCue != null)
            {
                AudioCuePlayer.Play2D(_steamCue);
            }
        }

        private void UpdateVisuals()
        {
            if (_renderer == null) return;

            if (_isErupting.Value)
            {
                _renderer.color = new Color(0.95f, 0.95f, 1f, 0.95f);
            }
            else
            {
                var progress = Mathf.Clamp01(_age / Mathf.Max(0.01f, _windup.Value));
                var pulse = 0.25f + 0.25f * Mathf.Sin(_age * (8f + 12f * progress) * Mathf.PI);
                _renderer.color = new Color(0.85f, 0.65f, 0.45f, pulse);
            }
        }

        /// <summary>Server only: despawns all active steam vents at once (arena swap, fight reset).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var vent = Active[i];
                if (vent != null && vent.IsServer && vent.IsSpawned)
                {
                    vent.NetworkObject.Despawn();
                }
            }
            Active.Clear();
        }
    }
}
