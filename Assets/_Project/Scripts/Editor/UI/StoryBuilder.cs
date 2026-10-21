using System.IO;
using GraceRun.UI.Story;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.UI
{
    /// <summary>
    /// Grace Run → Build Story Assets: sets the import settings of the pictures in Art/Story (Sprite, point filter,
    /// no compression, no mips, 24 pixels per unit), then creates or refreshes <c>Resources/StoryData.asset</c>:
    /// loads the sprites by their exact names and reads tower_windows.txt. A new asset gets the words of
    /// docs/LORE.md; an existing one keeps whatever words it has, so hand edits survive. Safe to run again.
    /// </summary>
    public static class StoryBuilder
    {
        private const string Dir = "Assets/_Project/Art/Story";
        private const string DataPath = "Assets/_Project/Resources/StoryData.asset";
        private const int Trophies = 7;

        [MenuItem("Grace Run/Build Story Assets")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            if (Directory.Exists(Dir))
                foreach (var file in Directory.GetFiles(Dir, "*.png")) Import(file.Replace('\\', '/'));
            var data = AssetDatabase.LoadAssetAtPath<StoryData>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<StoryData>();
                StoryDefaults.Fill(data);
                AssetDatabase.CreateAsset(data, DataPath);
            }
            var so = new SerializedObject(data);
            for (var i = 0; i < 5; i++) SetObject(so, $"_opening.Array.data[{i}].Picture", S($"intro_{i + 1}"));
            for (var i = 0; i < 2; i++) SetObject(so, $"_ending.Array.data[{i}].Picture", S($"ending_{i + 1}"));
            SetObject(so, "_defeat.Picture", S("defeat_1"));
            SetObject(so, "_hollowSilhouette", S("hollow_silhouette"));
            SetObject(so, "_tower", S("tower"));
            SetObject(so, "_windowLit", S("window_lit"));
            SetObject(so, "_windowDark", S("window_dark"));
            SetObject(so, "_lantern", S("lantern"));
            var trophies = so.FindProperty("_trophies");
            trophies.arraySize = Trophies;
            for (var i = 0; i < Trophies; i++) trophies.GetArrayElementAtIndex(i).objectReferenceValue = S($"trophy_{i + 1}");
            var windows = so.FindProperty("_windowCentres");
            var centres = ReadWindows();
            if (centres.Length > 0)
            {
                windows.arraySize = centres.Length;
                for (var i = 0; i < centres.Length; i++) windows.GetArrayElementAtIndex(i).vector2Value = centres[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log($"Story assets built: {DataPath} ({centres.Length} window centres).");
        }

        private static Vector2[] ReadWindows()
        {
            var path = $"{Dir}/tower_windows.txt";
            return File.Exists(path) ? StoryRules.ParseWindows(File.ReadAllText(path)) : new Vector2[0];
        }

        private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{name}.png");

        private static void SetObject(SerializedObject so, string path, Object value)
        {
            var property = so.FindProperty(path);
            if (property != null) property.objectReferenceValue = value;
        }

        private static void Import(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 24f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
