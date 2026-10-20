using System.Collections.Generic;
using System.IO;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using GraceRun.Core;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Set Up Elden Ring & Cuphead Bosses:
    /// 1. Adds BossPosture component to all boss prefabs.
    /// 2. Creates the BossProjectile prefab and adds it to DefaultNetworkPrefabs.
    /// 3. Builds and wires Cuphead bullet pattern behaviours (spreads, rings, bouncing venom, sonic rings).
    /// 4. Tunes boss posture thresholds and Elden Ring delayed wind-ups on Gatekeeper, Brood Mother, and Bell Twins.
    /// </summary>
    public static class BossOverhaulSetup
    {
        private const string BossFolder = "Assets/_Project/Prefabs/Bosses";
        private const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Bosses/BossProjectile.prefab";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string CirclePath = "Assets/_Project/Art/FX/Placeholder/circle.png";

        [InitializeOnLoadMethod]
        [MenuItem("Grace Run/Set Up Elden Ring & Cuphead Bosses")]
        public static void Apply()
        {
            var projectilePrefab = EnsureProjectilePrefab();
            RegisterNetworkPrefab(projectilePrefab);
            AddPostureToBossPrefabs();
            ConfigureGatekeeper(projectilePrefab);
            ConfigureBroodMother(projectilePrefab);
            ConfigureBellTwins(projectilePrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[BossOverhaulSetup] Elden Ring & Cuphead boss setup complete.");
        }

        private static GameObject EnsureProjectilePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(ProjectilePrefabPath));
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
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            sr.color = new Color(1f, 0.65f, 0.15f, 1f);
            visuals.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
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

        private static void AddPostureToBossPrefabs()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { BossFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == ProjectilePrefabPath) continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                var changed = false;
                if (root.GetComponent<BossBrain>() != null && root.GetComponent<BossPosture>() == null)
                {
                    root.AddComponent<BossPosture>();
                    changed = true;
                }
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureGatekeeper(GameObject projectilePrefab)
        {
            const string folder = "Assets/_Project/Data/Bosses/Gatekeeper";
            var data = AssetDatabase.LoadAssetAtPath<BossData>($"{folder}/GatekeeperData.asset");
            if (data == null) return;

            var dataSo = new SerializedObject(data);
            SerializedFields.Set(dataSo, "_maxPosture", 120f);
            SerializedFields.Set(dataSo, "_postureRecoveryDelay", 4f);
            SerializedFields.Set(dataSo, "_postureRecoveryRate", 25f);
            SerializedFields.Set(dataSo, "_staggerDuration", 3.5f);
            SerializedFields.Set(dataSo, "_staggerDamageMultiplier", 1.5f);
            dataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);

            // 1. Shockwaves on Overhead Slam
            var slamPattern = EnsureAsset<BulletPatternBehaviour>($"{folder}/Gatekeeper_SlamShockwaves.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 3);
                SerializedFields.Set(so, "_spreadAngle", 35f);
                SerializedFields.Set(so, "_speed", 10f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_lifetime", 2.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var slam = AssetDatabase.LoadAssetAtPath<AttackData>($"{folder}/OverheadSlam.asset");
            if (slam != null)
            {
                var slamSo = new SerializedObject(slam);
                SerializedFields.Set(slamSo, "_windup", 1.05f); // Elden Ring delayed wind-up
                SerializedFields.Set(slamSo, "_behaviour", slamPattern);
                slamSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(slam);
            }

            var slamDouble = AssetDatabase.LoadAssetAtPath<AttackData>($"{folder}/OverheadSlamDouble.asset");
            if (slamDouble != null)
            {
                var slamDoubleSo = new SerializedObject(slamDouble);
                SerializedFields.Set(slamDoubleSo, "_windup", 1.1f);
                slamDoubleSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(slamDouble);
            }
        }

        private static void ConfigureBroodMother(GameObject projectilePrefab)
        {
            const string folder = "Assets/_Project/Data/Bosses/BroodMother";
            var data = AssetDatabase.LoadAssetAtPath<BossData>($"{folder}/BroodMotherData.asset");
            if (data == null) return;

            var dataSo = new SerializedObject(data);
            SerializedFields.Set(dataSo, "_maxPosture", 150f);
            SerializedFields.Set(dataSo, "_postureRecoveryDelay", 4.5f);
            SerializedFields.Set(dataSo, "_postureRecoveryRate", 25f);
            SerializedFields.Set(dataSo, "_staggerDuration", 4.0f);
            SerializedFields.Set(dataSo, "_staggerDamageMultiplier", 1.5f);
            dataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);

            // Bouncing venom spread
            var venomPattern = EnsureAsset<BulletPatternBehaviour>($"{folder}/BroodMother_VenomBarrage.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 5);
                SerializedFields.Set(so, "_spreadAngle", 45f);
                SerializedFields.Set(so, "_speed", 11f);
                SerializedFields.Set(so, "_damage", 15);
                SerializedFields.Set(so, "_bounces", 1);
                SerializedFields.Set(so, "_gravity", 12f);
                SerializedFields.Set(so, "_lifetime", 3.5f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var spit = AssetDatabase.LoadAssetAtPath<AttackData>($"{folder}/WebSpit.asset");
            if (spit != null)
            {
                var spitSo = new SerializedObject(spit);
                SerializedFields.Set(spitSo, "_behaviour", venomPattern);
                spitSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(spit);
            }

            var drop = AssetDatabase.LoadAssetAtPath<AttackData>($"{folder}/CeilingDrop.asset");
            if (drop != null)
            {
                var dropSo = new SerializedObject(drop);
                SerializedFields.Set(dropSo, "_windup", 1.3f); // Delayed anticipation
                dropSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(drop);
            }
        }

        private static void ConfigureBellTwins(GameObject projectilePrefab)
        {
            const string folder = "Assets/_Project/Data/Bosses/BellTwins";
            var data = AssetDatabase.LoadAssetAtPath<BossData>($"{folder}/BellTwinsData.asset");
            if (data == null) return;

            var dataSo = new SerializedObject(data);
            SerializedFields.Set(dataSo, "_maxPosture", 90f);
            SerializedFields.Set(dataSo, "_postureRecoveryDelay", 3.5f);
            SerializedFields.Set(dataSo, "_postureRecoveryRate", 20f);
            SerializedFields.Set(dataSo, "_staggerDuration", 3.5f);
            SerializedFields.Set(dataSo, "_staggerDamageMultiplier", 1.5f);
            dataSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);

            // High/low stacked sonic rings
            var sonicPattern = EnsureAsset<BulletPatternBehaviour>($"{folder}/BellTwins_SonicRings.asset", so =>
            {
                SerializedFields.Set(so, "_patternType", BulletPatternType.Spread);
                SerializedFields.Set(so, "_triggerPhase", AttackPhase.Active);
                SerializedFields.Set(so, "_count", 3);
                SerializedFields.Set(so, "_spreadAngle", 30f);
                SerializedFields.Set(so, "_speed", 13f);
                SerializedFields.Set(so, "_damage", 16);
                SerializedFields.Set(so, "_lifetime", 3.0f);
                SerializedFields.Set(so, "_projectilePrefab", projectilePrefab);
            });

            var toll = AssetDatabase.LoadAssetAtPath<AttackData>($"{folder}/RingingToll.asset");
            if (toll != null)
            {
                var tollSo = new SerializedObject(toll);
                SerializedFields.Set(tollSo, "_behaviour", sonicPattern);
                tollSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(toll);
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
