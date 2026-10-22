using GraceRun.Core;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// The Mirror Duelist's signature attack: after the rapier cut, it throws back the kind of the last
    /// ability that hit it: a shard for projectiles, floor waves for area abilities, a dash for movement,
    /// nothing extra for plain weapon hits. The kind is fixed when the wind-up starts.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Mirror Echo Behaviour", fileName = "MirrorEchoBehaviour")]
    public class MirrorEchoBehaviour : BossAttackBehaviour
    {
        [SerializeField] private MirrorDuelistData _data;
        [SerializeField, Tooltip("Flying glass shard (a BellShockwave with its own ShockwaveData).")]
        private BellShockwave _shardPrefab;
        [SerializeField, Tooltip("Floor wave (a BellShockwave with its own ShockwaveData).")]
        private BellShockwave _wavePrefab;
        [SerializeField] private float _floorY = -5f;
        [SerializeField, Tooltip("Height of a floor wave's spawn point above the floor.")]
        private float _waveHeight = 0.8f;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (!runner.IsServer || !runner.TryGetComponent(out MirrorMemory memory)) return;

            if (phase == AttackPhase.Windup)
            {
                memory.Latch();
                return;
            }
            if (phase != AttackPhase.Active) return;

            var origin = runner.transform.position;
            switch (memory.Latched)
            {
                case MirrorKind.Shard:
                    var y = MirrorMath.ShardY(TargetY(runner), _floorY, _data.ShardHeight.x, _data.ShardHeight.y);
                    Spawn(_shardPrefab, origin.x + runner.Facing * _data.MuzzleOffset, y, runner.Facing);
                    break;
                case MirrorKind.Burst:
                    Spawn(_wavePrefab, origin.x - _data.MuzzleOffset, _floorY + _waveHeight, -1);
                    Spawn(_wavePrefab, origin.x + _data.MuzzleOffset, _floorY + _waveHeight, 1);
                    break;
                case MirrorKind.Dash:
                    if (runner.TryGetComponent(out BossMotor motor)) motor.ServerDash(runner.Facing * _data.DashSpeed, _data.DashTime);
                    break;
            }
        }

        private static void Spawn(BellShockwave prefab, float x, float y, int direction)
        {
            if (prefab == null) return;
            var instance = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
            instance.ServerConfigure((sbyte)direction);
            instance.GetComponent<NetworkObject>().Spawn(true);
        }

        private float TargetY(BossAttackRunner runner)
        {
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p != null && p.IsSpawned && p.NetworkObjectId == runner.TargetId) return p.transform.position.y;
            }
            return _floorY + _data.ShardHeight.x;
        }
    }
}
