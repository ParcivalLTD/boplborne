using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Server side: spawns the training dummy at this transform in the hub (and despawns it during
    /// floor boss fights).
    /// </summary>
    public class TrainingDummySpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject _dummyPrefab;

        public static TrainingDummySpawner Instance { get; private set; }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start() => ServerSpawn();

        /// <summary>Server only: spawns the training dummy if none is active.</summary>
        public void ServerSpawn()
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsServer || _dummyPrefab == null) return;
            if (TrainingDummy.Active != null && TrainingDummy.Active.IsSpawned) return;

            var dummy = Instantiate(_dummyPrefab, transform.position, Quaternion.identity);
            dummy.Spawn(true);
        }

        /// <summary>Server only: removes the active training dummy.</summary>
        public void ServerDespawn()
        {
            var current = TrainingDummy.Active;
            if (current != null && current.IsServer && current.IsSpawned) current.NetworkObject.Despawn();
        }
    }
}
