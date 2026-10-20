using System.IO;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using GraceRun.Core;
using GraceRun.Run;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build The Mirror Duelist:
    /// Constructs Boss #5 (The Mirror Duelist) for Milestone 7.
    /// Sets up MirrorDuelistArena, MirrorParryState, bullet & glass shard behaviours,
    /// AttackData assets (Prismatic Thrust, Glass Shard Burst, Mirror Stance, Shattered Kaleidoscope),
    /// MirrorDuelistData, MirrorDuelist.prefab, DefaultNetworkPrefabs, RunConfig, and BossSpawner.
    /// </summary>
    public static class MirrorDuelistSetup
    {
        private const string DataFolder = "Assets/_Project/Data/Bosses/MirrorDuelist";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Bosses";
        private const string DuelistPrefabPath = PrefabFolder + "/MirrorDuelist.prefab";
        private const string ProjectilePrefabPath = PrefabFolder + "/BossProjectile.prefab";
        private const string ArenaPrefabPath = "Assets/_Project/Prefabs/Arenas/MirrorDuelistArena.prefab";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
        private const string DuelistSpritePath = "Assets/_Project/Art/Bosses/boss_mirror_duelist.png";
        private const string CircleSpritePath = "Assets/_Project/Art/FX/Placeholder/circle.png";

        [InitializeOnLoadMethod]
        [MenuItem("Grace Run/Build The Mirror Duelist")]
        public static void Apply()
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(PrefabFolder);

            // 1. Arena Layout
            ArenaLayoutSetup.Run();
            var arenaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArenaPrefabPath);
            RegisterNetworkPrefab(arenaPrefab);

            // 2. Projectile Prefab
            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);

            // 3. Behaviours
            var shardBurstBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/MirrorDuelist_ShardBurst.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 6);
                SerializedFields.Set(so, "_spreadAngle", 55f);
                SerializedFields.Set(so, "_speed", 13.5f);
                SerializedFields.Set(so, "_damage", 15);
                SerializedFields.Set(so, "_lifetime", 3.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var kaleidoscopeBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/MirrorDuelist_Kaleidoscope.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Ring);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 8);
                SerializedFields.Set(so, "_speed", 10.5f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_bounces", 1);
                SerializedFields.Set(so, "_lifetime", 4.0f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var parryBehaviour = EnsureAsset<MirrorParryBehaviour>($"{DataFolder}/MirrorDuelist_Parry.asset", so => { });

            // 4. Attacks
            var prismaticThrust = EnsureAsset<AttackData>($"{DataFolder}/PrismaticThrust.asset", so =>
            {
                SerializedFields.Set(so, "_id", "prismatic_thrust");
                SerializedFields.Set(so, "_windup", 0.95f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 0.9f);
                SerializedFields.Set(so, "_punishWindow", 0.75f);
                SerializedFields.Set(so, "_damage", 28);
                SerializedFields.Set(so, "_knockback", new Vector2(14f, 5f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(4.5f, -0.5f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(8f, 2.5f));
                SerializedFields.Set(so, "_lungeDistance", 12f);
                SerializedFields.Set(so, "_lungeTime", 0.25f);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 4f);
                SerializedFields.Set(so, "_maxRange", 16f);
            });

            var glassShardBurst = EnsureAsset<AttackData>($"{DataFolder}/GlassShardBurst.asset", so =>
            {
                SerializedFields.Set(so, "_id", "glass_shard_burst");
                SerializedFields.Set(so, "_windup", 0.85f);
                SerializedFields.Set(so, "_active", 0.2f);
                SerializedFields.Set(so, "_recovery", 0.95f);
                SerializedFields.Set(so, "_punishWindow", 0.75f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_behaviour", shardBurstBehaviour);
                SerializedFields.Set(so, "_weight", 1.0f);
                SerializedFields.Set(so, "_cooldown", 5f);
                SerializedFields.Set(so, "_maxRange", 20f);
            });

            var mirrorStance = EnsureAsset<AttackData>($"{DataFolder}/MirrorStance.asset", so =>
            {
                SerializedFields.Set(so, "_id", "mirror_stance");
                SerializedFields.Set(so, "_windup", 0.3f);
                SerializedFields.Set(so, "_active", 1.0f);
                SerializedFields.Set(so, "_recovery", 1.1f);
                SerializedFields.Set(so, "_punishWindow", 0.9f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_behaviour", parryBehaviour);
                SerializedFields.Set(so, "_weight", 1.1f);
                SerializedFields.Set(so, "_cooldown", 7f);
                SerializedFields.Set(so, "_maxRange", 12f);
            });

            var shatteredKaleidoscope = EnsureAsset<AttackData>($"{DataFolder}/ShatteredKaleidoscope.asset", so =>
            {
                SerializedFields.Set(so, "_id", "shattered_kaleidoscope");
                SerializedFields.Set(so, "_windup", 1.0f);
                SerializedFields.Set(so, "_active", 0.3f);
                SerializedFields.Set(so, "_recovery", 1.2f);
                SerializedFields.Set(so, "_punishWindow", 0.9f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_behaviour", kaleidoscopeBehaviour);
                SerializedFields.Set(so, "_weight", 1.3f);
                SerializedFields.Set(so, "_cooldown", 6f);
                SerializedFields.Set(so, "_maxRange", 25f);
            });

            // 5. BossData
            var duelistData = EnsureAsset<BossData>($"{DataFolder}/MirrorDuelistData.asset", so =>
            {
                SerializedFields.Set(so, "_id", "mirror_duelist");
                SerializedFields.Set(so, "_displayName", "The Mirror Duelist");
                SerializedFields.Set(so, "_floor", 5);
                SerializedFields.Set(so, "_accentColor", new Color(0.73f, 0.65f, 1f, 1f));
                SerializedFields.Set(so, "_baseHP", 950);
                SerializedFields.Set(so, "_hpScalePerExtraPlayer", 0.6f);
                SerializedFields.Set(so, "_phase2Threshold", 0.5f);
                SerializedFields.Set(so, "_maxPosture", 120f);
                SerializedFields.Set(so, "_postureRecoveryDelay", 3.5f);
                SerializedFields.Set(so, "_postureRecoveryRate", 30f);
                SerializedFields.Set(so, "_staggerDuration", 3.5f);
                SerializedFields.Set(so, "_staggerDamageMultiplier", 1.5f);
                SerializedFields.Set(so, "_walkSpeed", 5.0f);
                SerializedFields.Set(so, "_chaseDistance", 12f);
                SerializedFields.Set(so, "_chaseSpeedMultiplier", 2.0f);
                SerializedFields.Set(so, "_idleTime", 0.6f);
                SerializedFields.Set(so, "_targetSwitchInterval", 7f);
                SerializedFields.Set(so, "_attackStartLead", 0.1f);
                SerializedFields.Set(so, "_despawnDelay", 1.5f);
                SerializedFields.Set(so, "_arenaPrefab", arenaPrefab);

                var p1 = so.FindProperty("_phase1Attacks");
                p1.arraySize = 3;
                p1.GetArrayElementAtIndex(0).objectReferenceValue = prismaticThrust;
                p1.GetArrayElementAtIndex(1).objectReferenceValue = glassShardBurst;
                p1.GetArrayElementAtIndex(2).objectReferenceValue = mirrorStance;

                var p2 = so.FindProperty("_phase2Attacks");
                p2.arraySize = 4;
                p2.GetArrayElementAtIndex(0).objectReferenceValue = shatteredKaleidoscope;
                p2.GetArrayElementAtIndex(1).objectReferenceValue = prismaticThrust;
                p2.GetArrayElementAtIndex(2).objectReferenceValue = glassShardBurst;
                p2.GetArrayElementAtIndex(3).objectReferenceValue = mirrorStance;
            });

            // 6. Boss Prefab
            var duelistPrefab = EnsureDuelistPrefab(duelistData, projectilePrefab);
            RegisterNetworkPrefab(duelistPrefab);

            var duelistDataSo = new SerializedObject(duelistData);
            SerializedFields.Set(duelistDataSo, "_prefab", duelistPrefab);
            duelistDataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(duelistData);

            // 7. RunConfig wiring
            WireRunConfig(duelistData);

            // 8. BossSpawner wiring
            WireBossSpawner(duelistData);

            AssetDatabase.SaveAssets();
            Debug.Log("[MirrorDuelistSetup] The Mirror Duelist (Floor 5) successfully built and configured.");
        }

        private static GameObject EnsureDuelistPrefab(BossData data, GameObject projectilePrefab)
        {
            var templatePath = "Assets/_Project/Prefabs/Bosses/Gatekeeper.prefab";
            if (!File.Exists(DuelistPrefabPath))
            {
                AssetDatabase.CopyAsset(templatePath, DuelistPrefabPath);
            }

            var root = PrefabUtility.LoadPrefabContents(DuelistPrefabPath);
            root.name = "MirrorDuelist";

            WireComponent(root.GetComponent<Health>(), "_stats", data);
            WireComponent(root.GetComponent<BossBrain>(), "_data", data);
            WireComponent(root.GetComponent<BossMotor>(), "_data", data);
            WireComponent(root.GetComponent<BossTargetSelector>(), "_data", data);
            WireComponent(root.GetComponent<BossAttackRunner>(), "_data", data);

            if (root.GetComponent<BossPosture>() == null)
            {
                root.AddComponent<BossPosture>();
            }

            var parry = root.GetComponent<MirrorParryState>();
            if (parry == null)
            {
                parry = root.AddComponent<MirrorParryState>();
            }
            parry.SetCounterPrefab(projectilePrefab);

            var visuals = root.transform.Find("Visuals");
            if (visuals != null && visuals.TryGetComponent(out SpriteRenderer sr))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DuelistSpritePath);
                if (sprite != null) sr.sprite = sprite;
                sr.color = Color.white;
            }

            // Shield visual for parry
            var shieldTransform = root.transform.Find("ParryShield");
            if (shieldTransform == null)
            {
                var shieldGo = new GameObject("ParryShield");
                shieldGo.transform.SetParent(root.transform, false);
                shieldGo.transform.localPosition = new Vector3(0f, 1f, 0f);
                var shieldSr = shieldGo.AddComponent<SpriteRenderer>();
                shieldSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                shieldSr.color = new Color(0.73f, 0.65f, 1f, 0.65f);
                shieldGo.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
                shieldSr.enabled = false;

                var parrySo = new SerializedObject(parry);
                SerializedFields.Set(parrySo, "_shieldRenderer", shieldSr);
                SerializedFields.Set(parrySo, "_counterProjectilePrefab", projectilePrefab);
                parrySo.ApplyModifiedPropertiesWithoutUndo();
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, DuelistPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            return prefab;
        }

        private static void WireComponent(Component comp, string field, Object val)
        {
            if (comp == null) return;
            var so = new SerializedObject(comp);
            SerializedFields.Set(so, field, val);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RegisterNetworkPrefab(GameObject prefab)
        {
            if (prefab == null) return;
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkPrefabsPath);
            if (list != null && !list.Contains(prefab))
            {
                list.Add(new NetworkPrefab { Prefab = prefab });
                EditorUtility.SetDirty(list);
            }
        }

        private static void WireRunConfig(BossData data)
        {
            var config = AssetDatabase.LoadAssetAtPath<RunConfig>(RunConfigPath);
            if (config == null) return;

            var so = new SerializedObject(config);
            var floors = so.FindProperty("_floors");
            // Floor 5 is index 4
            if (floors.arraySize <= 4) floors.arraySize = 5;
            var floor5 = floors.GetArrayElementAtIndex(4).FindPropertyRelative("_bosses");
            floor5.arraySize = 1;
            floor5.GetArrayElementAtIndex(0).objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void WireBossSpawner(BossData data)
        {
            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            var spawner = Object.FindFirstObjectByType<BossSpawner>();
            if (spawner != null)
            {
                var so = new SerializedObject(spawner);
                var bosses = so.FindProperty("_bosses");
                var exists = false;
                for (var i = 0; i < bosses.arraySize; i++)
                {
                    if (bosses.GetArrayElementAtIndex(i).objectReferenceValue == data) exists = true;
                }
                if (!exists)
                {
                    bosses.InsertArrayElementAtIndex(bosses.arraySize);
                    bosses.GetArrayElementAtIndex(bosses.arraySize - 1).objectReferenceValue = data;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        private static TAsset EnsureAsset<TAsset>(string path, System.Action<SerializedObject> configure) where TAsset : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            configure(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
