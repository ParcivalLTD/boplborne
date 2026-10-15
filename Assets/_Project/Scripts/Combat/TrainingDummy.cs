using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// A server-owned target for testing attacks. It never fights back; at 0 HP the server
    /// refills it after a short delay so testing can go on.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class TrainingDummy : NetworkBehaviour
    {
        [SerializeField] private TrainingDummyData _data;

        private Health _health;
        private Coroutine _refill;

        /// <summary>The spawned dummy on this PC, if any. Used by debug commands.</summary>
        public static TrainingDummy Active { get; private set; }

        public Health Health => _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        public override void OnNetworkSpawn()
        {
            Active = this;
            if (IsServer) _health.Died += OnDied;
        }

        public override void OnNetworkDespawn()
        {
            if (Active == this) Active = null;
            _health.Died -= OnDied;
            if (_refill != null) StopCoroutine(_refill);
            _refill = null;
        }

        private void OnDied()
        {
            if (_refill != null) StopCoroutine(_refill);
            _refill = StartCoroutine(RefillAfterDelay());
        }

        private IEnumerator RefillAfterDelay()
        {
            yield return new WaitForSeconds(_data.RefillDelay);
            _refill = null;
            _health.ServerRestoreFull();
        }
    }
}
