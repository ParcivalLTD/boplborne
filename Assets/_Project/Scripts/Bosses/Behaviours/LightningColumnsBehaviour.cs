using System.Collections.Generic;
using GraceRun.Core;
using GraceRun.Core.Framing;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// The Storm Wyrm's lightning: when the attack's wind-up starts, the server marks a spot under every
    /// living player (and, with a flank offset, beside them). The columns strike where players stood, so
    /// the wind-up is the time to move.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Lightning Columns Behaviour", fileName = "LightningColumnsBehaviour")]
    public class LightningColumnsBehaviour : BossAttackBehaviour
    {
        [SerializeField] private LightningColumn _columnPrefab;
        [SerializeField, Tooltip("World Y of the arena floor top.")] private float _floorY = -5f;
        [SerializeField, Min(1f), Tooltip("Half width authored for the 32-wide arena; widened with the real arena.")]
        private float _arenaHalfWidth = 14.5f;
        [SerializeField, Min(0.5f), Tooltip("Players closer than this share one column.")]
        private float _mergeDistance = 2.4f;
        [SerializeField, Min(0f), Tooltip("Extra column this far beside each player, toward the middle. 0 = none.")]
        private float _flankOffset;
        [SerializeField, Min(1)] private int _maxColumns = 6;
        [SerializeField, Tooltip("When nobody is alive to aim at, one column this far in front of the boss.")]
        private float _soloFallbackOffset = 5f;

        private readonly List<float> _playerX = new();
        private readonly List<float> _columnX = new();

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != AttackPhase.Windup || !runner.IsServer || _columnPrefab == null) return;

            _playerX.Clear();
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p == null || !p.IsSpawned || p.gameObject.layer == Layers.GhostIndex) continue;
                _playerX.Add(p.transform.position.x);
            }
            if (_playerX.Count == 0) _playerX.Add(runner.transform.position.x + runner.Facing * _soloFallbackOffset);

            StormWyrmMath.CollectColumnX(
                _playerX, _mergeDistance, _flankOffset, ArenaBounds.WidenedHalfWidth(_arenaHalfWidth), _maxColumns, _columnX);
            for (var i = 0; i < _columnX.Count; i++)
            {
                var instance = Instantiate(_columnPrefab, new Vector3(_columnX[i], _floorY, 0f), Quaternion.identity);
                instance.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}
