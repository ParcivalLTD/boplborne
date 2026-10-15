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
        [SerializeField] private Health _health;

        private IInvulnerability _invulnerability;

        public Health Health => _health;
        public bool IsInvulnerable => _invulnerability != null && _invulnerability.IsInvulnerable;

        private void Awake()
        {
            _invulnerability = _health.GetComponent<IInvulnerability>();
        }
    }
}
