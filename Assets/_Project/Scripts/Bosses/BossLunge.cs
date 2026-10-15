using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Server only: dashes the boss forward when an attack with a lunge (<see cref="AttackData.LungeDistance"/>)
    /// reaches its active phase. The server-auth NetworkTransform carries the move to every PC, and the
    /// victim-side hitbox moves with the boss each player sees.
    /// </summary>
    [RequireComponent(typeof(BossAttackRunner), typeof(BossMotor))]
    public class BossLunge : NetworkBehaviour
    {
        private BossAttackRunner _runner;
        private BossMotor _motor;

        private void Awake()
        {
            _runner = GetComponent<BossAttackRunner>();
            _motor = GetComponent<BossMotor>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer) _runner.PhaseChanged += OnAttackPhaseChanged;
        }

        public override void OnNetworkDespawn()
        {
            _runner.PhaseChanged -= OnAttackPhaseChanged;
        }

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            var attack = _runner.Attack;
            if (phase != AttackPhase.Active || attack == null) return;
            if (attack.LungeDistance <= 0f || attack.LungeTime <= 0f) return;
            _motor.ServerDash(_runner.Facing * attack.LungeDistance / attack.LungeTime, attack.LungeTime);
        }
    }
}
