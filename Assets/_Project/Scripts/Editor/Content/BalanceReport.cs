using System.Collections.Generic;
using System.Text;
using GraceRun.Abilities;
using GraceRun.Abilities.Effects;
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
        private const float ZoneUptime = 0.5f;
        private const float TurretUptime = 0.8f;

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

        /// <summary>
        /// Expected damage per second of every ability that can hurt an enemy, by ability id (all hits land;
        /// a zone is assumed to touch its target half the time it is up).
        /// </summary>
        public static List<KeyValuePair<string, float>> AbilityDpsList(CombatData combat)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AbilityCatalog>(CatalogPath);
            var list = new List<KeyValuePair<string, float>>();
            for (var i = 0; i < catalog.Count; i++)
            {
                var ability = catalog.Get(i);
                if (ability == null) continue;
                var damage = ExpectedDamagePerCast(ability, combat.AbilityDamageMultiplier);
                if (damage <= 0f || ability.Cooldown <= 0f) continue;
                list.Add(new(ability.Id, damage / ability.Cooldown));
            }
            return list;
        }

        /// <summary>A zone that keeps hurting (spike trap, fire pool, blade ring), not a mine or a dash strike that hits once.</summary>
        private static bool IsRepeatingZone(GameObject prefab)
        {
            var zone = prefab.GetComponent<ContactZone>();
            if (zone == null) return false;
            var so = new SerializedObject(zone);
            return !so.FindProperty("_detonateOnce").boolValue && !so.FindProperty("_hitEachTargetOnce").boolValue;
        }

        /// <summary>Authored damage times the multiplier, summed over what one cast puts out: shots, zone ticks, child effects, turret bolts.</summary>
        public static float ExpectedDamagePerCast(AbilityData ability, float multiplier)
        {
            var prefab = ability.EffectPrefab;
            var total = 0f;
            var direct = (float)DamageMath.Scaled(ability.Damage, multiplier);
            if (prefab != null && IsRepeatingZone(prefab.gameObject))
            {
                var ticks = ability.TickInterval > 0f ? ability.EffectLifetime / ability.TickInterval : 1f;
                total += direct * Mathf.Max(1f, ticks) * ZoneUptime;
            }
            else
            {
                var hits = HitsPerCast.TryGetValue(ability.Id, out var extra) ? extra : Mathf.Max(1, ability.Count);
                total += direct * hits;
            }

            var child = ability.ChildEffect;
            if (child != null && child.Ability != null)
            {
                var childAbility = child.Ability;
                var childDamage = (float)DamageMath.Scaled(childAbility.Damage, multiplier);
                if (prefab != null && prefab.GetComponent<Turret>() != null)
                    total += childDamage * Mathf.Max(0f, ability.EffectLifetime / Mathf.Max(0.1f, ability.TickInterval)) * TurretUptime;
                else if (IsRepeatingZone(child.gameObject))
                    total += childDamage * Mathf.Max(1f, childAbility.EffectLifetime / Mathf.Max(0.1f, childAbility.TickInterval)) * ZoneUptime * Mathf.Max(1, ability.ChildCount);
                else
                    total += childDamage * Mathf.Max(1, ability.ChildCount);
            }
            return total;
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
