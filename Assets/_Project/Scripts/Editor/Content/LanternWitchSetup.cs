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
    /// Grace Run → Build The Lantern Witch:
    /// Constructs Boss #7 (The Lantern Witch) for Milestone 7.
    /// Sets up LanternWitchArena, SplitLantern prefab, LanternWitchState,
    /// AttackData assets (Cursed Orb Fan, Phantom Beam, Fourfold Eclipse, Wisp Supernova),
    /// LanternWitchData, LanternWitch.prefab, DefaultNetworkPrefabs, RunConfig, and BossSpawner.
    /// </summary>
    public static class LanternWitchSetup
    {
        private const string DataFolder = "Assets/_Project/Data/Bosses/LanternWitch";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Bosses";
        private const string WitchPrefabPath = PrefabFolder + "/LanternWitch.prefab";
        private const string LanternPrefabPath = PrefabFolder + "/SplitLantern.prefab";
        private const string ProjectilePrefabPath = PrefabFolder + "/BossProjectile.prefab";
        private const string ArenaPrefabPath = "Assets/_Project/Prefabs/Arenas/LanternWitchArena.prefab";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
        private const string WitchSpritePath = "Assets/_Project/Art/Bosses/boss_lantern_witch.png";
        private const string CircleSpritePath = "Assets/_Project/Art/FX/Placeholder/circle.png";

        [InitializeOnLoadMethod]
        [MenuItem("Grace Run/Build The Lantern Witch")]
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
            var lanternPrefab = EnsureSplitLanternPrefab(projectilePrefab);
            RegisterNetworkPrefab(lanternPrefab);

            // 3. Behaviours
            var hexBarrageBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/LanternWitch_HexBarrage.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 6);
                SerializedFields.Set(so, "_spreadAngle", 55f);
                SerializedFields.Set(so, "_speed", 11.5f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_lifetime", 4.0f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var splitLanternsBehaviour = EnsureAsset<SplitLanternsBehaviour>($"{DataFolder}/LanternWitch_SplitLanterns.asset", so =>
            {
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_lanternPrefab", lanternPrefab);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
                SerializedFields.Set(so, "_count", 4);
                SerializedFields.Set(so, "_orbitRadius", 16f);
            });

            var supernovaBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/LanternWitch_Supernova.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Ring);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 12);
                SerializedFields.Set(so, "_speed", 10.0f);
                SerializedFields.Set(so, "_damage", 18);
                SerializedFields.Set(so, "_bounces", 1);
                SerializedFields.Set(so, "_lifetime", 4.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            // 4. Attacks
            var cursedOrbFan = EnsureAsset<AttackData>($"{DataFolder}/CursedOrbFan.asset", so =>
            {
                SerializedFields.Set(so, "_id", "cursed_orb_fan");
                SerializedFields.Set(so, "_windup", 0.85f);
                SerializedFields.Set(so, "_active", 0.2f);
                SerializedFields.Set(so, "_recovery", 0.95f);
                SerializedFields.Set(so, "_punishWindow", 0.75f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_behaviour", hexBarrageBehaviour);
                SerializedFields.Set(so, "_weight", 1.1f);
                SerializedFields.Set(so, "_cooldown", 4.5f);
                SerializedFields.Set(so, "_maxRange", 22f);
            });

            var phantomBeam = EnsureAsset<AttackData>($"{DataFolder}/PhantomBeam.asset", so =>
            {
                SerializedFields.Set(so, "_id", "phantom_beam");
                SerializedFields.Set(so, "_windup", 1.2f);
                SerializedFields.Set(so, "_active", 0.3f);
                SerializedFields.Set(so, "_recovery", 1.15f);
                SerializedFields.Set(so, "_punishWindow", 0.95f);
                SerializedFields.Set(so, "_damage", 32);
                SerializedFields.Set(so, "_knockback", new Vector2(14f, 6f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(10.0f, 0f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(20.0f, 2.5f));
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 6f);
                SerializedFields.Set(so, "_maxRange", 20f);
            });

            var fourfoldEclipse = EnsureAsset<AttackData>($"{DataFolder}/FourfoldEclipse.asset", so =>
            {
                SerializedFields.Set(so, "_id", "fourfold_eclipse");
                SerializedFields.Set(so, "_windup", 1.1f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 1.2f);
                SerializedFields.Set(so, "_punishWindow", 0.9f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_behaviour", splitLanternsBehaviour);
                SerializedFields.Set(so, "_weight", 1.5f);
                SerializedFields.Set(so, "_cooldown", 12f);
                SerializedFields.Set(so, "_maxRange", 30f);
            });

            var wispSupernova = EnsureAsset<AttackData>($"{DataFolder}/WispSupernova.asset", so =>
            {
                SerializedFields.Set(so, "_id", "wisp_supernova");
                SerializedFields.Set(so, "_windup", 1.0f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 1.1f);
                SerializedFields.Set(so, "_punishWindow", 0.85f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_behaviour", supernovaBehaviour);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 7f);
                SerializedFields.Set(so, "_maxRange", 26f);
            });

            // 5. BossData
            var witchData = EnsureAsset<BossData>($"{DataFolder}/LanternWitchData.asset", so =>
            {
                SerializedFields.Set(so, "_id", "lantern_witch");
                SerializedFields.Set(so, "_displayName", "The Lantern Witch");
                SerializedFields.Set(so, "_floor", 7);
                SerializedFields.Set(so, "_accentColor", new Color(0.91f, 0.86f, 0.75f, 1f));
                SerializedFields.Set(so, "_baseHP", 1150);
                SerializedFields.Set(so, "_hpScalePerExtraPlayer", 0.6f);
                SerializedFields.Set(so, "_phase2Threshold", 0.5f);
                SerializedFields.Set(so, "_maxPosture", 130f);
                SerializedFields.Set(so, "_postureRecoveryDelay", 3.5f);
                SerializedFields.Set(so, "_postureRecoveryRate", 25f);
                SerializedFields.Set(so, "_staggerDuration", 3.5f);
                SerializedFields.Set(so, "_staggerDamageMultiplier", 1.5f);
                SerializedFields.Set(so, "_walkSpeed", 4.2f);
                SerializedFields.Set(so, "_chaseDistance", 13f);
                SerializedFields.Set(so, "_chaseSpeedMultiplier", 1.8f);
                SerializedFields.Set(so, "_idleTime", 0.65f);
                SerializedFields.Set(so, "_targetSwitchInterval", 7f);
                SerializedFields.Set(so, "_attackStartLead", 0.1f);
                SerializedFields.Set(so, "_despawnDelay", 1.5f);
                SerializedFields.Set(so, "_arenaPrefab", arenaPrefab);

                var p1 = so.FindProperty("_phase1Attacks");
                p1.arraySize = 2;
                p1.GetArrayElementAtIndex(0).objectReferenceValue = cursedOrbFan;
                p1.GetArrayElementAtIndex(1).objectReferenceValue = phantomBeam;

                var p2 = so.FindProperty("_phase2Attacks");
                p2.arraySize = 4;
                p2.GetArrayElementAtIndex(0).objectReferenceValue = fourfoldEclipse;
                p2.GetArrayElementAtIndex(1).objectReferenceValue = wispSupernova;
                p2.GetArrayElementAtIndex(2).objectReferenceValue = cursedOrbFan;
                p2.GetArrayElementAtIndex(3).objectReferenceValue = phantomBeam;
            });

            // 6. Boss Prefab
            var witchPrefab = EnsureWitchPrefab(witchData);
            RegisterNetworkPrefab(witchPrefab);

            var witchDataSo = new SerializedObject(witchData);
            SerializedFields.Set(witchDataSo, "_prefab", witchPrefab);
            witchDataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(witchData);

            // 7. RunConfig wiring
            WireRunConfig(witchData);

            // 8. BossSpawner wiring
            WireBossSpawner(witchData);

            AssetDatabase.SaveAssets();
            Debug.Log("[LanternWitchSetup] The Lantern Witch (Floor 7) successfully built and configured.");
        }

        private static GameObject EnsureSplitLanternPrefab(GameObject projectilePrefab)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LanternPrefabPath);
            if (existing != null) return existing;

            var go = new GameObject("SplitLantern");
            go.AddComponent<NetworkObject>();

            var visuals = new GameObject("Visuals");
            visuals.transform.SetParent(go.transform, false);

            var haloGo = new GameObject("Halo");
            haloGo.transform.SetParent(visuals.transform, false);
            var haloSr = haloGo.AddComponent<SpriteRenderer>();
            haloSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            haloSr.color = new Color(0.95f, 0.88f, 0.72f, 0.4f);
            haloGo.transform.localScale = new Vector3(8f, 8f, 1f);

            var coreGo = new GameObject("Core");
            coreGo.transform.SetParent(visuals.transform, false);
            var coreSr = coreGo.AddComponent<SpriteRenderer>();
            coreSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            coreSr.color = new Color(1f, 0.95f, 0.75f, 1f);
            coreGo.transform.localScale = new Vector3(1.2f, 1.5f, 1f);

            var lantern = go.AddComponent<SplitLantern>();
            var so = new SerializedObject(lantern);
            SerializedFields.Set(so, "_coreRenderer", coreSr);
            SerializedFields.Set(so, "_haloRenderer", haloSr);
            SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, LanternPrefabPath);
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

        private static GameObject EnsureWitchPrefab(BossData data)
        {
            var templatePath = "Assets/_Project/Prefabs/Bosses/Gatekeeper.prefab";
            if (!File.Exists(WitchPrefabPath))
            {
                AssetDatabase.CopyAsset(templatePath, WitchPrefabPath);
            }

            var root = PrefabUtility.LoadPrefabContents(WitchPrefabPath);
            root.name = "LanternWitch";

            WireComponent(root.GetComponent<Health>(), "_stats", data);
            WireComponent(root.GetComponent<BossBrain>(), "_data", data);
            WireComponent(root.GetComponent<BossMotor>(), "_data", data);
            WireComponent(root.GetComponent<BossTargetSelector>(), "_data", data);
            WireComponent(root.GetComponent<BossAttackRunner>(), "_data", data);

            if (root.GetComponent<BossPosture>() == null)
            {
                root.AddComponent<BossPosture>();
            }

            var state = root.GetComponent<LanternWitchState>();
            if (state == null)
            {
                state = root.AddComponent<LanternWitchState>();
            }

            var visuals = root.transform.Find("Visuals");
            if (visuals != null && visuals.TryGetComponent(out SpriteRenderer sr))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WitchSpritePath);
                if (sprite != null) sr.sprite = sprite;
                sr.color = Color.white;
            }

            // Central Lantern child
            var lanternTransform = root.transform.Find("Lantern");
            SpriteRenderer lanternSr = null;
            if (lanternTransform == null)
            {
                var lanternGo = new GameObject("Lantern");
                lanternGo.transform.SetParent(root.transform, false);
                lanternGo.transform.localPosition = new Vector3(1.1f, 0.5f, 0f);
                lanternSr = lanternGo.AddComponent<SpriteRenderer>();
                lanternSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                lanternSr.color = new Color(1f, 0.95f, 0.65f, 1f);
                lanternGo.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
            }
            else
            {
                lanternSr = lanternTransform.GetComponent<SpriteRenderer>();
            }

            // Light halo
            var haloTransform = root.transform.Find("LightHalo");
            SpriteRenderer haloSr = null;
            if (haloTransform == null)
            {
                var haloGo = new GameObject("LightHalo");
                haloGo.transform.SetParent(root.transform, false);
                haloGo.transform.localPosition = new Vector3(1.1f, 0.5f, 0f);
                haloSr = haloGo.AddComponent<SpriteRenderer>();
                haloSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                haloSr.color = new Color(0.95f, 0.88f, 0.72f, 0.5f);
                haloGo.transform.localScale = new Vector3(14f, 14f, 1f);
            }
            else
            {
                haloSr = haloTransform.GetComponent<SpriteRenderer>();
            }

            // Gloom overlay
            var gloomTransform = root.transform.Find("GloomOverlay");
            SpriteRenderer gloomSr = null;
            if (gloomTransform == null)
            {
                var gloomGo = new GameObject("GloomOverlay");
                gloomGo.transform.SetParent(root.transform, false);
                gloomGo.transform.localPosition = Vector3.zero;
                gloomSr = gloomGo.AddComponent<SpriteRenderer>();
                gloomSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                gloomSr.color = new Color(0.06f, 0.05f, 0.10f, 0.45f);
                gloomGo.transform.localScale = new Vector3(120f, 60f, 1f);
                gloomSr.sortingOrder = 4;
            }
            else
            {
                gloomSr = gloomTransform.GetComponent<SpriteRenderer>();
            }

            var stateSo = new SerializedObject(state);
            SerializedFields.Set(stateSo, "_lanternRenderer", lanternSr);
            SerializedFields.Set(stateSo, "_lightHaloRenderer", haloSr);
            SerializedFields.Set(stateSo, "_gloomOverlay", gloomSr);
            stateSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, WitchPrefabPath);
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
            // Floor 7 is index 6
            if (floors.arraySize <= 6) floors.arraySize = 7;
            var floor7 = floors.GetArrayElementAtIndex(6).FindPropertyRelative("_bosses");
            floor7.arraySize = 1;
            floor7.GetArrayElementAtIndex(0).objectReferenceValue = data;
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
