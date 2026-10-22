using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Editor.Content;
using GraceRun.Player;
using NUnit.Framework;
using UnityEditor;

namespace GraceRun.Tests
{
    /// <summary>
    /// Keeps the combat roles from docs/GDD.md in check from the real assets: weapons are a modest
    /// builder, a good ability trio clearly out-damages any weapon.
    /// </summary>
    public class DpsBalanceTests
    {
        private const float WeaponCeiling = 10f;
        private const float WeaponFloor = 4f;
        private const float TrioOverWeapon = 2f;

        private static float BestWeapon(CombatData combat, PlayerStatsData stats, out float worst)
        {
            var best = 0f;
            worst = float.MaxValue;
            foreach (var guid in AssetDatabase.FindAssets("t:WeaponData"))
            {
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid));
                var dps = BalanceReport.WeaponDps(weapon, combat, stats);
                best = System.Math.Max(best, dps);
                worst = System.Math.Min(worst, dps);
            }
            return best;
        }

        private static (CombatData, PlayerStatsData) Load() => (
            AssetDatabase.LoadAssetAtPath<CombatData>("Assets/_Project/Data/Config/CombatData.asset"),
            AssetDatabase.LoadAssetAtPath<PlayerStatsData>("Assets/_Project/Data/Player/PlayerStatsData.asset"));

        [Test]
        public void WeaponsStayAModestBuilder()
        {
            var (combat, stats) = Load();
            var best = BestWeapon(combat, stats, out var worst);
            Assert.LessOrEqual(best, WeaponCeiling, "a weapon out-damages the builder role");
            Assert.GreaterOrEqual(worst, WeaponFloor, "a weapon is too weak to be worth swinging");
        }

        [Test]
        public void ABestAbilityTrioBeatsTheBestWeaponTwiceOver()
        {
            var (combat, stats) = Load();
            var best = BestWeapon(combat, stats, out _);
            var dps = new List<float>();
            foreach (var entry in BalanceReport.AbilityDpsList(combat)) dps.Add(entry.Value);
            dps.Sort();
            dps.Reverse();
            var trio = dps[0] + dps[1] + dps[2];
            Assert.GreaterOrEqual(trio, best * TrioOverWeapon, $"top three abilities {trio:F1} DPS vs best weapon {best:F1}");
        }
    }
}
