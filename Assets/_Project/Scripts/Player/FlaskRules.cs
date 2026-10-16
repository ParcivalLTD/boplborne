using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Pure flask rules: who may drink, and how much HP a sip gives back. Used by
    /// <see cref="PlayerFlask"/> on the server.
    /// </summary>
    public static class FlaskRules
    {
        /// <summary>
        /// A living player with a charge left and missing HP may drink. Full HP refuses, so a
        /// mistaken press never wastes a charge; a ghost (0 HP) can't drink.
        /// </summary>
        public static bool CanDrink(int charges, int currentHP, int maxHP) =>
            charges > 0 && currentHP > 0 && currentHP < maxHP;

        /// <summary>HP after drinking: current plus the heal, capped at max.</summary>
        public static int HealedHP(int currentHP, int maxHP, int heal) =>
            Mathf.Min(maxHP, currentHP + Mathf.Max(0, heal));
    }
}
