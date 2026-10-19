using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>Dashes the caster through <see cref="AbilityMotion.TryDash"/>. Holds no numbers: they are on the AbilityData.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Caster Actions/Dash", fileName = "DashAction")]
    public class DashAction : CasterAction
    {
        public override bool OwnerUse(AbilityRunner caster, AbilityData ability, Vector2 aim) =>
            caster.TryGetComponent(out AbilityMotion motion) && motion.TryDash(ability, aim);
    }
}
