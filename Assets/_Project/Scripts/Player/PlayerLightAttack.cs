using System;
using GraceRun.Abilities;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The player's weapon attacks: the light combo, Heavy, and the aerial / up / down swings of the
    /// equipped <see cref="PlayerWeapon"/> (Fists by default). The owner reads the press, pays stamina and detects hits through an
    /// attacker-side <see cref="Hitbox"/>; the server resolves them. Other PCs get the swing through
    /// <see cref="SwingRpc"/> and replay its timing for visuals only (their hitbox never reports).
    /// A keyboard press aims the swing at the mouse cursor (the hitbox turns toward it); a gamepad
    /// press swings the facing way. A dodge cancels the swing; hitstop pauses it.
    /// </summary>
    public class PlayerLightAttack : NetworkBehaviour
    {
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerStamina _stamina;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private BoxCollider2D _hitboxShape;
        [SerializeField] private Hitstop _hitstop;

        private LightAttackCombo _combo;
        private WeaponMoveSet _moves;
        private float _moveSpeed = 1f;
        private Vector2 _swingDirection = Vector2.right;
        private Transform _hitboxTransform;
        private Vector2 _aim = Vector2.right;
        private bool _aimAtCursor;

        /// <summary>Raised on every PC when a swing starts: hit index, side (+1 or -1).</summary>
        public event Action<int, float> SwingStarted;

        public bool IsSwinging => _combo != null && _combo.Phase != SwingPhase.Idle;

        /// <summary>Unit direction of the current (or last) swing, on every PC.</summary>
        public Vector2 Aim => _aim;

        /// <summary>Unit direction the weapon points during the current (or last) swing: up / down for those moves, else the aim.</summary>
        public Vector2 SwingDirection => _swingDirection;

        public SwingPhase Phase => _combo.Phase;

        /// <summary>True for the last light hit and for Heavy: the big swings that get the heavy sound.</summary>
        public bool IsFinisher(int hitIndex) => _moves.IsFinisher(hitIndex);

        private void Awake()
        {
            _combo = new LightAttackCombo(Array.Empty<LightAttackHit>(), 0f, 0f);
            Equip(_weapon.Catalog.Get(0));
            _hitboxTransform = _hitboxShape.transform;
        }

        public override void OnNetworkSpawn()
        {
            _combo.SwingStarted += OnSwingStarted;
            _combo.ActiveStarted += OnActiveStarted;
            _combo.ActiveEnded += OnActiveEnded;
            _combo.SwingEnded += OnSwingEnded;
            _weapon.Changed += Equip;
            Equip(_weapon.Current);
            if (!IsOwner) return;

            _combo.CanStart = CanStart;
            _input.AttackPressed += OnAttackPressed;
            _input.HeavyPressed += OnHeavyPressed;
            _dodge.Dodged += OnDodged;
        }

        public override void OnNetworkDespawn()
        {
            _combo.Cancel();
            _combo.SwingStarted -= OnSwingStarted;
            _combo.ActiveStarted -= OnActiveStarted;
            _combo.ActiveEnded -= OnActiveEnded;
            _combo.SwingEnded -= OnSwingEnded;
            _weapon.Changed -= Equip;
            if (_input != null)
            {
                _input.AttackPressed -= OnAttackPressed;
                _input.HeavyPressed -= OnHeavyPressed;
            }
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

        private void Equip(WeaponData weapon)
        {
            _moves = weapon.BuildMoves();
            _moveSpeed = weapon.MoveSpeedMultiplier;
            _combo.Configure(_moves.Hits, weapon.InputBuffer, weapon.ChainWindow, _moves.ComboLength);
        }

        private void OnAttackPressed(bool atCursor) => Press(atCursor, false);

        private void OnHeavyPressed(bool atCursor) => Press(atCursor, true);

        private void Press(bool atCursor, bool heavy)
        {
            _aimAtCursor = atCursor;
            var move = WeaponMoves.Select(heavy, _movement.IsGrounded, _input.Move.y, _weapon.Catalog.DirectionThreshold);
            if (move == WeaponMove.Light) _combo.Press();
            else _combo.PressMove(_moves.IndexFor(move));
        }

        private bool CanStart(int hitIndex) =>
            enabled && !_dodge.IsDodging && _stamina.TrySpend(_moves.Hits[hitIndex].StaminaCost);

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
                if (IsVertical(hitIndex)) _aim = new Vector2(AbilityMath.SideOf(_aim.x), 0f);
                _movement.SpeedMultiplier = _moveSpeed;
                SwingRpc((byte)hitIndex, _aim);
            }
            _swingDirection = SwingDirectionOf(hitIndex);
            SwingStarted?.Invoke(hitIndex, AbilityMath.SideOf(_aim.x));
        }

        private Vector2 OwnerAim()
        {
            if (_aimAtCursor && _input.TryGetCursorWorld(out var cursor))
                return AbilityMath.CursorAim(transform.position, cursor, _movement.Facing);
            return new Vector2(AbilityMath.SideOf(_movement.Facing), 0f);
        }

        private bool IsVertical(int hitIndex) => _moves.MoveOf(hitIndex) is WeaponMove.Up or WeaponMove.Down;

        private Vector2 SwingDirectionOf(int hitIndex) => _moves.MoveOf(hitIndex) switch
        {
            WeaponMove.Up => Vector2.up,
            WeaponMove.Down => Vector2.down,
            _ => _aim,
        };

        private void OnActiveStarted(int hitIndex)
        {
            var hit = _moves.Hits[hitIndex];
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
