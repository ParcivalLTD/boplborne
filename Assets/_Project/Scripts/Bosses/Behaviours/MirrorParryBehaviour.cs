using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// BossAttackBehaviour that arms the MirrorParryState during the active phase of the parry stance.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Behaviours/Mirror Parry", fileName = "MirrorParryBehaviour")]
    public class MirrorParryBehaviour : BossAttackBehaviour
    {
        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (runner.TryGetComponent(out MirrorParryState parry))
            {
                parry.SetParry(phase == AttackPhase.Active);
            }
        }
    }
}
