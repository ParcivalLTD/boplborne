using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Part of an ability that runs on the caster's own PC the moment the button is pressed, so
    /// movement abilities feel instant (the caster's movement is owner-authoritative). The server
    /// still re-checks the cooldown and spawns the effect prefab, if any.
    /// </summary>
    public abstract class CasterAction : ScriptableObject
    {
        /// <summary>Owner only. Returns false when there is nothing to do; the ability is then not used.</summary>
        public abstract bool OwnerUse(AbilityRunner caster, AbilityData ability, Vector2 aim);
    }
}
