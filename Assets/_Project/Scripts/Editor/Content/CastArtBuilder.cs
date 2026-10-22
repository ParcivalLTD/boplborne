using GraceRun.Abilities;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Cast Art: imports Art/Characters/Casts/&lt;ability&gt;_&lt;phase&gt;.png (64×64 body poses,
    /// 24 PPU, point filter, bottom-centre pivot) and fills the matching <see cref="CastPoseSet"/> in
    /// Data/Abilities/CastPoses with the three sprites and the hat position of each pose (head
    /// position against the anchor, like the weapon poses). Safe to run again.
    /// </summary>
    public static class CastArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Characters/Casts";
        private const string DataDir = "Assets/_Project/Data/Abilities/CastPoses";

        private static readonly (string file, string asset)[] Casts =
        {
            ("arrow_volley", "ArrowVolley"), ("bomb", "Bomb"), ("gust", "Gust"),
            ("black_hole", "BlackHole"), ("stone_wall", "StoneWall"), ("grapple_hook", "GrappleHook"),
        };

        [MenuItem("Grace Run/Build Cast Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            var anchorHead = PlayerAnimationBuilder.AnchorHead();
            foreach (var (file, asset) in Casts)
            {
                var set = AssetDatabase.LoadAssetAtPath<CastPoseSet>($"{DataDir}/{asset}Cast.asset");
                if (set == null) { Debug.LogWarning($"Cast art: no {asset}Cast.asset"); continue; }
                var poses = new SerializedObject(set).FindProperty("_poses");
                var so = poses.serializedObject;
                foreach (var phase in new[] { "Windup", "Strike", "Recovery" })
                {
                    var png = $"{ArtDir}/{file}_{phase.ToLowerInvariant()}.png";
                    if (!System.IO.File.Exists(png)) { Debug.LogWarning($"Cast art: missing {png}"); continue; }
                    ImportPose(png);
                    poses.FindPropertyRelative(phase).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(png);
                    poses.FindPropertyRelative("Hat" + phase).vector2Value = (PlayerAnimationBuilder.HeadOf(png, 0) - anchorHead) / 24f;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(set);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Cast art built.");
        }

        private static void ImportPose(string path)
        {
            AssetDatabase.ImportAsset(path);
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
