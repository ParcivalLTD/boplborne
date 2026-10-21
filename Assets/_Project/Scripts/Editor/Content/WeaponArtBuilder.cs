using GraceRun.Player;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Weapon Art: imports the weapon PNGs in Art/Weapons (24 PPU, point filter,
    /// the pivot on the grip), gives each weapon asset its sprite, smear, swing look and (Art/Characters/Weapons)
    /// body poses, and wires
    /// <see cref="PlayerAnimSync"/> into the Player prefab's <see cref="WeaponView"/>. Safe to run
    /// again; it only touches the art fields of the weapon assets, never the attack numbers.
    /// </summary>
    public static class WeaponArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Weapons";
        private const string PoseDir = "Assets/_Project/Art/Characters/Weapons";
        private const string DataDir = "Assets/_Project/Data/Weapons";

        private static readonly (string field, string file)[] PoseFields =
        {
            ("_lightA", "lightA"), ("_lightB", "lightB"), ("_heavy", "heavy"), ("_aerial", "aerial"), ("_up", "up"), ("_down", "down"),
        };
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        private readonly struct Look
        {
            public readonly string Sprite;
            public readonly Vector2 Pivot;
            public readonly WeaponSwingStyle Style;
            public readonly float Wind, Sweep, Reach, SmearScale;

            public Look(string sprite, Vector2 pivot, WeaponSwingStyle style, float wind, float sweep, float reach, float smearScale)
            {
                Sprite = sprite; Pivot = pivot; Style = style; Wind = wind; Sweep = sweep; Reach = reach; SmearScale = smearScale;
            }
        }

        [MenuItem("Grace Run/Build Weapon Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            Import($"{ArtDir}/weapon_slash.png", new Vector2(0.5f, 0.5f));
            var slash = Load("slash");
            // Fists draw nothing of their own: the body punch clips are the animation (a glove sprite floated next to the body).
            Apply("Fists", new Look(null, new Vector2(0.2f, 0.5f), WeaponSwingStyle.Thrust, 0f, 0f, 0.7f, 0.55f), null);
            Apply("Sword", new Look("sword", new Vector2(0.18f, 0.5f), WeaponSwingStyle.Arc, 65f, 115f, 0.5f, 1.35f), slash);
            Apply("Spear", new Look("spear", new Vector2(0.3f, 0.5f), WeaponSwingStyle.Thrust, 0f, 0f, 1f, 0.9f), slash);
            Apply("Hammer", new Look("hammer", new Vector2(0.12f, 0.5f), WeaponSwingStyle.Arc, 105f, 155f, 0.5f, 1.8f), slash);
            WirePlayer();
            AssetDatabase.SaveAssets();
            Debug.Log("Weapon art built.");
        }

        private static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/weapon_{name}.png");

        private static void Apply(string weapon, Look look, Sprite smear)
        {
            if (look.Sprite != null) Import($"{ArtDir}/weapon_{look.Sprite}.png", look.Pivot);
            var asset = AssetDatabase.LoadAssetAtPath<WeaponData>($"{DataDir}/{weapon}.asset");
            var so = new SerializedObject(asset);
            so.FindProperty("_sprite").objectReferenceValue = look.Sprite != null ? Load(look.Sprite) : null;
            so.FindProperty("_smear").objectReferenceValue = smear;
            so.FindProperty("_swingStyle").enumValueIndex = (int)look.Style;
            so.FindProperty("_windAngle").floatValue = look.Wind;
            so.FindProperty("_sweepAngle").floatValue = look.Sweep;
            so.FindProperty("_swingReach").floatValue = look.Reach;
            so.FindProperty("_smearScale").floatValue = look.SmearScale;
            so.FindProperty("_bodyPoses").objectReferenceValue = BuildBodyPoses(weapon);
            so.FindProperty("_animations").objectReferenceValue = BuildAnimations(weapon);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        /// <summary>Imports Art/Characters/Weapons/&lt;weapon&gt;/&lt;move&gt;_&lt;phase&gt;.png and fills the weapon's pose asset; null when the weapon has no such folder (Fists).</summary>
        private static WeaponBodyPoses BuildBodyPoses(string weapon)
        {
            var dir = $"{PoseDir}/{weapon}";
            if (!AssetDatabase.IsValidFolder(dir)) return null;
            var path = $"{DataDir}/{weapon}BodyPoses.asset";
            var asset = AssetDatabase.LoadAssetAtPath<WeaponBodyPoses>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WeaponBodyPoses>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            var anchorHead = PlayerAnimationBuilder.AnchorHead();
            foreach (var (field, file) in PoseFields)
            {
                var moves = so.FindProperty(field);
                foreach (var phase in new[] { "Windup", "Strike", "Recovery" })
                {
                    var png = $"{dir}/{file}_{phase.ToLowerInvariant()}.png";
                    if (!System.IO.File.Exists(png)) { Debug.LogWarning($"Weapon art: missing {png}"); continue; }
                    ImportPose(png);
                    moves.FindPropertyRelative(phase).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(png);
                    moves.FindPropertyRelative("Hat" + phase).vector2Value = (PlayerAnimationBuilder.HeadOf(png, 0) - anchorHead) / 24f;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static readonly (string folder, bool loop)[] LocomotionClips =
        {
            ("idle", true), ("run", true), ("jump", false), ("fall", true),
        };

        /// <summary>
        /// Builds idle / run / jump / fall with the weapon in hand from Art/Characters/Weapons/&lt;weapon&gt;/Loco/&lt;clip&gt;/*.png
        /// and an override of the player's Animator for them; null when the weapon has no such folder (Fists).
        /// </summary>
        private static AnimatorOverrideController BuildAnimations(string weapon)
        {
            var root = $"{PoseDir}/{weapon}/Loco";
            if (!AssetDatabase.IsValidFolder(root)) return null;
            var baseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/Player/PlayerAnimator.controller");
            var data = AssetDatabase.LoadAssetAtPath<PlayerAnimData>("Assets/_Project/Data/Player/PlayerAnimData.asset");
            var anchorHead = PlayerAnimationBuilder.AnchorHead();

            var path = $"{DataDir}/{weapon}Animations.overrideController";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (controller == null)
            {
                controller = new AnimatorOverrideController(baseController);
                AssetDatabase.CreateAsset(controller, path);
            }
            controller.runtimeAnimatorController = baseController;

            var built = new System.Collections.Generic.Dictionary<string, AnimationClip>();
            foreach (var (folder, loop) in LocomotionClips)
            {
                var dir = $"{root}/{folder}";
                if (!AssetDatabase.IsValidFolder(dir)) continue;
                built[folder] = PlayerAnimationBuilder.BuildClip(dir, folder, PlayerAnimationBuilder.FpsOf(data, folder), loop, anchorHead);
            }

            var pairs = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            controller.GetOverrides(pairs);
            for (var i = 0; i < pairs.Count; i++)
            {
                if (built.TryGetValue(pairs[i].Key.name, out var clip))
                    pairs[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(pairs[i].Key, clip);
            }
            controller.ApplyOverrides(pairs);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void ImportPose(string path)
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

        private static void WirePlayer()
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var view = root.GetComponentInChildren<WeaponView>(true);
                var so = new SerializedObject(view);
                so.FindProperty("_sync").objectReferenceValue = root.GetComponentInChildren<PlayerAnimSync>(true);
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Import(string path, Vector2 pivot)
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
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            importer.SetTextureSettings(settings);
            importer.spritePivot = pivot;
            importer.SaveAndReimport();
        }
    }
}
