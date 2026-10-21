using System.IO;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Training Dummy Art: imports the cleaned PNGs in Art/Props/Dummy and wires
    /// <see cref="TrainingDummyView"/> into the TrainingDummy prefab. Safe to run again.
    /// </summary>
    public static class TrainingDummyArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Props/Dummy";
        private const string Prefab = "Assets/_Project/Prefabs/TrainingDummy.prefab";
        private const string DataPath = "Assets/_Project/Data/Config/TrainingDummyData.asset";
        // The collider is 2.5 tall around the root; the art's feet sit 3 px (0.125 units) above its bottom row.
        private const float FeetOffset = -1.25f - 3f / 24f;

        [MenuItem("Grace Run/Build Training Dummy Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var path in Directory.GetFiles(ArtDir, "*.png")) Import(path.Replace('\\', '/'));

            var root = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                var visuals = root.transform.Find("Visuals");
                visuals.Find("Eye").gameObject.SetActive(false);
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                var idle = Load("idle", 8);
                body.sprite = idle[0];
                body.color = Color.white; // the placeholder grey would multiply the art's colours down
                body.transform.localPosition = new Vector3(0f, FeetOffset, 0f);
                body.transform.localScale = Vector3.one;

                var view = root.GetComponent<TrainingDummyView>() ?? root.AddComponent<TrainingDummyView>();
                var so = new SerializedObject(view);
                so.FindProperty("_data").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TrainingDummyData>(DataPath);
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = body;
                SetSprites(so.FindProperty("_idle"), idle);
                SetSprites(so.FindProperty("_hit"), Load("hit", 4));
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, Prefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Training dummy art built.");
        }

        private static Sprite[] Load(string clip, int count)
        {
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++) frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{clip}_{i:00}.png");
            return frames;
        }

        private static void SetSprites(SerializedProperty list, Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
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
