using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Spawns <see cref="BroodCrawler"/> minions to either side of The Brood Mother when the
    /// attack enters its active phase on the server, capped at <see cref="_maxAlive"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Spawn Crawlers Behaviour", fileName = "SpawnCrawlersBehaviour")]
    public class SpawnCrawlersBehaviour : BossAttackBehaviour
    {
        [SerializeField] private BroodCrawler _crawlerPrefab;
        [SerializeField, Min(1)] private int _spawnCount = 2;
        [SerializeField, Min(1)] private int _maxAlive = 4;
        [SerializeField] private float _spawnOffsetX = 2.8f;
        [SerializeField] private float _spawnOffsetY = -1.6f;
        [SerializeField] private Vector2 _launchVelocity = new(4.5f, 5.5f);
        [SerializeField, Min(1f)] private float _arenaHalfWidth = 14.5f;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != AttackPhase.Active || !runner.IsServer || _crawlerPrefab == null) return;

            var toSpawn = BroodMotherMath.CrawlersToSpawn(_spawnCount, BroodCrawler.ActiveCount, _maxAlive);
            var origin = runner.transform.position;
            var facing = runner.Facing;

            for (var i = 0; i < toSpawn; i++)
            {
                var side = (sbyte)((i % 2 == 0 ? 1 : -1) * facing);
                var x = BroodMotherMath.ClampArenaX(origin.x + side * _spawnOffsetX, _arenaHalfWidth);
                var spawnPos = new Vector3(x, origin.y + _spawnOffsetY, 0f);
                var instance = Instantiate(_crawlerPrefab, spawnPos, Quaternion.identity);
                instance.GetComponent<NetworkObject>().Spawn(true);
                instance.ServerLaunch(new Vector2(side * _launchVelocity.x, _launchVelocity.y), side);
            }
        }
    }
}
