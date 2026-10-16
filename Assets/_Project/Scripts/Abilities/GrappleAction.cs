using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>Fires the caster's <see cref="GrappleHook"/>. Holds no numbers: they are on the AbilityData.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Caster Actions/Grapple", fileName = "GrappleAction")]
    public class GrappleAction : CasterAction
    {
        public override bool OwnerUse(AbilityRunner caster, AbilityData ability, Vector2 aim) =>
            caster.TryGetComponent(out GrappleHook hook) && hook.TryFire(ability, aim);
    }
}
