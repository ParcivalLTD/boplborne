using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Server side: spawns the training dummy at this transform when the Game scene starts.
    /// Placeholder until arenas and the hub exist.
    /// </summary>
    public class TrainingDummySpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject _dummyPrefab;

        private void Start()
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsServer) return;

            var dummy = Instantiate(_dummyPrefab, transform.position, Quaternion.identity);
            dummy.Spawn(true);
        }
    }
}
