using System.IO;
using System.Linq;
using GraceRun.Editor.Content;
using GraceRun.UI.Toon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GraceRun.Editor.UI
{
    /// <summary>
    /// Grace Run → Build Toon UI: imports the cartoon sprites in Art/UI/Toon (tools/art/build_toon_ui.py;
    /// 100 PPU, point filter, 9-slice borders) and fills <c>Resources/ToonTheme.asset</c>, including the
    /// menu mascot (the blue blob's idle frames and plume hat). Safe to run again.
    /// </summary>
    public static class ToonBuilder
    {
        private const string Dir = "Assets/_Project/Art/UI/Toon";
        private const string ThemePath = "Assets/_Project/Resources/ToonTheme.asset";
        private const string IdleDir = "Assets/_Project/Art/Characters/Player/idle";
        private const string HatPath = "Assets/_Project/Art/Characters/hat_plume.png";
        private const string GameScene = "Assets/_Project/Scenes/Game.unity";
        private const string PanelPrefab = "Assets/_Project/Prefabs/UI/PlayerPanel.prefab";

        [MenuItem("Grace Run/Build Toon UI")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(Dir, "*.png")) Import(file.Replace('\\', '/'));
            var theme = AssetDatabase.LoadAssetAtPath<ToonTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ToonTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }
            var so = new SerializedObject(theme);
            var buttons = so.FindProperty("_buttons");
            var tints = new[] { "cream", "orange", "mint", "sky", "pink", "stone", "plum" };
            buttons.arraySize = tints.Length;
            for (var i = 0; i < tints.Length; i++) buttons.GetArrayElementAtIndex(i).objectReferenceValue = S($"btn_{tints[i]}");
            var ribbons = so.FindProperty("_ribbons");
            var ribbonNames = new[] { "orange", "mint", "sky", "pink", "stone", "cream" };
            ribbons.arraySize = ribbonNames.Length;
            for (var i = 0; i < ribbonNames.Length; i++) ribbons.GetArrayElementAtIndex(i).objectReferenceValue = S($"ribbon_{ribbonNames[i]}");
            Set(so, "_roundButton", "btn_round");
            Set(so, "_roundButtonOrange", "btn_round_orange");
            Set(so, "_poster", "poster");
            Set(so, "_posterPaper", "poster_paper");
            Set(so, "_posterPlum", "poster_plum");
            Set(so, "_ticket", "ticket");
            Set(so, "_badge", "badge");
            Set(so, "_badgeOrange", "badge_orange");
            Set(so, "_bulbOn", "bulb_on");
            Set(so, "_bulbOff", "bulb_off");
            Set(so, "_star", "star");
            Set(so, "_starOrange", "star_orange");
            Set(so, "_sparkle", "sparkle");
            Set(so, "_curtain", "curtain");
            Set(so, "_valance", "valance");
            Set(so, "_spotlight", "spotlight");
            Set(so, "_stripes", "stripes");
            Set(so, "_barFrame", "bar_frame");
            so.FindProperty("_fistIcon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Weapons/weapon_glove.png");
            FillMascot(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            ApplyToGame();
            AssetDatabase.SaveAssets();
            Debug.Log("Toon UI built.");
        }

        /// <summary>Restyles the Game scene overlays and the PlayerPanel prefab (also called by Build UI after the cinema skin).</summary>
        public static void ApplyToGame()
        {
            if (ToonTheme.Current == null) return;
            var root = PrefabUtility.LoadPrefabContents(PanelPrefab);
            try
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                ToonSkin.PlayerPanel(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, PanelPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            foreach (var sceneRoot in scene.GetRootGameObjects())
                foreach (var t in sceneRoot.GetComponentsInChildren<Transform>(true)) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            foreach (var go in scene.GetRootGameObjects())
            {
                switch (go.name)
                {
                    case "HudCanvas":
                        foreach (var panel in go.GetComponentsInChildren<GraceRun.UI.PlayerPanel>(true)) ToonSkin.PlayerPanel(panel.transform);
                        break;
                    case "BossBarCanvas": ToonSkin.BossBar(go.transform); break;
                    case "FightScreenCanvas": ToonSkin.FightScreen(go.transform); break;
                    case "DraftScreenCanvas": ToonSkin.DraftScreen(go.transform); break;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{name}.png");

        private static void Set(SerializedObject so, string field, string sprite) =>
            so.FindProperty(field).objectReferenceValue = S(sprite);

        private static void FillMascot(SerializedObject so)
        {
            var paths = Directory.GetFiles(IdleDir, "idle_*.png").Select(p => p.Replace('\\', '/')).OrderBy(p => p).ToArray();
            var frames = so.FindProperty("_mascotFrames");
            var hats = so.FindProperty("_mascotHatPixels");
            frames.arraySize = paths.Length;
            hats.arraySize = paths.Length;
            var anchor = PlayerAnimationBuilder.AnchorHead();
            for (var i = 0; i < paths.Length; i++)
            {
                frames.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]);
                hats.GetArrayElementAtIndex(i).vector2Value = PlayerAnimationBuilder.HeadOf(paths[i], 0) - anchor;
            }
            so.FindProperty("_mascotHat").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(HatPath);
        }

        private static void Import(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spriteBorder = BorderFor(Path.GetFileNameWithoutExtension(path));
            importer.SaveAndReimport();
        }

        /// <summary>9-slice borders in art pixels: left, bottom, right, top.</summary>
        private static Vector4 BorderFor(string name)
        {
            if (name.StartsWith("btn_") && !name.Contains("round")) return new Vector4(9f, 12f, 9f, 9f);
            if (name.StartsWith("poster")) return new Vector4(9f, 11f, 11f, 9f);
            if (name == "ticket") return new Vector4(12f, 12f, 12f, 12f);
            if (name.StartsWith("ribbon_")) return new Vector4(13f, 0f, 13f, 0f);
            if (name == "hud_plate") return new Vector4(7f, 8f, 7f, 6f);
            if (name == "bar_frame_thin") return new Vector4(3f, 3f, 3f, 3f);
            if (name.StartsWith("bar_frame")) return new Vector4(7f, 7f, 7f, 7f);
            return Vector4.zero;
        }
    }
}
