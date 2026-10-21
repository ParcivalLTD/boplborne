using System.IO;
using GraceRun.Bosses;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Gatekeeper Art (docs/ART_PIPELINE.md step 4): imports the cleaned pose
    /// PNGs in Art/Bosses/Gatekeeper, fills <c>GatekeeperPoses.asset</c> and wires
    /// <see cref="BossPoseView"/> and the sprites into the Gatekeeper prefab. Safe to run again.
    /// </summary>
    public static class GatekeeperArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Bosses/Gatekeeper";
        private const string PosesPath = "Assets/_Project/Data/Bosses/Gatekeeper/GatekeeperPoses.asset";
        private const string PrefabPath = "Assets/_Project/Prefabs/Bosses/Gatekeeper.prefab";
        private const float FeetOffset = -3f; // the boss root is 3 units above the floor

        [MenuItem("Grace Run/Build Gatekeeper Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var path in Directory.GetFiles(ArtDir, "*.png")) Import(path.Replace('\\', '/'));
            var poses = BuildPoseSet();
            WirePrefab(poses);
            AssetDatabase.SaveAssets();
            Debug.Log("Gatekeeper art built.");
        }

        private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/gatekeeper_{name}.png");

        private static BossPoseSet BuildPoseSet()
        {
            var set = AssetDatabase.LoadAssetAtPath<BossPoseSet>(PosesPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<BossPoseSet>();
                AssetDatabase.CreateAsset(set, PosesPath);
            }
            var so = new SerializedObject(set);
            SetSprites(so.FindProperty("_idle"), S("idle_0"), S("idle_1"), S("idle_2"), S("idle_3"));
            so.FindProperty("_stagger").objectReferenceValue = S("stagger");
            SetSprites(so.FindProperty("_death"), S("death_0"), S("death_1"), S("death_2"), S("death_3"));

            var list = so.FindProperty("_attacks");
            list.arraySize = 3;
            SetAttack(list.GetArrayElementAtIndex(0), "slam", "overhead_slam", "overhead_slam_double", "overhead_slam_p2", "delayed_second_slam");
            SetAttack(list.GetArrayElementAtIndex(1), "bash", "shield_bash");
            SetAttack(list.GetArrayElementAtIndex(2), "sweep", "sweeping_cut");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void SetAttack(SerializedProperty e, string pose, params string[] ids)
        {
            var list = e.FindPropertyRelative("AttackIds");
            list.arraySize = ids.Length;
            for (var i = 0; i < ids.Length; i++) list.GetArrayElementAtIndex(i).stringValue = ids[i];
            e.FindPropertyRelative("Windup").objectReferenceValue = S(pose + "_windup");
            e.FindPropertyRelative("Strike").objectReferenceValue = S(pose + "_strike");
            e.FindPropertyRelative("Recovery").objectReferenceValue = S(pose + "_recovery");
        }

        private static void SetSprites(SerializedProperty list, params Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void WirePrefab(BossPoseSet poses)
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var visuals = root.transform.Find("Visuals");
                foreach (var placeholder in new[] { "Sword", "Shield", "Eye" }) visuals.Find(placeholder).gameObject.SetActive(false);
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                Place(body, poses.Idle[0]);

                var view = root.GetComponent<BossPoseView>() ?? root.AddComponent<BossPoseView>();
                var so = new SerializedObject(view);
                so.FindProperty("_poses").objectReferenceValue = poses;
                so.FindProperty("_runner").objectReferenceValue = root.GetComponent<BossAttackRunner>();
                so.FindProperty("_brain").objectReferenceValue = root.GetComponent<BossBrain>();
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = body;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>The art faces left and the Visuals root is written for a boss facing right, so the sprites are flipped.</summary>
        private static void Place(SpriteRenderer renderer, Sprite sprite)
        {
            renderer.sprite = sprite;
            renderer.flipX = true;
            renderer.color = Color.white; // the placeholder accent tint would multiply the art's colours down
            renderer.transform.localPosition = new Vector3(0f, FeetOffset, 0f);
            renderer.transform.localScale = Vector3.one;
        }

        private static void Import(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 24;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
