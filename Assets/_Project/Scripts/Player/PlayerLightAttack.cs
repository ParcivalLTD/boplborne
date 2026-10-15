using System;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The 3-hit light attack. The owner reads the press, pays stamina and detects hits through an
    /// attacker-side <see cref="Hitbox"/>; the server resolves them. Other PCs get the swing through
    /// <see cref="SwingRpc"/> and replay its timing for visuals only (their hitbox never reports).
    /// A dodge cancels the swing; hitstop pauses it.
    /// </summary>
    public class PlayerLightAttack : NetworkBehaviour
    {
        [SerializeField] private LightAttackData _data;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerStamina _stamina;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private BoxCollider2D _hitboxShape;
        [SerializeField] private Hitstop _hitstop;

        private LightAttackCombo _combo;
        private float _facing = 1f;

        /// <summary>Raised on every PC when a swing starts: hit index, facing (+1 or -1).</summary>
        public event Action<int, float> SwingStarted;

        public bool IsSwinging => _combo != null && _combo.Phase != SwingPhase.Idle;

        private void Awake()
        {
            _combo = new LightAttackCombo(_data.Hits, _data.InputBuffer, _data.ChainWindow);
        }

        public override void OnNetworkSpawn()
        {
            _combo.SwingStarted += OnSwingStarted;
            _combo.ActiveStarted += OnActiveStarted;
            _combo.ActiveEnded += OnActiveEnded;
            _combo.SwingEnded += OnSwingEnded;
            if (!IsOwner) return;

            _combo.CanStart = CanStart;
            _input.AttackPressed += OnAttackPressed;
            _dodge.Dodged += OnDodged;
        }

        public override void OnNetworkDespawn()
        {
            _combo.Cancel();
            _combo.SwingStarted -= OnSwingStarted;
            _combo.ActiveStarted -= OnActiveStarted;
            _combo.ActiveEnded -= OnActiveEnded;
            _combo.SwingEnded -= OnSwingEnded;
            if (_input != null) _input.AttackPressed -= OnAttackPressed;
            if (_dodge != null) _dodge.Dodged -= OnDodged;
        }

        /// <summary>Disabled (ghost): the swing in progress stops and nothing new starts.</summary>
        private void OnDisable() => _combo?.Cancel();

        private void FixedUpdate()
        {
            if (IsSpawned && !_hitstop.IsFrozen) _combo.Tick(Time.fixedDeltaTime);
        }

        /// <summary>Owner to everyone else: replay this swing locally.</summary>
        [Rpc(SendTo.NotOwner)]
        private void SwingRpc(byte hitIndex, sbyte facing)
        {
            _facing = facing;
            _combo.ForceStart(hitIndex);
        }

        /// <summary>Owner to everyone else: the swing was cancelled (dodge).</summary>
        [Rpc(SendTo.NotOwner)]
        private void CancelSwingRpc() => _combo.Cancel();

        private void OnAttackPressed() => _combo.Press();

        private bool CanStart(int hitIndex) =>
            enabled && !_dodge.IsDodging && _stamina.TrySpend(_data.Hits[hitIndex].StaminaCost);

        private void OnDodged()
        {
            if (!IsSwinging) return;
            _combo.Cancel();
            CancelSwingRpc();
        }

        private void OnSwingStarted(int hitIndex)
        {
            if (IsOwner)
            {
                _facing = _movement.Facing;
                _movement.SpeedMultiplier = _data.MoveSpeedMultiplier;
                SwingRpc((byte)hitIndex, (sbyte)_facing);
            }
            SwingStarted?.Invoke(hitIndex, _facing);
        }

        private void OnActiveStarted(int hitIndex)
        {
            var hit = _data.Hits[hitIndex];
            _hitboxShape.size = hit.HitboxSize;
            _hitboxShape.offset = new Vector2(hit.HitboxOffset.x * _facing, hit.HitboxOffset.y);
            _hitbox.Arm(hit.Damage, new Vector2(hit.Knockback.x * _facing, hit.Knockback.y));
        }

        private void OnActiveEnded() => _hitbox.Disarm();

        private void OnSwingEnded()
        {
            if (IsOwner) _movement.SpeedMultiplier = 1f;
        }
    }
}
