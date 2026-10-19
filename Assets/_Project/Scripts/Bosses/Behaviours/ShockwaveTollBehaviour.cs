using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Signature attack behaviour of The Bell Twins: when the bell toll enters its active phase on
    /// the server, emits left- and right-traveling ground-hugging <see cref="BellShockwave"/>s along
    /// the arena floor.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Shockwave Toll Behaviour", fileName = "ShockwaveTollBehaviour")]
    public class ShockwaveTollBehaviour : BossAttackBehaviour
    {
        [SerializeField] private BellShockwave _shockwavePrefab;
        [SerializeField] private float _spawnOffsetX = 1.8f;
        [SerializeField] private float _floorY = -4.2f;
        [SerializeField, Min(0f), Tooltip("0 = use ShockwaveData.Speed; >0 overrides wave speed (used by Phase 2 enraged echo).")]
        private float _speedOverride;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != AttackPhase.Active || !runner.IsServer || _shockwavePrefab == null) return;

            var x = runner.transform.position.x;
            SpawnWave(x - _spawnOffsetX, -1);
            SpawnWave(x + _spawnOffsetX, 1);
        }

        private void SpawnWave(float x, sbyte direction)
        {
            var instance = Instantiate(_shockwavePrefab, new Vector3(x, _floorY, 0f), Quaternion.identity);
            instance.ServerConfigure(direction, _speedOverride);
            instance.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
