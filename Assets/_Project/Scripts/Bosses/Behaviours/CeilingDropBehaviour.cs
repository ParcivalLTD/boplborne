using GraceRun.Player;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Phase 2 signature attack of The Brood Mother: during the wind-up the boss perches visibly
    /// under the arena ceiling directly above the targeted player while the red ground zone marks
    /// the floor below; in the active phase she slams straight down onto the floor.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Ceiling Drop Behaviour", fileName = "CeilingDropBehaviour")]
    public class CeilingDropBehaviour : BossAttackBehaviour
    {
        [SerializeField, Tooltip("World Y of the boss centre while perched under the ceiling.")]
        private float _ceilingY = 5.4f;
        [SerializeField, Min(1f), Tooltip("Downward slam speed in units per second.")]
        private float _dropSpeed = 44f;
        [SerializeField, Min(0.05f)] private float _dropDuration = 0.22f;
        [SerializeField, Min(1f)] private float _arenaHalfWidth = 14.0f;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (!runner.IsServer || !runner.TryGetComponent(out BossMotor motor)) return;

            switch (phase)
            {
                case AttackPhase.Windup:
                    var dropX = BroodMotherMath.ClampArenaX(ResolveTargetX(runner), _arenaHalfWidth);
                    motor.ServerPerch(new Vector2(dropX, _ceilingY));
                    break;
                case AttackPhase.Active:
                    motor.ServerDrop(_dropSpeed, _dropDuration);
                    break;
                case AttackPhase.Recovery:
                case AttackPhase.None:
                    motor.ServerClearVertical();
                    break;
            }
        }

        private static float ResolveTargetX(BossAttackRunner runner)
        {
            var targetId = runner.TargetId;
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p != null && p.IsSpawned && p.NetworkObjectId == targetId)
                    return p.transform.position.x;
            }
            return runner.transform.position.x;
        }
    }
}
