using System.Collections.Generic;

namespace GraceRun.Abilities
{
    /// <summary>Pure combo rule lookup (EditMode tested). Only tags are compared, never named abilities.</summary>
    public static class ComboLookup
    {
        /// <summary>The <see cref="ComboSubject"/> bits of an ability's tags.</summary>
        public static ComboSubject SubjectOf(AbilityTags tags) => (ComboSubject)(int)tags;

        /// <summary>
        /// The first rule whose source shares a tag with <paramref name="source"/> and whose target
        /// shares a bit with <paramref name="target"/>. False (and a None rule) when nothing matches.
        /// </summary>
        public static bool TryFind(IReadOnlyList<ComboRule> rules, AbilityTags source, ComboSubject target, out ComboRule rule)
        {
            if (rules != null)
            {
                for (var i = 0; i < rules.Count; i++)
                {
                    var candidate = rules[i];
                    if (candidate.Interaction == ComboInteraction.None) continue;
                    if ((candidate.Source & source) == 0 || (candidate.Target & target) == 0) continue;
                    rule = candidate;
                    return true;
                }
            }
            rule = default;
            return false;
        }
    }
}
