using GraceRun.Abilities.Effects;
using GraceRun.Combat;
using GraceRun.Core;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// The grapple, on the Player prefab. The owner casts the hook along the aim: a wall, the boss
    /// or an ability object it may latch onto (a Movement → Summon combo rule) pulls the caster to
    /// the hit point; other ability objects are passed through; a living teammate is yanked toward the caster
    /// instead (the server sends that push to the teammate's owner). The rope is synced through
    /// owner-write NetworkVariables so every PC can draw it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(AbilityLoadout))]
    public class GrappleHook : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private PlayerMovementData _movementData;
        [SerializeField] private Health _health;
        [SerializeField, Tooltip("Decides which ability objects (summons) the hook can latch onto.")]
        private ComboRules _combos;

        private readonly NetworkVariable<Vector2> _anchor = new(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _attached = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];
        private Rigidbody2D _body;
        private AbilityLoadout _loadout;
        private ContactFilter2D _filter;
        private AbilityData _pulling;
        private float _timer;
        private bool _ropeOnly;

        /// <summary>True while the rope is out (every PC).</summary>
        public bool Attached => _attached.Value;

        /// <summary>World point the rope is fixed to (every PC).</summary>
        public Vector2 Anchor => _anchor.Value;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _loadout = GetComponent<AbilityLoadout>();
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.GroundIndex) | Layers.Mask(Layers.BossIndex) |
                            Layers.Mask(Layers.PlayerIndex) | Layers.Mask(Layers.AbilityIndex),
                useTriggers = false,
            };
        }

        public override void OnNetworkDespawn() => _pulling = null;

        private void FixedUpdate()
        {
            if (!IsOwner || _pulling == null) return;

            _timer += Time.fixedDeltaTime;
            if (_ropeOnly) { if (_timer >= _pulling.VisualTime) Release(false); return; }

            var toAnchor = _anchor.Value - _body.position;
            if (_health.IsDead || _timer >= _pulling.MaxFlightTime || toAnchor.magnitude <= _pulling.EffectRadius)
            {
                Release(true);
                return;
            }
            _body.linearVelocity = toAnchor.normalized * (_pulling.ForceSpeed * PowerMultiplierFor(_pulling));
            _movement.LockControl(Time.fixedDeltaTime * 2f);
        }

        /// <summary>Owner only. Casts the hook; false when it reaches nothing (the cooldown is then not spent).</summary>
        public bool TryFire(AbilityData ability, Vector2 aim)
        {
            if (!IsOwner || _pulling != null || _health.IsDead) return false;
            if (!TryCast(ability, aim, out var hit)) return false;

            _pulling = ability;
            _timer = 0f;
            _anchor.Value = hit.point;
            _attached.Value = true;

            var teammate = Teammate(hit);
            _ropeOnly = teammate != null;
            if (_ropeOnly) YankRpc(teammate.NetworkObjectId, _loadout.Catalog.IdOf(ability));
            return true;
        }

        private bool TryCast(AbilityData ability, Vector2 aim, out RaycastHit2D found)
        {
            var count = Physics2D.Raycast(_body.position, aim, _filter, _hits, ability.Range);
            for (var i = 0; i < count; i++)
            {
                if (_hits[i].rigidbody == _body || !CanLatch(ability, _hits[i].collider)) continue;
                found = _hits[i];
                return true;
            }
            found = default;
            return false;
        }

        private bool CanLatch(AbilityData ability, Collider2D collider)
        {
            if (collider.gameObject.layer != Layers.AbilityIndex) return true;
            var effect = collider.GetComponentInParent<AbilityEffect>();
            return effect != null && ComboRules.Match(_combos, ability.Tags, effect.Tags, ComboInteraction.Latch, out _);
        }

        private static Health Teammate(RaycastHit2D hit)
        {
            var body = hit.rigidbody;
            if (body == null || !body.TryGetComponent(out Health health)) return null;
            return health.Team == Team.Player && !health.IsDead ? health : null;
        }

        private void Release(bool keepMomentum)
        {
            _pulling = null;
            _attached.Value = false;
            if (keepMomentum) _movement.LockControl(_movementData.KnockbackControlLock);
        }

        private float PowerMultiplierFor(AbilityData ability)
        {
            if (_loadout == null || ability == null) return 1f;
            for (var i = 0; i < AbilityLoadout.SlotCount; i++)
                if (_loadout.Get(i) == ability && _loadout.Level(i) >= 2)
                    return ability.Level2PowerMultiplier;
            return 1f;
        }

        /// <summary>Server: pulls a teammate toward the caster, through the teammate's own Knockback.</summary>
        [Rpc(SendTo.Server)]
        private void YankRpc(ulong targetId, short abilityId)
        {
            var ability = _loadout.Catalog.Get(abilityId);
            if (ability == null || !NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var target)) return;
            if (!target.TryGetComponent(out Health health) || health.IsDead) return;
            if (!target.TryGetComponent(out Knockback knockback)) return;

            var toCaster = ((Vector2)transform.position - (Vector2)target.transform.position).normalized;
            var power = PowerMultiplierFor(ability);
            knockback.PushRpc(toCaster * (ability.Knockback * power) + Vector2.up * (ability.KnockbackLift * power));
        }
    }
}
