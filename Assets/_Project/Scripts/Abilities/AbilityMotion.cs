using GraceRun.Combat;
using GraceRun.Core;
using GraceRun.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Movement abilities on the Player prefab, run by the owner the moment the button is pressed
    /// (movement is owner-authoritative): Blink teleports along the aim, stopping short of solid
    /// ground (it passes through walls summoned by abilities, ledges and teammates); Dash drives
    /// the body along the aim for a moment with gravity off.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(NetworkTransform), typeof(AbilityLoadout))]
    public class AbilityMotion : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Health _health;
        [SerializeField, Min(0f), Tooltip("Blink stops this far before solid ground.")]
        private float _blinkSkin = 0.1f;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private Rigidbody2D _body;
        private NetworkTransform _networkTransform;
        private AbilityLoadout _loadout;
        private ContactFilter2D _groundFilter;
        private AbilityData _dashing;
        private Vector2 _dashVelocity;
        private float _dashTimer;

        /// <summary>True while a dash owns the body (owner).</summary>
        public bool IsDashing => _dashing != null;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _networkTransform = GetComponent<NetworkTransform>();
            _loadout = GetComponent<AbilityLoadout>();
            _groundFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.GroundIndex),
                useTriggers = false,
            };
        }

        public override void OnNetworkDespawn() => EndDash();

        private void FixedUpdate()
        {
            if (!IsOwner || _dashing == null) return;
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0f || _health.IsDead)
            {
                EndDash();
                return;
            }
            _body.linearVelocity = _dashVelocity;
        }

        /// <summary>Owner only. Teleports up to the ability's range along <paramref name="aim"/>; false when there is no room.</summary>
        public bool TryBlink(AbilityData ability, Vector2 aim)
        {
            if (!IsOwner || _health.IsDead) return false;
            aim = AbilityMath.SafeAim(aim);
            var distance = ability.Range * PowerFor(ability);
            var count = _body.Cast(aim, _groundFilter, _hits, distance);
            for (var i = 0; i < count; i++) distance = Mathf.Min(distance, _hits[i].distance - _blinkSkin);
            if (distance <= _blinkSkin) return false;

            EndDash();
            var target = _body.position + aim * distance;
            _body.position = target;
            _body.linearVelocity = aim * ability.ForceSpeed;
            _networkTransform.Teleport(target, transform.rotation, transform.localScale);
            return true;
        }

        /// <summary>Owner only. Drives the body along <paramref name="aim"/> at the ability's force speed for its max flight time.</summary>
        public bool TryDash(AbilityData ability, Vector2 aim)
        {
            if (!IsOwner || _health.IsDead || ability.MaxFlightTime <= 0f) return false;
            _dashing = ability;
            _dashTimer = ability.MaxFlightTime;
            _dashVelocity = AbilityMath.SafeAim(aim) * (ability.ForceSpeed * PowerFor(ability));
            _movement.ControlSuspended = true;
            _movement.GravitySuspended = true;
            _body.linearVelocity = _dashVelocity;
            return true;
        }

        private void EndDash()
        {
            if (_dashing == null) return;
            _body.linearVelocity = _dashVelocity * _dashing.Size.x;
            _dashing = null;
            if (_movement == null) return;
            _movement.ControlSuspended = false;
            _movement.GravitySuspended = false;
        }

        private float PowerFor(AbilityData ability)
        {
            for (var i = 0; i < AbilityLoadout.SlotCount; i++)
                if (_loadout.Get(i) == ability && _loadout.Level(i) >= 2) return ability.PowerMultiplierAt(_loadout.Level(i));
            return 1f;
        }
    }
}
