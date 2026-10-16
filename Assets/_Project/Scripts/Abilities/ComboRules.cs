using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// Every tag interaction in the game (docs/GDD.md, Combo rules). Effects ask this asset what to
    /// do when they meet another tagged thing; the first matching rule wins. Lives under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Combo Rules", fileName = "ComboRules")]
    public class ComboRules : ScriptableObject
    {
        [SerializeField, Tooltip("Checked top to bottom; the first match wins.")]
        private ComboRule[] _rules;

        /// <summary>
        /// True when <paramref name="rules"/> (may be null: no combos) has a rule for an ability with
        /// <paramref name="source"/> tags meeting <paramref name="target"/> that does <paramref name="interaction"/>.
        /// </summary>
        public static bool Match(ComboRules rules, AbilityTags source, ComboSubject target, ComboInteraction interaction, out ComboRule rule)
        {
            rule = default;
            return rules != null && ComboLookup.TryFind(rules._rules, source, target, out rule) && rule.Interaction == interaction;
        }

        /// <summary>The same, when the target is another ability.</summary>
        public static bool Match(ComboRules rules, AbilityTags source, AbilityTags target, ComboInteraction interaction, out ComboRule rule) =>
            Match(rules, source, ComboLookup.SubjectOf(target), interaction, out rule);
    }
}
