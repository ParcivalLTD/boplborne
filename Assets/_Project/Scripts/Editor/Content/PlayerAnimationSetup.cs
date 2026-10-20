using System;
using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using GraceRun.Player;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Configures the art import pipeline (<c>docs/ART_PIPELINE.md</c>: 16 PPU, Point filter,
    /// Uncompressed, Bottom-Center character pivot), slices the character sprite sheets, and wires
    /// <see cref="Animator"/> and <see cref="PlayerAnimator"/> onto <c>Player.prefab</c>.
    /// </summary>
    public static class PlayerAnimationSetup
    {
        private const string AsepritePath = "Assets/_Project/Art/Characters/player.aseprite";
        private const string AnimDataPath = "Assets/_Project/Data/Player/PlayerAnimationData.asset";
        private const string MovementDataPath = "Assets/_Project/Data/Player/PlayerMovementData.asset";
        private const string GameConfigPath = "Assets/_Project/Data/Config/GameConfig.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        private static readonly (string tag, int count)[] SheetRows =
        {
            (PlayerAnimationRules.Idle, 4),
            (PlayerAnimationRules.Run, 6),
            (PlayerAnimationRules.Jump, 2),
            (PlayerAnimationRules.Fall, 1),
            (PlayerAnimationRules.Dodge, 5),
            (PlayerAnimationRules.Attack1, 4),
            (PlayerAnimationRules.Attack2, 4),
            (PlayerAnimationRules.Attack3, 4),
            (PlayerAnimationRules.Hurt, 2),
            (PlayerAnimationRules.Ghost, 4)
        };


        [MenuItem("Grace Run/Set Up Player Animations")]
        public static void Apply()
        {
            ConfigureAsepriteImporter();
            ConfigurePngTextures();
            var animData = EnsureAnimationData();
            WireGameConfigHats();
            WirePlayerPrefab(animData);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlayerAnimationSetup] Configured player.aseprite (16 PPU, Point, BottomCenter) and wired PlayerAnimator on Player.prefab.");
        }

        private static void ConfigureAsepriteImporter()
        {
            if (AssetImporter.GetAtPath(AsepritePath) is not AsepriteImporter importer) return;

            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.pivotSpace = PivotSpaces.Canvas;
            importer.pivotAlignment = SpriteAlignment.BottomCenter;
            importer.layerImportMode = LayerImportModes.MergeFrame;
            importer.generateModelPrefab = true;
            importer.generateAnimationClips = true;

            var so = new SerializedObject(importer);
            var meshType = so.FindProperty("m_TextureImporterSettings.spriteMeshType");
            if (meshType != null) meshType.intValue = (int)SpriteMeshType.FullRect;
            so.FindProperty("m_AnimatedSpriteImportData")?.ClearArray();
            so.FindProperty("m_SpriteSheetImportData")?.ClearArray();
            so.FindProperty("m_AsepriteLayers")?.ClearArray();
            so.ApplyModifiedPropertiesWithoutUndo();

            importer.SaveAndReimport();
        }

        private static void ConfigurePngTextures()
        {
            var folders = new[]
            {
                "Assets/_Project/Art/Characters",
                "Assets/_Project/Art/Bosses",
                "Assets/_Project/Art/Environment",
                "Assets/_Project/Art/UI"
            };

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", folders))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Placeholder/") || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (AssetImporter.GetAtPath(path) is not TextureImporter ti)
                    continue;

                var isCharacterOrBoss = path.Contains("/Characters/") || path.Contains("/Bosses/");
                var isSheet = path.EndsWith("_sheet.png", StringComparison.OrdinalIgnoreCase);

                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spritePixelsPerUnit = 16f;
                settings.filterMode = FilterMode.Point;
                settings.mipmapEnabled = false;
                settings.alphaIsTransparency = true;
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteMode = (int)(isSheet ? SpriteImportMode.Multiple : SpriteImportMode.Single);
                settings.spriteAlignment = (int)(isCharacterOrBoss ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
                settings.spritePivot = isCharacterOrBoss ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
                ti.SetTextureSettings(settings);
                ti.textureCompression = TextureImporterCompression.Uncompressed;

                if (isSheet) SliceCharacterSheet(ti);
                ti.SaveAndReimport();
            }
        }

        private static void SliceCharacterSheet(TextureImporter ti)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(ti);
            if (provider == null) return;

            provider.InitSpriteEditorDataProvider();
            var rects = new List<SpriteRect>();
            for (var row = 0; row < SheetRows.Length; row++)
            {
                var (tag, count) = SheetRows[row];
                var y = (SheetRows.Length - 1 - row) * 32;
                for (var col = 0; col < count; col++)
                {
                    rects.Add(new SpriteRect
                    {
                        name = $"{tag}_{col}",
                        rect = new Rect(col * 32, y, 32, 32),
                        alignment = SpriteAlignment.BottomCenter,
                        pivot = new Vector2(0.5f, 0f),
                        spriteID = GUID.Generate()
                    });
                }
            }

            provider.SetSpriteRects(rects.ToArray());
            provider.Apply();
        }

        private static PlayerAnimationData EnsureAnimationData()
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerAnimationData>(AnimDataPath);
            if (data != null) return data;

            data = ScriptableObject.CreateInstance<PlayerAnimationData>();
            AssetDatabase.CreateAsset(data, AnimDataPath);
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void WireGameConfigHats()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigPath);
            if (config == null) return;

            var hatPaths = new[]
            {
                "Assets/_Project/Art/Characters/Placeholder/hat_plume.png",
                "Assets/_Project/Art/Characters/Placeholder/hat_horns.png",
                "Assets/_Project/Art/Characters/Placeholder/hat_crown.png",
                "Assets/_Project/Art/Characters/Placeholder/hat_wizard.png"
            };

            var so = new SerializedObject(config);
            var prop = so.FindProperty("_hatSprites");
            if (prop == null) return;

            prop.arraySize = hatPaths.Length;
            for (var i = 0; i < hatPaths.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(hatPaths[i]);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void WirePlayerPrefab(PlayerAnimationData animData)
        {
            RuntimeAnimatorController controller = null;
            Sprite idleSprite = null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AsepritePath))
            {
                if (asset is RuntimeAnimatorController rac) controller = rac;
                else if (asset is Sprite s && (idleSprite == null || s.name == "Frame_0")) idleSprite = s;
            }

            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var visuals = root.transform.Find("Visuals");
                var bodyTransform = visuals != null ? visuals.Find("Body") : null;
                var hatTransform = visuals != null ? visuals.Find("Hat") : null;
                var eyeTransform = visuals != null ? visuals.Find("Eye") : null;
                if (bodyTransform == null) return;

                bodyTransform.localPosition = Vector3.zero;
                bodyTransform.localScale = Vector3.one;
                var bodyRenderer = bodyTransform.GetComponent<SpriteRenderer>();
                if (bodyRenderer != null && idleSprite != null)
                {
                    bodyRenderer.sprite = idleSprite;
                    bodyRenderer.drawMode = SpriteDrawMode.Simple;
                }

                if (hatTransform != null)
                {
                    hatTransform.localPosition = new Vector3(0f, 1.375f, 0f);
                    hatTransform.localScale = Vector3.one;
                }

                if (eyeTransform != null && eyeTransform.TryGetComponent(out SpriteRenderer eyeRenderer))
                    eyeRenderer.enabled = false;

                var animator = bodyTransform.GetComponent<Animator>();
                if (animator == null) animator = bodyTransform.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;

                var playerAnimator = root.GetComponent<PlayerAnimator>();
                if (playerAnimator == null) playerAnimator = root.AddComponent<PlayerAnimator>();

                var paSo = new SerializedObject(playerAnimator);
                SerializedFields.Set(paSo, "_data", animData);
                SerializedFields.Set(paSo, "_movementData", AssetDatabase.LoadAssetAtPath<PlayerMovementData>(MovementDataPath));
                SerializedFields.Set(paSo, "_animator", animator);
                SerializedFields.Set(paSo, "_bodyRenderer", bodyRenderer);
                SerializedFields.Set(paSo, "_hatRenderer", hatTransform != null ? hatTransform.GetComponent<SpriteRenderer>() : null);
                SerializedFields.Set(paSo, "_movement", root.GetComponent<PlayerMovement>());
                SerializedFields.Set(paSo, "_dodge", root.GetComponent<PlayerDodge>());
                SerializedFields.Set(paSo, "_attack", root.GetComponent<PlayerLightAttack>());
                SerializedFields.Set(paSo, "_health", root.GetComponent<Health>());
                SerializedFields.Set(paSo, "_ghost", root.GetComponent<Ghost>());
                SerializedFields.Set(paSo, "_groundCheck", root.transform.Find("GroundCheck"));
                paSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
