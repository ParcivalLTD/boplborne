using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>Teleports the caster through <see cref="AbilityMotion.TryBlink"/>. Holds no numbers: they are on the AbilityData.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Caster Actions/Blink", fileName = "BlinkAction")]
    public class BlinkAction : CasterAction
    {
        public override bool OwnerUse(AbilityRunner caster, AbilityData ability, Vector2 aim) =>
            caster.TryGetComponent(out AbilityMotion motion) && motion.TryBlink(ability, aim);
    }
}
