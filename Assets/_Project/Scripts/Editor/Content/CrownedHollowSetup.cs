using System.IO;
using GraceRun.Abilities;
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
    /// Grace Run → Build The Crowned Hollow:
    /// Constructs Boss #8 / Final Boss (The Crowned Hollow) for Milestone 7.
    /// Sets up CrownedHollowArena, CrownedHollowState, MimicAbilityBehaviour,
    /// AttackData assets (Royal Slam, Hollow Hex Barrage, Hollow Beam, Hollow Cataclysm, Mimicked Assault),
    /// CrownedHollowData, CrownedHollow.prefab, DefaultNetworkPrefabs, RunConfig, and BossSpawner.
    /// </summary>
    public static class CrownedHollowSetup
    {
        private const string DataFolder = "Assets/_Project/Data/Bosses/CrownedHollow";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Bosses";
        private const string HollowPrefabPath = PrefabFolder + "/CrownedHollow.prefab";
        private const string ProjectilePrefabPath = PrefabFolder + "/BossProjectile.prefab";
        private const string ShockwavePrefabPath = PrefabFolder + "/BellShockwave.prefab";
        private const string ArenaPrefabPath = "Assets/_Project/Prefabs/Arenas/CrownedHollowArena.prefab";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string AbilityCatalogPath = "Assets/_Project/Data/Config/AbilityCatalog.asset";
        private const string FallbackAbilityPath = "Assets/_Project/Data/Abilities/BlackHole.asset";
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";
        private const string HollowSpritePath = "Assets/_Project/Art/Bosses/boss_crowned_hollow.png";
        private const string CircleSpritePath = "Assets/_Project/Art/FX/Placeholder/circle.png";

        [InitializeOnLoadMethod]
        [MenuItem("Grace Run/Build The Crowned Hollow")]
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
            var shockwavePrefabComp = shockwavePrefab != null ? shockwavePrefab.GetComponent<BellShockwave>() : null;
            var catalog = AssetDatabase.LoadAssetAtPath<AbilityCatalog>(AbilityCatalogPath);
            var fallbackAbility = AssetDatabase.LoadAssetAtPath<AbilityData>(FallbackAbilityPath);

            // 3. Behaviours
            var stompShockwaves = EnsureAsset<ShockwaveTollBehaviour>($"{DataFolder}/CrownedHollow_StompShockwaves.asset", so =>
            {
                SerializedFields.Set(so, "_shockwavePrefab", shockwavePrefabComp);
                SerializedFields.Set(so, "_spawnOffsetX", 2.2f);
                SerializedFields.Set(so, "_floorY", -4.5f);
                SerializedFields.Set(so, "_speedOverride", 13.0f);
            });

            var hexBarrageBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/CrownedHollow_HexBarrage.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 7);
                SerializedFields.Set(so, "_spreadAngle", 60f);
                SerializedFields.Set(so, "_speed", 12.5f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_bounces", 1);
                SerializedFields.Set(so, "_lifetime", 4.0f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var cataclysmBehaviour = EnsureAsset<BulletPatternBehaviour>($"{DataFolder}/CrownedHollow_Cataclysm.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Ring);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 16);
                SerializedFields.Set(so, "_speed", 10.5f);
                SerializedFields.Set(so, "_damage", 18);
                SerializedFields.Set(so, "_bounces", 1);
                SerializedFields.Set(so, "_lifetime", 4.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var mimicBehaviour = EnsureAsset<MimicAbilityBehaviour>($"{DataFolder}/CrownedHollow_Mimic.asset", so =>
            {
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_catalog", catalog);
                SerializedFields.Set(so, "_fallbackAbility", fallbackAbility);
                SerializedFields.Set(so, "_powerMultiplier", 1.0f);
            });

            // 4. Attacks
            var royalSlam = EnsureAsset<AttackData>($"{DataFolder}/RoyalSlam.asset", so =>
            {
                SerializedFields.Set(so, "_id", "royal_slam");
                SerializedFields.Set(so, "_windup", 1.1f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 1.0f);
                SerializedFields.Set(so, "_punishWindow", 0.8f);
                SerializedFields.Set(so, "_damage", 32);
                SerializedFields.Set(so, "_knockback", new Vector2(12f, 8f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(4.0f, -1.0f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(6.0f, 3.5f));
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_behaviour", stompShockwaves);
                SerializedFields.Set(so, "_weight", 1.2f);
                SerializedFields.Set(so, "_cooldown", 4.5f);
                SerializedFields.Set(so, "_maxRange", 12f);
            });

            var hollowHexBarrage = EnsureAsset<AttackData>($"{DataFolder}/HollowHexBarrage.asset", so =>
            {
                SerializedFields.Set(so, "_id", "hollow_hex_barrage");
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

            var hollowBeam = EnsureAsset<AttackData>($"{DataFolder}/HollowBeam.asset", so =>
            {
                SerializedFields.Set(so, "_id", "hollow_beam");
                SerializedFields.Set(so, "_windup", 1.15f);
                SerializedFields.Set(so, "_active", 0.3f);
                SerializedFields.Set(so, "_recovery", 1.1f);
                SerializedFields.Set(so, "_punishWindow", 0.9f);
                SerializedFields.Set(so, "_damage", 34);
                SerializedFields.Set(so, "_knockback", new Vector2(16f, 6f));
                SerializedFields.Set(so, "_hitboxOffset", new Vector2(10.0f, 0f));
                SerializedFields.Set(so, "_hitboxSize", new Vector2(20.0f, 2.5f));
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_weight", 1.0f);
                SerializedFields.Set(so, "_cooldown", 6f);
                SerializedFields.Set(so, "_maxRange", 22f);
            });

            var hollowCataclysm = EnsureAsset<AttackData>($"{DataFolder}/HollowCataclysm.asset", so =>
            {
                SerializedFields.Set(so, "_id", "hollow_cataclysm");
                SerializedFields.Set(so, "_windup", 1.25f);
                SerializedFields.Set(so, "_active", 0.3f);
                SerializedFields.Set(so, "_recovery", 1.35f);
                SerializedFields.Set(so, "_punishWindow", 1.05f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_behaviour", cataclysmBehaviour);
                SerializedFields.Set(so, "_weight", 1.4f);
                SerializedFields.Set(so, "_cooldown", 8f);
                SerializedFields.Set(so, "_maxRange", 30f);
            });

            var mimickedAssault = EnsureAsset<AttackData>($"{DataFolder}/MimickedAssault.asset", so =>
            {
                SerializedFields.Set(so, "_id", "mimicked_assault");
                SerializedFields.Set(so, "_windup", 0.9f);
                SerializedFields.Set(so, "_active", 0.25f);
                SerializedFields.Set(so, "_recovery", 1.05f);
                SerializedFields.Set(so, "_punishWindow", 0.8f);
                SerializedFields.Set(so, "_damage", 0);
                SerializedFields.Set(so, "_weakPointOffset", new Vector2(0f, 2.0f));
                SerializedFields.Set(so, "_behaviour", mimicBehaviour);
                SerializedFields.Set(so, "_weight", 1.3f);
                SerializedFields.Set(so, "_cooldown", 7f);
                SerializedFields.Set(so, "_maxRange", 25f);
            });

            // 5. BossData
            var hollowData = EnsureAsset<BossData>($"{DataFolder}/CrownedHollowData.asset", so =>
            {
                SerializedFields.Set(so, "_id", "crowned_hollow");
                SerializedFields.Set(so, "_displayName", "The Crowned Hollow");
                SerializedFields.Set(so, "_floor", 8);
                SerializedFields.Set(so, "_accentColor", new Color(0.78f, 0.49f, 0.60f, 1f));
                SerializedFields.Set(so, "_baseHP", 1350);
                SerializedFields.Set(so, "_hpScalePerExtraPlayer", 0.6f);
                SerializedFields.Set(so, "_phase2Threshold", 0.5f);
                SerializedFields.Set(so, "_maxPosture", 180f);
                SerializedFields.Set(so, "_postureRecoveryDelay", 4.5f);
                SerializedFields.Set(so, "_postureRecoveryRate", 25f);
                SerializedFields.Set(so, "_staggerDuration", 4.0f);
                SerializedFields.Set(so, "_staggerDamageMultiplier", 1.5f);
                SerializedFields.Set(so, "_walkSpeed", 4.5f);
                SerializedFields.Set(so, "_chaseDistance", 14f);
                SerializedFields.Set(so, "_chaseSpeedMultiplier", 2.0f);
                SerializedFields.Set(so, "_idleTime", 0.6f);
                SerializedFields.Set(so, "_targetSwitchInterval", 6.5f);
                SerializedFields.Set(so, "_attackStartLead", 0.1f);
                SerializedFields.Set(so, "_despawnDelay", 2.0f);
                SerializedFields.Set(so, "_arenaPrefab", arenaPrefab);

                var p1 = so.FindProperty("_phase1Attacks");
                p1.arraySize = 3;
                p1.GetArrayElementAtIndex(0).objectReferenceValue = royalSlam;
                p1.GetArrayElementAtIndex(1).objectReferenceValue = hollowHexBarrage;
                p1.GetArrayElementAtIndex(2).objectReferenceValue = hollowBeam;

                var p2 = so.FindProperty("_phase2Attacks");
                p2.arraySize = 5;
                p2.GetArrayElementAtIndex(0).objectReferenceValue = hollowCataclysm;
                p2.GetArrayElementAtIndex(1).objectReferenceValue = mimickedAssault;
                p2.GetArrayElementAtIndex(2).objectReferenceValue = royalSlam;
                p2.GetArrayElementAtIndex(3).objectReferenceValue = hollowHexBarrage;
                p2.GetArrayElementAtIndex(4).objectReferenceValue = hollowBeam;
            });

            // 6. Boss Prefab
            var hollowPrefab = EnsureHollowPrefab(hollowData);
            RegisterNetworkPrefab(hollowPrefab);

            var hollowDataSo = new SerializedObject(hollowData);
            SerializedFields.Set(hollowDataSo, "_prefab", hollowPrefab);
            hollowDataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hollowData);

            // 7. RunConfig wiring
            WireRunConfig(hollowData);

            // 8. BossSpawner wiring
            WireBossSpawner(hollowData);

            AssetDatabase.SaveAssets();
            Debug.Log("[CrownedHollowSetup] The Crowned Hollow (Final Boss, Floor 8) successfully built and configured.");
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
            sr.color = new Color(0.78f, 0.49f, 0.60f, 1f);
            visuals.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject EnsureHollowPrefab(BossData data)
        {
            var templatePath = "Assets/_Project/Prefabs/Bosses/Gatekeeper.prefab";
            if (!File.Exists(HollowPrefabPath))
            {
                AssetDatabase.CopyAsset(templatePath, HollowPrefabPath);
            }

            var root = PrefabUtility.LoadPrefabContents(HollowPrefabPath);
            root.name = "CrownedHollow";

            WireComponent(root.GetComponent<Health>(), "_stats", data);
            WireComponent(root.GetComponent<BossBrain>(), "_data", data);
            WireComponent(root.GetComponent<BossMotor>(), "_data", data);
            WireComponent(root.GetComponent<BossTargetSelector>(), "_data", data);
            WireComponent(root.GetComponent<BossAttackRunner>(), "_data", data);

            if (root.GetComponent<BossPosture>() == null)
            {
                root.AddComponent<BossPosture>();
            }

            var state = root.GetComponent<CrownedHollowState>();
            if (state == null)
            {
                state = root.AddComponent<CrownedHollowState>();
            }

            var visuals = root.transform.Find("Visuals");
            if (visuals != null && visuals.TryGetComponent(out SpriteRenderer sr))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HollowSpritePath);
                if (sprite != null) sr.sprite = sprite;
                sr.color = Color.white;
            }

            // Floating Crown child
            var crownTransform = root.transform.Find("Crown");
            SpriteRenderer crownSr = null;
            if (crownTransform == null)
            {
                var crownGo = new GameObject("Crown");
                crownGo.transform.SetParent(root.transform, false);
                crownGo.transform.localPosition = new Vector3(0f, 2.8f, 0f);
                crownSr = crownGo.AddComponent<SpriteRenderer>();
                crownSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                crownSr.color = new Color(0.78f, 0.49f, 0.60f, 0.85f);
                crownGo.transform.localScale = new Vector3(1.2f, 0.7f, 1f);
            }
            else
            {
                crownSr = crownTransform.GetComponent<SpriteRenderer>();
            }

            // Shadowy Aura child
            var auraTransform = root.transform.Find("Aura");
            SpriteRenderer auraSr = null;
            if (auraTransform == null)
            {
                var auraGo = new GameObject("Aura");
                auraGo.transform.SetParent(root.transform, false);
                auraGo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                auraSr = auraGo.AddComponent<SpriteRenderer>();
                auraSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                auraSr.color = new Color(0.78f, 0.49f, 0.60f, 0.35f);
                auraGo.transform.localScale = new Vector3(5.5f, 5.5f, 1f);
            }
            else
            {
                auraSr = auraTransform.GetComponent<SpriteRenderer>();
            }

            var stateSo = new SerializedObject(state);
            SerializedFields.Set(stateSo, "_crownRenderer", crownSr);
            SerializedFields.Set(stateSo, "_auraRenderer", auraSr);
            stateSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, HollowPrefabPath);
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
            // Floor 8 is index 7
            if (floors.arraySize <= 7) floors.arraySize = 8;
            var floor8 = floors.GetArrayElementAtIndex(7).FindPropertyRelative("_bosses");
            floor8.arraySize = 1;
            floor8.GetArrayElementAtIndex(0).objectReferenceValue = data;
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
