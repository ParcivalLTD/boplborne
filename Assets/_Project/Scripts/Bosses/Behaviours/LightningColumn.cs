using GraceRun.Combat;
using GraceRun.Core.Audio;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Vertical lightning pillar summoned by The Storm Wyrm. Displays a warning column
    /// during wind-up, strikes down across the floor, arms a victim-side Hitbox, and despawns.
    /// </summary>
    public class LightningColumn : NetworkBehaviour
    {
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private AudioCue _strikeCue;

        private readonly NetworkVariable<float> _windup = new(
            0.8f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _activeTime = new(
            0.25f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<int> _damage = new(
            20, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _isStriking = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Vector2 _knockback = new(0f, 8f);
        private float _age;

        public void ServerConfigure(float windup, float activeTime, int damage, Vector2 knockback)
        {
            _windup.Value = windup;
            _activeTime.Value = activeTime;
            _damage.Value = damage;
            _knockback = knockback;
        }

        public override void OnNetworkSpawn()
        {
            _age = 0f;
            _isStriking.OnValueChanged += OnStrikingChanged;
            UpdateVisuals();
        }

        public override void OnNetworkDespawn()
        {
            _isStriking.OnValueChanged -= OnStrikingChanged;
            if (_hitbox != null) _hitbox.Disarm();
        }

        private void Update()
        {
            if (!IsSpawned) return;
            _age += Time.deltaTime;
            UpdateVisuals();

            if (!IsServer) return;

            if (_age >= _windup.Value && !_isStriking.Value)
            {
                _isStriking.Value = true;
                if (_hitbox != null) _hitbox.Arm(_damage.Value, _knockback);
            }

            if (_age >= _windup.Value + _activeTime.Value || BossBrain.AreAllDefeated)
            {
                NetworkObject.Despawn();
            }
        }

        private void OnStrikingChanged(bool previous, bool current)
        {
            if (current && _strikeCue != null)
            {
                AudioCuePlayer.Play2D(_strikeCue);
            }
        }

        private void UpdateVisuals()
        {
            if (_renderer == null) return;

            if (_isStriking.Value)
            {
                _renderer.color = new Color(0.9f, 1f, 1f, 0.95f);
            }
            else
            {
                var progress = Mathf.Clamp01(_age / Mathf.Max(0.01f, _windup.Value));
                var pulse = 0.3f + 0.3f * Mathf.Sin(_age * (6f + 14f * progress) * Mathf.PI);
                _renderer.color = new Color(0.35f, 0.85f, 0.95f, pulse);
            }
        }
    }
}
