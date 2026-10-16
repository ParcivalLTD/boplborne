using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Optional behaviour asset hooked onto an <see cref="AttackData"/> for boss-specific attack
    /// effects (spawning minions, dropping hazard zones, ceiling leaps). All tunable values live
    /// on the asset.
    /// </summary>
    public abstract class BossAttackBehaviour : ScriptableObject
    {
        /// <summary>Called on every PC when <paramref name="runner"/> moves into <paramref name="phase"/> for <paramref name="attack"/>.</summary>
        public abstract void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase);
    }
}
