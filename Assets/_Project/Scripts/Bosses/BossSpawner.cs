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
            SpawnEscorts(boss, seed);
            _swarm = boss.Swarm;
            _nextWaveAt = _swarm.Crawler != null ? Time.time + _swarm.FirstDelay : float.MaxValue;
        }

        private SwarmSpec _swarm;
        private float _nextWaveAt = float.MaxValue;

        private void Update()
        {
            if (!IsServer || _swarm.Crawler == null || Time.time < _nextWaveAt) return;
            if (BossBrain.All.Count == 0 || BossBrain.AreAllDefeated)
            {
                _swarm = default;
                return;
            }
            _nextWaveAt = Time.time + _swarm.Interval;
            SendWave();
        }

        /// <summary>Server only: drops a wave of crawlers in from both edges (the nearer edge first), up to the cap.</summary>
        private void SendWave()
        {
            var toSpawn = Behaviours.BroodMotherMath.CrawlersToSpawn(_swarm.PerWave, Behaviours.BroodCrawler.ActiveCount, _swarm.MaxAlive);
            var half = Core.Framing.ArenaBounds.WidenedHalfWidth(_swarm.ArenaHalfWidth);
            for (var i = 0; i < toSpawn; i++)
            {
                var side = (sbyte)(i % 2 == 0 ? 1 : -1);
                var x = Behaviours.BroodMotherMath.ClampArenaX(side * half, half);
                var at = new Vector3(x, _spawnPoint.position.y + _swarm.SpawnHeight, 0f);
                var crawler = Instantiate(_swarm.Crawler, at, Quaternion.identity);
                crawler.GetComponent<NetworkObject>().Spawn(true);
                crawler.ServerLaunch(new Vector2(-side * 3f, 1f), (sbyte)-side);
            }
        }

        /// <summary>Server only: fields the boss's miniboss escorts (smaller, weaker bodies) around the spawn point.</summary>
        private void SpawnEscorts(BossData boss, int? seed)
        {
            var index = 0;
            foreach (var escort in boss.Escorts)
            {
                if (escort.Boss == null || escort.Boss.Prefab == null) continue;
                for (var i = 0; i < Mathf.Max(1, escort.Count); i++, index++)
                {
                    var side = index % 2 == 0 ? 1f : -1f;
                    var offset = new Vector3(escort.Offset.x * side * (1 + index / 2), escort.Offset.y, 0f);
                    var body = Instantiate(escort.Boss.Prefab, _spawnPoint.position + offset, Quaternion.identity);
                    if (body.TryGetComponent<BossBrain>(out var brain))
                    {
                        brain.ServerSetEscort(escort.HpScale, escort.SizeScale, escort.DamageScale);
                        if (seed.HasValue) brain.ServerSetSeed(seed.Value + 101 * (index + 1));
                    }
                    body.GetComponent<NetworkObject>().Spawn(true);
                }
            }
        }

        /// <summary>Server only: removes all active boss bodies (and any boss-spawned minions or hazard zones).</summary>
        public void ServerDespawnCurrent()
        {
            if (!IsServer) return;
            _swarm = default;
            Behaviours.BroodCrawler.ServerDespawnAll();
            Behaviours.WebZone.ServerDespawnAll();
            Behaviours.BellShockwave.ServerDespawnAll();
            Behaviours.LightningColumn.ServerDespawnAll();
            var all = BossBrain.All;
            for (var i = all.Count - 1; i >= 0; i--)
            {
                var boss = all[i];
                if (boss != null && boss.IsSpawned) boss.NetworkObject.Despawn();
            }
        }

        private void ServerSpawn(int index)
        {
            if (index >= 0 && index < _bosses.Length) ServerSpawn(_bosses[index]);
        }
    }
}
