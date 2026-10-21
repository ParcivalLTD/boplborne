using System.IO;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Bell Twins Art (docs/ART_PIPELINE.md step 4): imports the cleaned pose
    /// PNGs in Art/Bosses/BellTwins, fills one <see cref="BossPoseSet"/> per twin and wires
    /// <see cref="BossPoseView"/> (twin A, brass) and <see cref="BellTwinPoseSelector"/> (twin B,
    /// silver) into the BellTwins prefab. Safe to run again.
    /// </summary>
    public static class BellTwinsArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Bosses/BellTwins";
        private const string DataDir = "Assets/_Project/Data/Bosses/BellTwins";
        private const string PrefabPath = "Assets/_Project/Prefabs/Bosses/BellTwins.prefab";
        private const float FeetOffset = -3f; // the boss root is 3 units above the floor

        [MenuItem("Grace Run/Build Bell Twins Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var path in Directory.GetFiles(ArtDir, "*.png")) Import(path.Replace('\\', '/'));
            var a = BuildPoseSet("a", "BellTwinAPoses");
            var b = BuildPoseSet("b", "BellTwinBPoses");
            WirePrefab(a, b);
            AssetDatabase.SaveAssets();
            Debug.Log("Bell Twins art built.");
        }

        private static Sprite S(string twin, string name) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/belltwin_{twin}_{name}.png");

        private static BossPoseSet BuildPoseSet(string twin, string assetName)
        {
            var path = $"{DataDir}/{assetName}.asset";
            var set = AssetDatabase.LoadAssetAtPath<BossPoseSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<BossPoseSet>();
                AssetDatabase.CreateAsset(set, path);
            }
            var so = new SerializedObject(set);
            SetSprites(so.FindProperty("_idle"), S(twin, "idle_0"), S(twin, "idle_1"), S(twin, "idle_2"), S(twin, "idle_3"));
            so.FindProperty("_stagger").objectReferenceValue = S(twin, "stagger");
            SetSprites(so.FindProperty("_death"), S(twin, "death_0"), S(twin, "death_1"), S(twin, "death_2"), S(twin, "death_3"));

            var list = so.FindProperty("_attacks");
            list.arraySize = 3;
            SetAttack(list.GetArrayElementAtIndex(0), twin, "swing", "clapper_swing", "clapper_swing_double", "clapper_swing_second");
            SetAttack(list.GetArrayElementAtIndex(1), twin, "toll", "ringing_toll", "enraged_ringing_toll", "enraged_ringing_echo");
            SetAttack(list.GetArrayElementAtIndex(2), twin, "vault", "vaulting_dash");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void SetAttack(SerializedProperty e, string twin, string pose, params string[] ids)
        {
            var list = e.FindPropertyRelative("AttackIds");
            list.arraySize = ids.Length;
            for (var i = 0; i < ids.Length; i++) list.GetArrayElementAtIndex(i).stringValue = ids[i];
            e.FindPropertyRelative("Windup").objectReferenceValue = S(twin, pose + "_windup");
            e.FindPropertyRelative("Strike").objectReferenceValue = S(twin, pose + "_strike");
            e.FindPropertyRelative("Recovery").objectReferenceValue = S(twin, pose + "_recovery");
        }

        private static void SetSprites(SerializedProperty list, params Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void WirePrefab(BossPoseSet a, BossPoseSet b)
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var visuals = root.transform.Find("Visuals");
                foreach (var placeholder in new[] { "Clapper", "Crown", "Eye" }) visuals.Find(placeholder).gameObject.SetActive(false);
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                Place(body, a.Idle[0]);

                var view = root.GetComponent<BossPoseView>();
                if (view == null) view = root.AddComponent<BossPoseView>();
                var so = new SerializedObject(view);
                so.FindProperty("_poses").objectReferenceValue = a;
                so.FindProperty("_runner").objectReferenceValue = root.GetComponent<BossAttackRunner>();
                so.FindProperty("_brain").objectReferenceValue = root.GetComponent<BossBrain>();
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = body;
                so.ApplyModifiedProperties();

                var selector = root.GetComponent<BellTwinPoseSelector>();
                if (selector == null) selector = root.AddComponent<BellTwinPoseSelector>();
                var sel = new SerializedObject(selector);
                sel.FindProperty("_partnerPoses").objectReferenceValue = b;
                sel.FindProperty("_link").objectReferenceValue = root.GetComponent<BellTwinLink>();
                sel.FindProperty("_view").objectReferenceValue = view;
                sel.FindProperty("_body").objectReferenceValue = body;
                sel.ApplyModifiedProperties();
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
