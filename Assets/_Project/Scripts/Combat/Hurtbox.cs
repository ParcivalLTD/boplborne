using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Marks a collider as something hitboxes can hit and points to the <see cref="Health"/> it
    /// belongs to. Reports whether that target is invulnerable right now (dodge i-frames).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hurtbox : MonoBehaviour
    {
        private static readonly Dictionary<Collider2D, Hurtbox> ByCollider = new();

        [SerializeField] private Health _health;

        private IInvulnerability _invulnerability;
        private Collider2D _collider;

        /// <summary>Every live hurtbox on this PC (for the debug overlay).</summary>
        public static Dictionary<Collider2D, Hurtbox>.ValueCollection All => ByCollider.Values;

        public Health Health => _health;
        public bool IsInvulnerable => _invulnerability != null && _invulnerability.IsInvulnerable;

        private void Awake()
        {
            _invulnerability = _health.GetComponent<IInvulnerability>();
            _collider = GetComponent<Collider2D>();
            ByCollider[_collider] = this;
        }

        private void OnDestroy()
        {
            if (_collider != null) ByCollider.Remove(_collider);
        }

        /// <summary>The hurtbox on <paramref name="collider"/>, without a GetComponent call (safe in FixedUpdate).</summary>
        public static bool TryFind(Collider2D collider, out Hurtbox hurtbox)
        {
            hurtbox = null;
            return collider != null && ByCollider.TryGetValue(collider, out hurtbox) && hurtbox != null;
        }
    }
}
