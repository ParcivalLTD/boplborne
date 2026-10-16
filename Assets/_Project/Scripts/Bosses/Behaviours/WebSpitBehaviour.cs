using System.Collections.Generic;
using GraceRun.Core;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Spawns sticky <see cref="WebZone"/> floor hazards at living players' horizontal positions
    /// when The Brood Mother's web spit enters its active phase on the server. Rewards spreading out.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Web Spit Behaviour", fileName = "WebSpitBehaviour")]
    public class WebSpitBehaviour : BossAttackBehaviour
    {
        [SerializeField] private WebZone _webZonePrefab;
        [SerializeField] private float _floorY = -4.9f;
        [SerializeField, Min(1f)] private float _arenaHalfWidth = 14.5f;
        [SerializeField, Min(0.5f)] private float _mergeDistance = 2.0f;
        [SerializeField] private float _soloSecondOffset = 5.0f;
        [SerializeField, Min(1)] private int _maxZonesPerCast = 4;

        private readonly List<float> _playerX = new();
        private readonly List<float> _zoneX = new();

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != AttackPhase.Active || !runner.IsServer || _webZonePrefab == null) return;

            _playerX.Clear();
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p == null || !p.IsSpawned || p.gameObject.layer == Layers.GhostIndex) continue;
                _playerX.Add(p.transform.position.x);
            }

            if (_playerX.Count == 0)
                _playerX.Add(runner.transform.position.x + runner.Facing * _soloSecondOffset);

            BroodMotherMath.CollectWebZoneX(
                _playerX, runner.Facing, _soloSecondOffset, _mergeDistance, _arenaHalfWidth, _maxZonesPerCast, _zoneX);

            for (var i = 0; i < _zoneX.Count; i++)
            {
                var pos = new Vector3(_zoneX[i], _floorY, 0f);
                var instance = Instantiate(_webZonePrefab, pos, Quaternion.identity);
                instance.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}
