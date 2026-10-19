using System;
using GraceRun.Abilities;
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
    /// A keyboard press aims the swing at the mouse cursor (the hitbox turns toward it); a gamepad
    /// press swings the facing way. A dodge cancels the swing; hitstop pauses it.
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
        private Transform _hitboxTransform;
        private Vector2 _aim = Vector2.right;
        private bool _aimAtCursor;

        /// <summary>Raised on every PC when a swing starts: hit index, side (+1 or -1).</summary>
        public event Action<int, float> SwingStarted;

        public bool IsSwinging => _combo != null && _combo.Phase != SwingPhase.Idle;

        /// <summary>Unit direction of the current (or last) swing, on every PC.</summary>
        public Vector2 Aim => _aim;

        private void Awake()
        {
            _combo = new LightAttackCombo(_data.Hits, _data.InputBuffer, _data.ChainWindow);
            _hitboxTransform = _hitboxShape.transform;
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
        private void SwingRpc(byte hitIndex, Vector2 aim)
        {
            _aim = AbilityMath.SafeAim(aim);
            _combo.ForceStart(hitIndex);
        }

        /// <summary>Owner to everyone else: the swing was cancelled (dodge).</summary>
        [Rpc(SendTo.NotOwner)]
        private void CancelSwingRpc() => _combo.Cancel();

        private void OnAttackPressed(bool atCursor)
        {
            _aimAtCursor = atCursor;
            _combo.Press();
        }

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
                _aim = OwnerAim();
                _movement.SpeedMultiplier = _data.MoveSpeedMultiplier;
                SwingRpc((byte)hitIndex, _aim);
            }
            SwingStarted?.Invoke(hitIndex, AbilityMath.SideOf(_aim.x));
        }

        private Vector2 OwnerAim()
        {
            if (_aimAtCursor && _input.TryGetCursorWorld(out var cursor))
                return AbilityMath.CursorAim(transform.position, cursor, _movement.Facing);
            return new Vector2(AbilityMath.SideOf(_movement.Facing), 0f);
        }

        private void OnActiveStarted(int hitIndex)
        {
            var hit = _data.Hits[hitIndex];
            _hitboxTransform.localRotation = Quaternion.Euler(0f, 0f, ActionAim.HitboxAngle(_aim));
            _hitboxShape.size = hit.HitboxSize;
            _hitboxShape.offset = ActionAim.HitboxLocalOffset(hit.HitboxOffset, _aim);
            _hitbox.Arm(hit.Damage, ActionAim.Knockback(hit.Knockback, _aim));
        }

        private void OnActiveEnded() => _hitbox.Disarm();

        private void OnSwingEnded()
        {
            if (IsOwner) _movement.SpeedMultiplier = 1f;
        }
    }
}
