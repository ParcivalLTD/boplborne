using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// In-scene networked spawner for bosses. The server spawns a boss (replacing any current one)
    /// at the spawn point; "Skip to boss" in the F1 menu asks for it through <see cref="SkipToBossRpc"/>.
    /// Placeholder until RunManager swaps arena prefabs (M4/M6).
    /// </summary>
    public class BossSpawner : NetworkBehaviour
    {
        [SerializeField] private BossData[] _bosses;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField, Tooltip("Spawn the first boss as soon as the Game scene starts. Off: use F1 > Skip to boss after everyone joined, so HP scales to the right player count.")]
        private bool _spawnOnStart;

        /// <summary>The spawner in the loaded Game scene on this PC, if any.</summary>
        public static BossSpawner Instance { get; private set; }

        /// <summary>Raised on the server when a debug skip spawns a boss.</summary>
        public event System.Action<BossData> BossSkipped;

        public IReadOnlyList<BossData> Bosses => _bosses;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            if (IsServer && _spawnOnStart && _bosses.Length > 0) ServerSpawn(0);
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Debug: (re)spawns boss <paramref name="index"/> with HP scaled to the players connected now.</summary>
        [Rpc(SendTo.Server)]
        public void SkipToBossRpc(int index)
        {
            if (index < 0 || index >= _bosses.Length) return;
            var boss = _bosses[index];
            BossSkipped?.Invoke(boss);
            ServerSpawn(boss);
        }

        /// <summary>Server only: (re)spawns <paramref name="boss"/> with HP scaled to the players connected now.</summary>
        public void ServerSpawn(BossData boss, int? seed = null)
        {
            if (!IsServer || boss == null || boss.Prefab == null) return;
            ServerDespawnCurrent();
            var instance = Instantiate(boss.Prefab, _spawnPoint.position, Quaternion.identity);
            if (seed.HasValue && instance.TryGetComponent<BossBrain>(out var brain)) brain.ServerSetSeed(seed.Value);
            instance.GetComponent<NetworkObject>().Spawn(true);
        }

        /// <summary>Server only: removes the current boss (and any boss-spawned minions or hazard zones).</summary>
        public void ServerDespawnCurrent()
        {
            if (!IsServer) return;
            Behaviours.BroodCrawler.ServerDespawnAll();
            Behaviours.WebZone.ServerDespawnAll();
            var current = BossBrain.Active;
            if (current != null && current.IsSpawned) current.NetworkObject.Despawn();
        }

        private void ServerSpawn(int index)
        {
            if (index >= 0 && index < _bosses.Length) ServerSpawn(_bosses[index]);
        }
    }
}
