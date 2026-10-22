using System.Collections.Generic;
using System.Text;
using GraceRun.Abilities;
using GraceRun.Combat;
using GraceRun.Player;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Balance → Print DPS Table: logs the model DPS of every weapon (light combo, stamina
    /// limited) and every damaging ability, with the current CombatData multipliers applied.
    /// </summary>
    public static class BalanceReport
    {
        private const string CatalogPath = "Assets/_Project/Data/Config/AbilityCatalog.asset";
        private const string CombatPath = "Assets/_Project/Data/Config/CombatData.asset";
        private const string StatsPath = "Assets/_Project/Data/Player/PlayerStatsData.asset";
        private const float Horizon = 60f;

        /// <summary>Extra direct hits per cast beyond <see cref="AbilityData.Count"/> (boomerang return, bomblets, spikes).</summary>
        private static readonly Dictionary<string, int> HitsPerCast = new() { { "boomerang", 2 }, { "cluster_bomb", 4 } };

        [MenuItem("Grace Run/Balance/Print DPS Table")]
        public static void Print() => Debug.Log(Build());

        /// <summary>The report text; also used by the balance tests.</summary>
        public static string Build()
        {
            var sb = new StringBuilder();
            var combat = AssetDatabase.LoadAssetAtPath<CombatData>(CombatPath);
            var stats = AssetDatabase.LoadAssetAtPath<PlayerStatsData>(StatsPath);
            sb.AppendLine($"DPS model (all hits land). Weapon x{combat.WeaponDamageMultiplier}, ability x{combat.AbilityDamageMultiplier}");
            foreach (var weapon in Weapons(out var names))
                sb.AppendLine($"  weapon {names[weapon]}: sustained {WeaponDps(weapon, combat, stats):F1}");
            foreach (var entry in AbilityDpsList(combat))
                sb.AppendLine($"  ability {entry.Key}: {entry.Value:F1}");
            return sb.ToString();
        }

        /// <summary>Sustained light-combo DPS of <paramref name="weapon"/> with the current multipliers.</summary>
        public static float WeaponDps(WeaponData weapon, CombatData combat, PlayerStatsData stats) =>
            DpsModel.SustainedWeaponDps(Combo(weapon), combat.WeaponDamageMultiplier, stats.MaxStamina,
                stats.StaminaRegenPerSecond, stats.RegenDelay, Horizon);

        /// <summary>DPS of every ability that deals direct damage, by ability id.</summary>
        public static List<KeyValuePair<string, float>> AbilityDpsList(CombatData combat)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AbilityCatalog>(CatalogPath);
            var list = new List<KeyValuePair<string, float>>();
            for (var i = 0; i < catalog.Count; i++)
            {
                var ability = catalog.Get(i);
                if (ability == null || ability.Damage <= 0) continue;
                var hits = HitsPerCast.TryGetValue(ability.Id, out var extra) ? extra : ability.Count;
                list.Add(new(ability.Id, DpsModel.AbilityDps(ability.Damage, hits, ability.Cooldown, combat.AbilityDamageMultiplier)));
            }
            return list;
        }

        private static List<SwingStat> Combo(WeaponData weapon)
        {
            var swings = new List<SwingStat>();
            var moves = weapon.BuildMoves();
            for (var i = 0; i < moves.ComboLength; i++)
            {
                var hit = moves.Hits[i];
                swings.Add(new SwingStat(hit.Damage, hit.Startup + hit.Active + hit.Recovery, hit.StaminaCost));
            }
            return swings;
        }

        private static IEnumerable<WeaponData> Weapons(out Dictionary<WeaponData, string> names)
        {
            names = new Dictionary<WeaponData, string>();
            var list = new List<WeaponData>();
            foreach (var guid in AssetDatabase.FindAssets("t:WeaponData"))
            {
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid));
                names[weapon] = weapon.name;
                list.Add(weapon);
            }
            return list;
        }
    }
}
