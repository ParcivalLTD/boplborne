using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// A trigger collider that deals one hit to each <see cref="Hurtbox"/> it touches while armed.
    /// Only the PC that owns the detection reports the hit (see <see cref="HitDetection"/>); the
    /// server resolves it. Attack code calls <see cref="Arm"/> for the active frames and
    /// <see cref="Disarm"/> after.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private NetworkObject _attacker;
        [SerializeField] private Team _team = Team.Player;
        [SerializeField] private HitDetection _detection = HitDetection.AttackerSide;

        private readonly HashSet<Health> _alreadyHit = new();
        private Collider2D _collider;
        private DamageInfo _damage;

        public bool IsArmed { get; private set; }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        private void OnDisable() => Disarm();

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);

        /// <summary>Victim side: a player still inside when their i-frames end gets hit then.</summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            if (IsArmed && _detection == HitDetection.VictimSide) TryHit(other);
        }

        /// <summary>Starts a new swing: clears the hit list and enables the collider.</summary>
        public void Arm(int amount, Vector2 knockback)
        {
            _damage = new DamageInfo(amount, knockback, _attacker.NetworkObjectId, _team);
            _alreadyHit.Clear();
            IsArmed = true;
            _collider.enabled = true;
        }

        public void Disarm()
        {
            IsArmed = false;
            if (_collider != null) _collider.enabled = false;
        }

        private void TryHit(Collider2D other)
        {
            if (!IsArmed || !other.TryGetComponent(out Hurtbox hurtbox)) return;

            var target = hurtbox.Health;
            if (target == null || !target.IsSpawned || target.IsDead) return;
            if (target.NetworkObjectId == _attacker.NetworkObjectId) return;
            if (!DetectsHere(hurtbox) || !_alreadyHit.Add(target)) return;

            target.ReportHit(_damage);
        }

        private bool DetectsHere(Hurtbox hurtbox) => _detection switch
        {
            HitDetection.AttackerSide => _attacker.IsOwner && !hurtbox.IsInvulnerable,
            HitDetection.VictimSide => hurtbox.Health.IsOwner && !hurtbox.IsInvulnerable,
            _ => false
        };
    }
}
