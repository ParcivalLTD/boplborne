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
    /// Grace Run → Build The Clockwork Colossus:
    /// Constructs Boss #6 (The Clockwork Colossus) for Milestone 7.
    /// Sets up ClockworkColossusArena, SteamVent hazard, ClockworkConveyor, ClockworkColossusState,
    /// AttackData assets (Quake Stomp, Steam Vents, Bouncing Gears, Pendulum Sweep, Overdrive Vents),
    /// ClockworkColossusData, ClockworkColossus.prefab, DefaultNetworkPrefabs, RunConfig, and BossSpawner.
    /// </summary>
    public static class ClockworkColossusSetup
    {
        private const string DataFolder = "Assets/_Project/Data/Bosses/ClockworkColossus";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Bosses";
        private const string ColossusPrefabPath = PrefabFolder + "/ClockworkColossus.prefab";
        private const string VentPrefabPath = PrefabFolder + "/SteamVent.prefab";
        private const string ProjectilePrefabPath = PrefabFolder + "/BossProjectile.prefab";
        private const string ShockwavePrefabPath = PrefabFolder + "/BellShockwave.prefab";
        private const string ArenaPrefabPath = "Assets/_Project/Prefabs/Arenas/ClockworkColossusArena.prefab";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
        private const string ColossusSpritePath = "Assets/_Project/Art/Bosses/boss_clockwork_colossus.png";
        private const string CircleSpritePath = "Assets/_Project/Art/FX/Placeholder/circle.png";

        [InitializeOnLoadMethod]
        [MenuItem("Grace Run/Build The Clockwork Colossus")]
        public static void Apply()
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(PrefabFolder);

            // 1. Arena Layout
            ArenaLayoutSetup.Run();
            var arenaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArenaPrefabPath);
            RegisterNetworkPrefab(arenaPrefab);

            // 2. Prefabs
            var projectilePrefab = EnsureProjectilePrefab();
            RegisterNetworkPrefab(projectilePrefab);
            var shockwavePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShockwavePrefabPath);
            var ventPrefab = EnsureVentPrefab();
            RegisterNetworkPrefab(ventPrefab);

            // 3. Behaviours
            var shockwavePrefabComp = shockwavePrefab != null ? shockwavePrefab.GetComponent<BellShockwave>() : null;
            var stompShockwaveBehaviour = EnsureAsset<ShockwaveTollBehaviour>($"{DataFolder}/ClockworkColossus_StompShockwaves.asset", so =>
            {
                SerializedFields.Set(so, "_shockwavePrefab", shockwavePrefabComp);
                SerializedFields.Set(so, "_spawnOffsetX", 2.2f);
                SerializedFields.Set(so, "_floorY", -4.5f);
                SerializedFields.Set(so, "_speedOverride", 12.0f);
            });

            var steamVentsBehaviour = EnsureAsset<SteamVentsBehaviour>($"{DataFolder}/ClockworkColossus_SteamVents.asset", so =>
            {
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_ventPrefab", ventPrefab);
                SerializedFields.Set(so, "_count", 5);
                SerializedFields.Set(so, "_spacing", 11f);
                SerializedFields.Set(so, "_groundY", -5f);
                SerializedFields.Set(so, "_windup", 0.85f);
                SerializedFields.Set(so, "_activeTime", 0.25f);
                SerializedFields.Set(so, "_damage", 22);
                SerializedFields.Set(so, "_knockback", new Vector2(0f, 12f));
            });

            var overdriveVentsBehaviour = EnsureAsset<SteamVentsBehaviour>($"{DataFolder}/ClockworkColossus_OverdriveVents.asset", so =>
            {
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_ventPrefab", ventPrefab);
                SerializedFields.Set(so, "_count", 7);
                SerializedFields.Set(so, "_spacing", 9f);
                SerializedFields.Set(so, "_groundY", -5f);
                SerializedFields.Set(so, "_windup", 0.75f);
                SerializedFields.Set(so, "_activeTime", 0.25f);
                SerializedFields.Set(so, "_damage", 25);
                SerializedFields.Set(so, "_knockback", new Vector2(0f, 14f));
            });

            var bouncingGearsBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/ClockworkColossus_BouncingGears.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 5);
                SerializedFields.Set(so, "_spreadAngle", 45f);
                SerializedFields.Set(so, "_speed", 12f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_bounces", 2);
                SerializedFields.Set(so, "_gravity", 2.5f);
                SerializedFields.Set(so, "_lifetime", 4.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            // 4. Attacks
            var quakeStomp = EnsureAsset<AttackData>($"{DataFolder}/QuakeStomp.asset", so =>
            {
                SerializedFields.Set(so, "_id", "quake_stomp");
                SerializedFields.Set(so, "_windup", 1.25f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 1.1f);
                SerializedFields.Set(so, "_punishWindow", 0.85f);
                SerializedFields.Set(so, "_damage", 32);
                SerializedFields.Set(so, "_knockback", new Vector2(12f, 8f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(3.5f, -1.5f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(5.0f, 4.0f));
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(-2.5f, 2.5f));
                SerializedFields.Set(so, "_behaviour", stompShockwaveBehaviour);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 5f);
                SerializedFields.Set(so, "_maxRange", 10f);
            });

            var steamVents = EnsureAsset<AttackData>($"{DataFolder}/SteamVents.asset", so =>
            {
                SerializedFields.Set(so, "_id", "steam_vents");
                SerializedFields.Set(so, "_windup", 0.9f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 1.0f);
                SerializedFields.Set(so, "_punishWindow", 0.8f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(-2.0f, 2.5f));
                SerializedFields.Set(so, "_behaviour", steamVentsBehaviour);
                SerializedFields.Set(so, "_weight", 1.0f);
                SerializedFields.Set(so, "_cooldown", 6f);
                SerializedFields.Set(so, "_maxRange", 25f);
            });

            var bouncingGears = EnsureAsset<AttackData>($"{DataFolder}/BouncingGears.asset", so =>
            {
                SerializedFields.Set(so, "_id", "bouncing_gears");
                SerializedFields.Set(so, "_windup", 0.85f);
                SerializedFields.Set(so, "_active", 0.2f);
                SerializedFields.Set(so, "_recovery", 1.05f);
                SerializedFields.Set(so, "_punishWindow", 0.8f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(-2.0f, 2.5f));
                SerializedFields.Set(so, "_behaviour", bouncingGearsBehaviour);
                SerializedFields.Set(so, "_weight", 1.1f);
                SerializedFields.Set(so, "_cooldown", 6f);
                SerializedFields.Set(so, "_maxRange", 22f);
            });

            var pendulumSweep = EnsureAsset<AttackData>($"{DataFolder}/PendulumSweep.asset", so =>
            {
                SerializedFields.Set(so, "_id", "pendulum_sweep");
                SerializedFields.Set(so, "_windup", 1.1f);
                SerializedFields.Set(so, "_active", 0.35f);
                SerializedFields.Set(so, "_recovery", 1.3f);
                SerializedFields.Set(so, "_punishWindow", 1.0f);
                SerializedFields.Set(so, "_damage", 36);
                SerializedFields.Set(so, "_knockback", new Vector2(16f, 6f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(5.0f, -0.5f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(11.0f, 4.5f));
                SerializedFields.Set(so, "_lungeDistance", 8.0f);
                SerializedFields.Set(so, "_lungeTime", 0.25f);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(-2.5f, 3.0f));
                SerializedFields.Set(so, "_weight", 1.3f);
                SerializedFields.Set(so, "_cooldown", 7f);
                SerializedFields.Set(so, "_maxRange", 18f);
            });

            var overdriveVents = EnsureAsset<AttackData>($"{DataFolder}/OverdriveVents.asset", so =>
            {
                SerializedFields.Set(so, "_id", "overdrive_vents");
                SerializedFields.Set(so, "_windup", 0.8f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 0.9f);
                SerializedFields.Set(so, "_punishWindow", 0.75f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(-2.0f, 2.5f));
                SerializedFields.Set(so, "_behaviour", overdriveVentsBehaviour);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 7f);
                SerializedFields.Set(so, "_maxRange", 28f);
            });

            // 5. BossData
            var colossusData = EnsureAsset<BossData>($"{DataFolder}/ClockworkColossusData.asset", so =>
            {
                SerializedFields.Set(so, "_id", "clockwork_colossus");
                SerializedFields.Set(so, "_displayName", "The Clockwork Colossus");
                SerializedFields.Set(so, "_floor", 6);
                SerializedFields.Set(so, "_accentColor", new Color(0.69f, 0.48f, 0.35f, 1f));
                SerializedFields.Set(so, "_baseHP", 1050);
                SerializedFields.Set(so, "_hpScalePerExtraPlayer", 0.6f);
                SerializedFields.Set(so, "_phase2Threshold", 0.5f);
                SerializedFields.Set(so, "_maxPosture", 160f);
                SerializedFields.Set(so, "_postureRecoveryDelay", 4.0f);
                SerializedFields.Set(so, "_postureRecoveryRate", 25f);
                SerializedFields.Set(so, "_staggerDuration", 4.0f);
                SerializedFields.Set(so, "_staggerDamageMultiplier", 1.5f);
                SerializedFields.Set(so, "_walkSpeed", 3.2f);
                SerializedFields.Set(so, "_chaseDistance", 14f);
                SerializedFields.Set(so, "_chaseSpeedMultiplier", 1.6f);
                SerializedFields.Set(so, "_idleTime", 0.7f);
                SerializedFields.Set(so, "_targetSwitchInterval", 7f);
                SerializedFields.Set(so, "_attackStartLead", 0.1f);
                SerializedFields.Set(so, "_despawnDelay", 1.5f);
                SerializedFields.Set(so, "_arenaPrefab", arenaPrefab);

                var p1 = so.FindProperty("_phase1Attacks");
                p1.arraySize = 3;
                p1.GetArrayElementAtIndex(0).objectReferenceValue = quakeStomp;
                p1.GetArrayElementAtIndex(1).objectReferenceValue = steamVents;
                p1.GetArrayElementAtIndex(2).objectReferenceValue = bouncingGears;

                var p2 = so.FindProperty("_phase2Attacks");
                p2.arraySize = 5;
                p2.GetArrayElementAtIndex(0).objectReferenceValue = pendulumSweep;
                p2.GetArrayElementAtIndex(1).objectReferenceValue = overdriveVents;
                p2.GetArrayElementAtIndex(2).objectReferenceValue = quakeStomp;
                p2.GetArrayElementAtIndex(3).objectReferenceValue = steamVents;
                p2.GetArrayElementAtIndex(4).objectReferenceValue = bouncingGears;
            });

            // 6. Boss Prefab
            var colossusPrefab = EnsureColossusPrefab(colossusData);
            RegisterNetworkPrefab(colossusPrefab);

            var colossusDataSo = new SerializedObject(colossusData);
            SerializedFields.Set(colossusDataSo, "_prefab", colossusPrefab);
            colossusDataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(colossusData);

            // 7. RunConfig wiring
            WireRunConfig(colossusData);

            // 8. BossSpawner wiring
            WireBossSpawner(colossusData);

            AssetDatabase.SaveAssets();
            Debug.Log("[ClockworkColossusSetup] The Clockwork Colossus (Floor 6) successfully built and configured.");
        }

        private static GameObject EnsureVentPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(VentPrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("SteamVent");
            var netObj = go.AddComponent<NetworkObject>();
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 12f);
            col.offset = new Vector2(0f, 6f);
            go.layer = Layers.BossAttackIndex;

            var hitbox = go.AddComponent<Hitbox>();
            var hitboxSo = new SerializedObject(hitbox);
            SerializedFields.Set(hitboxSo, "_attacker", netObj);
            SerializedFields.Set(hitboxSo, "_team", Team.Boss);
            SerializedFields.Set(hitboxSo, "_detection", HitDetection.VictimSide);
            hitboxSo.ApplyModifiedPropertiesWithoutUndo();

            var visuals = new GameObject("Visuals");
            visuals.transform.SetParent(go.transform, false);
            visuals.transform.localPosition = new Vector3(0f, 6f, 0f);
            var sr = visuals.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            sr.color = new Color(0.85f, 0.65f, 0.45f, 0.4f);
            visuals.transform.localScale = new Vector2(2.2f, 12f);

            var vent = go.AddComponent<SteamVent>();
            var ventSo = new SerializedObject(vent);
            SerializedFields.Set(ventSo, "_hitbox", hitbox);
            SerializedFields.Set(ventSo, "_renderer", sr);
            ventSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, VentPrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject EnsureProjectilePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("BossProjectile");
            var netObj = go.AddComponent<NetworkObject>();
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
            go.layer = Layers.BossAttackIndex;

            var hitbox = go.AddComponent<Hitbox>();
            var hitboxSo = new SerializedObject(hitbox);
            SerializedFields.Set(hitboxSo, "_attacker", netObj);
            SerializedFields.Set(hitboxSo, "_team", Team.Boss);
            SerializedFields.Set(hitboxSo, "_detection", HitDetection.VictimSide);
            hitboxSo.ApplyModifiedPropertiesWithoutUndo();

            var proj = go.AddComponent<BossProjectile>();
            var projSo = new SerializedObject(proj);
            SerializedFields.Set(projSo, "_hitbox", hitbox);
            projSo.ApplyModifiedPropertiesWithoutUndo();

            var visuals = new GameObject("Visuals");
            visuals.transform.SetParent(go.transform, false);
            var sr = visuals.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            sr.color = new Color(1f, 0.65f, 0.15f, 1f);
            visuals.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject EnsureColossusPrefab(BossData data)
        {
            var templatePath = "Assets/_Project/Prefabs/Bosses/Gatekeeper.prefab";
            if (!File.Exists(ColossusPrefabPath))
            {
                AssetDatabase.CopyAsset(templatePath, ColossusPrefabPath);
            }

            var root = PrefabUtility.LoadPrefabContents(ColossusPrefabPath);
            root.name = "ClockworkColossus";

            WireComponent(root.GetComponent<Health>(), "_stats", data);
            WireComponent(root.GetComponent<BossBrain>(), "_data", data);
            WireComponent(root.GetComponent<BossMotor>(), "_data", data);
            WireComponent(root.GetComponent<BossTargetSelector>(), "_data", data);
            WireComponent(root.GetComponent<BossAttackRunner>(), "_data", data);

            if (root.GetComponent<BossPosture>() == null)
            {
                root.AddComponent<BossPosture>();
            }

            var conveyor = root.GetComponent<ClockworkConveyor>();
            if (conveyor == null)
            {
                conveyor = root.AddComponent<ClockworkConveyor>();
            }

            var state = root.GetComponent<ClockworkColossusState>();
            if (state == null)
            {
                state = root.AddComponent<ClockworkColossusState>();
            }

            var visuals = root.transform.Find("Visuals");
            if (visuals != null && visuals.TryGetComponent(out SpriteRenderer sr))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ColossusSpritePath);
                if (sprite != null) sr.sprite = sprite;
                sr.color = Color.white;
            }

            var coreTransform = root.transform.Find("BackCore");
            SpriteRenderer coreSr = null;
            if (coreTransform == null)
            {
                var coreGo = new GameObject("BackCore");
                coreGo.transform.SetParent(root.transform, false);
                coreGo.transform.localPosition = new Vector3(-1.8f, 2.2f, 0f);
                coreSr = coreGo.AddComponent<SpriteRenderer>();
                coreSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                coreSr.color = new Color(1f, 0.79f, 0.29f, 0.6f);
                coreGo.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            }
            else
            {
                coreSr = coreTransform.GetComponent<SpriteRenderer>();
            }

            var stateSo = new SerializedObject(state);
            SerializedFields.Set(stateSo, "_backCoreRenderer", coreSr);
            SerializedFields.Set(stateSo, "_conveyor", conveyor);
            stateSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ColossusPrefabPath);
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
            // Floor 6 is index 5
            if (floors.arraySize <= 5) floors.arraySize = 6;
            var floor6 = floors.GetArrayElementAtIndex(5).FindPropertyRelative("_bosses");
            floor6.arraySize = 1;
            floor6.GetArrayElementAtIndex(0).objectReferenceValue = data;
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
