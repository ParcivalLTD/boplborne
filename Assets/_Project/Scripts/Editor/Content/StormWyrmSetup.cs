using System.Collections.Generic;
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
    /// Grace Run → Build The Storm Wyrm:
    /// Constructs Boss #4 (The Storm Wyrm) for Milestone 7.
    /// Sets up StormWyrmArena, LightningColumn prefab, bullet & lightning behaviours,
    /// AttackData assets (Tail Sweep, Lightning Surge, Thunder Orb Fan, Tempest Dive, Overcharged Storm),
    /// StormWyrmData, StormWyrm.prefab, DefaultNetworkPrefabs, RunConfig, and BossSpawner.
    /// </summary>
    public static class StormWyrmSetup
    {
        private const string DataFolder = "Assets/_Project/Data/Bosses/StormWyrm";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Bosses";
        private const string WyrmPrefabPath = PrefabFolder + "/StormWyrm.prefab";
        private const string ColumnPrefabPath = PrefabFolder + "/LightningColumn.prefab";
        private const string ArenaPrefabPath = "Assets/_Project/Prefabs/Arenas/StormWyrmArena.prefab";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
        private const string WyrmSpritePath = "Assets/_Project/Art/Bosses/boss_storm_wyrm.png";
        private const string CircleSpritePath = "Assets/_Project/Art/FX/Placeholder/circle.png";

        [InitializeOnLoadMethod]
        [MenuItem("Grace Run/Build The Storm Wyrm")]
        public static void Apply()
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(PrefabFolder);

            // 1. Column Prefab
            var columnPrefab = EnsureColumnPrefab();
            RegisterNetworkPrefab(columnPrefab);

            // 2. Arena Layout
            ArenaLayoutSetup.Run();
            var arenaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArenaPrefabPath);
            RegisterNetworkPrefab(arenaPrefab);

            // 3. Projectile Prefab
            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/BossProjectile.prefab");

            // 4. Behaviours
            var columnsBehaviour = EnsureAsset<LightningColumnsBehaviour>($"{DataFolder}/StormWyrm_LightningColumns.asset", so =>
            {
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_columnPrefab", columnPrefab);
                SerializedFields.Set(so, "_count", 5);
                SerializedFields.Set(so, "_spacing", 11f);
                SerializedFields.Set(so, "_damage", 24);
                SerializedFields.Set(so, "_windup", 0.8f);
                SerializedFields.Set(so, "_activeTime", 0.25f);
                SerializedFields.Set(so, "_knockback", new Vector2(0f, 9f));
            });

            var overchargedBehaviour = EnsureAsset<LightningColumnsBehaviour>($"{DataFolder}/StormWyrm_OverchargedColumns.asset", so =>
            {
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_columnPrefab", columnPrefab);
                SerializedFields.Set(so, "_count", 7);
                SerializedFields.Set(so, "_spacing", 8.5f);
                SerializedFields.Set(so, "_damage", 28);
                SerializedFields.Set(so, "_windup", 0.7f);
                SerializedFields.Set(so, "_activeTime", 0.3f);
                SerializedFields.Set(so, "_knockback", new Vector2(0f, 10f));
            });

            var orbFanBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/StormWyrm_ThunderOrbFan.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 5);
                SerializedFields.Set(so, "_spreadAngle", 50f);
                SerializedFields.Set(so, "_speed", 12.5f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_lifetime", 3.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            // 5. Attacks
            var tailSweep = EnsureAsset<AttackData>($"{DataFolder}/TailSweep.asset", so =>
            {
                SerializedFields.Set(so, "_id", "tail_sweep");
                SerializedFields.Set(so, "_windup", 1.1f);
                SerializedFields.Set(so, "_active", 0.2f);
                SerializedFields.Set(so, "_recovery", 1.0f);
                SerializedFields.Set(so, "_punishWindow", 0.8f);
                SerializedFields.Set(so, "_damage", 26);
                SerializedFields.Set(so, "_knockback", new Vector2(12f, 6f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(4.5f, -1.0f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(9f, 3f));
                SerializedFields.Set(so, "_telegraphType", TelegraphType.GroundZone);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 4f);
                SerializedFields.Set(so, "_maxRange", 8f);
            });

            var lightningSurge = EnsureAsset<AttackData>($"{DataFolder}/LightningSurge.asset", so =>
            {
                SerializedFields.Set(so, "_id", "lightning_surge");
                SerializedFields.Set(so, "_windup", 1.0f);
                SerializedFields.Set(so, "_active", 0.3f);
                SerializedFields.Set(so, "_recovery", 1.2f);
                SerializedFields.Set(so, "_punishWindow", 0.9f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_behaviour", columnsBehaviour);
                SerializedFields.Set(so, "_weight", 1.0f);
                SerializedFields.Set(so, "_cooldown", 6f);
                SerializedFields.Set(so, "_maxRange", 25f);
            });

            var thunderOrbFan = EnsureAsset<AttackData>($"{DataFolder}/ThunderOrbFan.asset", so =>
            {
                SerializedFields.Set(so, "_id", "thunder_orb_fan");
                SerializedFields.Set(so, "_windup", 0.9f);
                SerializedFields.Set(so, "_active", 0.2f);
                SerializedFields.Set(so, "_recovery", 1.0f);
                SerializedFields.Set(so, "_punishWindow", 0.8f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_behaviour", orbFanBehaviour);
                SerializedFields.Set(so, "_weight", 1.1f);
                SerializedFields.Set(so, "_cooldown", 5f);
                SerializedFields.Set(so, "_maxRange", 20f);
            });

            var delayedTailSweep = EnsureAsset<AttackData>($"{DataFolder}/DelayedTailSweep.asset", so =>
            {
                SerializedFields.Set(so, "_id", "delayed_tail_sweep");
                SerializedFields.Set(so, "_windup", 0.8f);
                SerializedFields.Set(so, "_active", 0.2f);
                SerializedFields.Set(so, "_recovery", 1.1f);
                SerializedFields.Set(so, "_punishWindow", 0.85f);
                SerializedFields.Set(so, "_damage", 28);
                SerializedFields.Set(so, "_knockback", new Vector2(13f, 7f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(4.5f, -1.0f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(9.5f, 3.2f));
                SerializedFields.Set(so, "_telegraphType", TelegraphType.GroundZone);
            });

            var tempestDive = EnsureAsset<AttackData>($"{DataFolder}/TempestDive.asset", so =>
            {
                SerializedFields.Set(so, "_id", "tempest_dive");
                SerializedFields.Set(so, "_windup", 0.95f);
                SerializedFields.Set(so, "_active", 0.35f);
                SerializedFields.Set(so, "_recovery", 0.4f);
                SerializedFields.Set(so, "_punishWindow", 0.35f);
                SerializedFields.Set(so, "_damage", 30);
                SerializedFields.Set(so, "_knockback", new Vector2(14f, 8f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(4.0f, -0.5f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(8f, 3.5f));
                SerializedFields.Set(so, "_lungeDistance", 16f);
                SerializedFields.Set(so, "_lungeTime", 0.35f);
                SerializedFields.Set(so, "_followUp", delayedTailSweep);
                SerializedFields.Set(so, "_weight", 1.3f);
                SerializedFields.Set(so, "_cooldown", 8f);
                SerializedFields.Set(so, "_maxRange", 22f);
            });

            var overchargedStorm = EnsureAsset<AttackData>($"{DataFolder}/OverchargedStorm.asset", so =>
            {
                SerializedFields.Set(so, "_id", "overcharged_storm");
                SerializedFields.Set(so, "_windup", 1.2f);
                SerializedFields.Set(so, "_active", 0.3f);
                SerializedFields.Set(so, "_recovery", 1.3f);
                SerializedFields.Set(so, "_punishWindow", 1.0f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_behaviour", overchargedBehaviour);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 7f);
                SerializedFields.Set(so, "_maxRange", 25f);
            });

            // 6. BossData
            var wyrmData = EnsureAsset<BossData>($"{DataFolder}/StormWyrmData.asset", so =>
            {
                SerializedFields.Set(so, "_id", "storm_wyrm");
                SerializedFields.Set(so, "_displayName", "The Storm Wyrm");
                SerializedFields.Set(so, "_floor", 4);
                SerializedFields.Set(so, "_accentColor", new Color(0.37f, 0.72f, 0.69f, 1f));
                SerializedFields.Set(so, "_baseHP", 850);
                SerializedFields.Set(so, "_hpScalePerExtraPlayer", 0.6f);
                SerializedFields.Set(so, "_phase2Threshold", 0.5f);
                SerializedFields.Set(so, "_maxPosture", 140f);
                SerializedFields.Set(so, "_postureRecoveryDelay", 4f);
                SerializedFields.Set(so, "_postureRecoveryRate", 25f);
                SerializedFields.Set(so, "_staggerDuration", 3.5f);
                SerializedFields.Set(so, "_staggerDamageMultiplier", 1.5f);
                SerializedFields.Set(so, "_walkSpeed", 4.0f);
                SerializedFields.Set(so, "_chaseDistance", 12f);
                SerializedFields.Set(so, "_chaseSpeedMultiplier", 2.0f);
                SerializedFields.Set(so, "_idleTime", 0.7f);
                SerializedFields.Set(so, "_targetSwitchInterval", 8f);
                SerializedFields.Set(so, "_attackStartLead", 0.1f);
                SerializedFields.Set(so, "_despawnDelay", 1.5f);
                SerializedFields.Set(so, "_arenaPrefab", arenaPrefab);

                var p1 = so.FindProperty("_phase1Attacks");
                p1.arraySize = 3;
                p1.GetArrayElementAtIndex(0).objectReferenceValue = tailSweep;
                p1.GetArrayElementAtIndex(1).objectReferenceValue = lightningSurge;
                p1.GetArrayElementAtIndex(2).objectReferenceValue = thunderOrbFan;

                var p2 = so.FindProperty("_phase2Attacks");
                p2.arraySize = 4;
                p2.GetArrayElementAtIndex(0).objectReferenceValue = tempestDive;
                p2.GetArrayElementAtIndex(1).objectReferenceValue = overchargedStorm;
                p2.GetArrayElementAtIndex(2).objectReferenceValue = thunderOrbFan;
                p2.GetArrayElementAtIndex(3).objectReferenceValue = tailSweep;
            });

            // 7. Boss Prefab
            var wyrmPrefab = EnsureWyrmPrefab(wyrmData);
            RegisterNetworkPrefab(wyrmPrefab);

            var wyrmDataSo = new SerializedObject(wyrmData);
            SerializedFields.Set(wyrmDataSo, "_prefab", wyrmPrefab);
            wyrmDataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wyrmData);

            // 8. RunConfig wiring
            WireRunConfig(wyrmData);

            // 9. BossSpawner wiring
            WireBossSpawner(wyrmData);

            AssetDatabase.SaveAssets();
            Debug.Log("[StormWyrmSetup] The Storm Wyrm (Floor 4) successfully built and configured.");
        }

        private static GameObject EnsureColumnPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ColumnPrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("LightningColumn");
            var netObj = go.AddComponent<NetworkObject>();
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.4f, 22f);
            col.offset = new Vector2(0f, 11f);
            go.layer = Layers.BossAttackIndex;

            var hitbox = go.AddComponent<Hitbox>();
            var hitboxSo = new SerializedObject(hitbox);
            SerializedFields.Set(hitboxSo, "_attacker", netObj);
            SerializedFields.Set(hitboxSo, "_team", Team.Boss);
            SerializedFields.Set(hitboxSo, "_detection", HitDetection.VictimSide);
            hitboxSo.ApplyModifiedPropertiesWithoutUndo();

            var visuals = new GameObject("Visuals");
            visuals.transform.SetParent(go.transform, false);
            visuals.transform.localPosition = new Vector3(0f, 11f, 0f);
            var sr = visuals.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            sr.color = new Color(0.35f, 0.85f, 0.95f, 0.4f);
            visuals.transform.localScale = new Vector2(2.4f, 22f);

            var column = go.AddComponent<LightningColumn>();
            var colSo = new SerializedObject(column);
            SerializedFields.Set(colSo, "_hitbox", hitbox);
            SerializedFields.Set(colSo, "_renderer", sr);
            colSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ColumnPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject EnsureWyrmPrefab(BossData data)
        {
            var templatePath = "Assets/_Project/Prefabs/Bosses/Gatekeeper.prefab";
            if (!File.Exists(WyrmPrefabPath))
            {
                AssetDatabase.CopyAsset(templatePath, WyrmPrefabPath);
            }

            var root = PrefabUtility.LoadPrefabContents(WyrmPrefabPath);
            root.name = "StormWyrm";

            WireComponent(root.GetComponent<Health>(), "_stats", data);
            WireComponent(root.GetComponent<BossBrain>(), "_data", data);
            WireComponent(root.GetComponent<BossMotor>(), "_data", data);
            WireComponent(root.GetComponent<BossTargetSelector>(), "_data", data);
            WireComponent(root.GetComponent<BossAttackRunner>(), "_data", data);

            if (root.GetComponent<BossPosture>() == null)
            {
                root.AddComponent<BossPosture>();
            }

            var visuals = root.transform.Find("Visuals");
            if (visuals != null && visuals.TryGetComponent(out SpriteRenderer sr))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WyrmSpritePath);
                if (sprite != null) sr.sprite = sprite;
                sr.color = Color.white;
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, WyrmPrefabPath);
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
            // Floor 4 is index 3
            if (floors.arraySize <= 3) floors.arraySize = 4;
            var floor4 = floors.GetArrayElementAtIndex(3).FindPropertyRelative("_bosses");
            floor4.arraySize = 1;
            floor4.GetArrayElementAtIndex(0).objectReferenceValue = data;
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
