using System.IO;
using GraceRun.UI;
using GraceRun.UI.Skin;
using GraceRun.UI.TitleCards;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GraceRun.Editor.UI.SkinHelpers;

namespace GraceRun.Editor.UI
{
    /// <summary>
    /// Grace Run → Build UI (docs/PROMPTS.md 8.9): imports the pixel fonts and the 9-slice sprites in
    /// Art/UI, fills <c>Resources/UiTheme.asset</c> and <c>Resources/TitleCards.asset</c>, and restyles the
    /// Game scene's HUD, boss bar, fight overlay and draft screen plus the PlayerPanel prefab. Safe to run
    /// again: it only writes the look (fonts, sprites, colours, sizes), never behaviour.
    /// </summary>
    public static class UiBuilder
    {
        private const string FontDir = "Assets/_Project/Art/UI/Fonts";
        private const string SpriteDir = "Assets/_Project/Art/UI/Cinema";
        private const string ThemePath = "Assets/_Project/Resources/UiTheme.asset";
        private const string CardsPath = "Assets/_Project/Resources/TitleCards.asset";
        private const string GameScene = "Assets/_Project/Scenes/Game.unity";
        private const string PanelPrefab = "Assets/_Project/Prefabs/UI/PlayerPanel.prefab";

        [MenuItem("Grace Run/Build UI")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            ImportFonts();
            ImportSprites();
            var theme = BuildTheme();
            BuildTitleCards();
            SkinPrefab(theme);
            SkinScene(theme);
            AssetDatabase.SaveAssets();
            Debug.Log("UI built.");
        }

        private static void ImportFonts()
        {
            foreach (var path in Directory.GetFiles(FontDir, "*.ttf"))
            {
                var importer = (TrueTypeFontImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                importer.fontSize = 16;
                importer.fontRenderingMode = FontRenderingMode.HintedRaster;
                importer.fontTextureCase = FontTextureCase.Dynamic;
                importer.includeFontData = true;
                importer.SaveAndReimport();
            }
        }

        private static void ImportSprites()
        {
            foreach (var file in Directory.GetFiles(SpriteDir, "*.png"))
            {
                var path = file.Replace('\\', '/');
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
                var border = BorderFor(Path.GetFileNameWithoutExtension(path));
                importer.spriteBorder = new Vector4(border, border, border, border);
                importer.SaveAndReimport();
            }
        }

        private static float BorderFor(string name) => name switch
        {
            "panel_cream" or "panel_paper" or "panel_ink" => 6f,
            "button" => 8f,
            "bar_frame" => 5f,
            "slot" => 5f,
            "field" => 6f,
            "card_frame" => 12f,
            "title_card" => 14f,
            _ => 0f,
        };

        private static Sprite LoadSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/{name}.png");

        private static UiTheme BuildTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ThemePath);
            if (theme == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ThemePath));
                theme = ScriptableObject.CreateInstance<UiTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }
            var so = new SerializedObject(theme);
            so.FindProperty("_display").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>($"{FontDir}/GraceDisplay.ttf");
            so.FindProperty("_body").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>($"{FontDir}/GraceBody.ttf");
            Set(so, "_panelCream", "panel_cream");
            Set(so, "_panelPaper", "panel_paper");
            Set(so, "_panelInk", "panel_ink");
            Set(so, "_button", "button");
            Set(so, "_barFrame", "bar_frame");
            Set(so, "_slot", "slot");
            Set(so, "_field", "field");
            Set(so, "_cardFrame", "card_frame");
            Set(so, "_titleCard", "title_card");
            Set(so, "_arrow", "arrow_left");
            Set(so, "_flaskPip", "flask_pip");
            Set(so, "_divider", "divider");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(theme);
            return theme;
        }

        private static void Set(SerializedObject so, string field, string sprite) =>
            so.FindProperty(field).objectReferenceValue = LoadSprite(sprite);

        private static void BuildTitleCards()
        {
            var data = AssetDatabase.LoadAssetAtPath<TitleCardData>(CardsPath);
            if (data != null) return; // our words are tuned in the asset after the first build
            data = ScriptableObject.CreateInstance<TitleCardData>();
            AssetDatabase.CreateAsset(data, CardsPath);
            var so = new SerializedObject(data);
            var entries = so.FindProperty("_entries");
            string[,] lines =
            {
                { "gatekeeper", "No key, no entry, no excuses.", "The gate swings open." },
                { "brood_mother", "Mind the webs, and mind the children.", "The nursery falls silent." },
                { "bell_twins", "Two chimes, one nasty ring.", "The last echo fades." },
                { "test_boss", "A very patient tutorial.", "Lesson learned." },
                { "final_test_boss", "The big finish, rehearsed.", "A flawless rehearsal." },
            };
            entries.arraySize = lines.GetLength(0);
            for (var i = 0; i < lines.GetLength(0); i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("BossId").stringValue = lines[i, 0];
                e.FindPropertyRelative("Tagline").stringValue = lines[i, 1];
                e.FindPropertyRelative("VictoryTagline").stringValue = lines[i, 2];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SkinPrefab(UiTheme theme)
        {
            var root = PrefabUtility.LoadPrefabContents(PanelPrefab);
            try
            {
                HudSkin.PlayerPanel(root.transform, theme);
                PrefabUtility.SaveAsPrefabAsset(root, PanelPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SkinScene(UiTheme theme)
        {
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects())
            {
                switch (go.name)
                {
                    case "HudCanvas":
                        foreach (var panel in go.GetComponentsInChildren<GraceRun.UI.PlayerPanel>(true))
                            HudSkin.PlayerPanel(panel.transform, theme);
                        PanelsRow(go.transform);
                        break;
                    case "BossBarCanvas": HudSkin.BossBar(go.transform, theme); break;
                    case "FightScreenCanvas": OverlaySkin.FightScreen(go.transform, theme); break;
                    case "DraftScreenCanvas": OverlaySkin.DraftScreen(go.transform, theme); break;
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void PanelsRow(Transform hud)
        {
            var row = hud.Find("Panels") as RectTransform;
            if (row != null) row.sizeDelta = new Vector2(row.sizeDelta.x, 150f);
        }
    }
}
