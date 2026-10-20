using System.IO;
using System.Linq;
using GraceRun.Combat;
using GraceRun.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Player Animations (docs/ART_PIPELINE.md step 3): imports the cleaned
    /// 64×64 frames in Art/Characters/Player/&lt;clip&gt;/, builds one clip per folder (body sprite
    /// plus a hat offset per frame, so the hat follows the head), the Animator controller, and
    /// wires <see cref="PlayerAnimSync"/> and <see cref="PlayerAnimator"/> into the Player prefab.
    /// Safe to run again after re-exporting frames or changing PlayerAnimData frame rates.
    /// </summary>
    public static class PlayerAnimationBuilder
    {
        private const string FramesRoot = "Assets/_Project/Art/Characters/Player";
        private const string AnchorPath = "Assets/_Project/Art/Characters/player_body.png";
        private const string ControllerPath = FramesRoot + "/PlayerAnimator.controller";
        private const string DataPath = "Assets/_Project/Data/Player/PlayerAnimData.asset";
        private const string StatsPath = "Assets/_Project/Data/Player/PlayerStatsData.asset";
        private const string PlayerPath = "Assets/_Project/Prefabs/Player.prefab";
        private const int PixelsPerUnit = 24;
        private const int Canvas = 64;
        private const int HeadRun = 5;

        private static readonly (string folder, string state, bool loop)[] Clips =
        {
            ("idle", "Idle", true), ("run", "Run", true), ("jump", "Jump", false), ("fall", "Fall", true),
            ("dodge", "Dodge", false), ("attack1", "Attack1", false), ("attack2", "Attack2", false),
            ("attack3", "Attack3", false), ("hurt", "Hurt", false), ("ghost", "Ghost", true),
        };

        [MenuItem("Grace Run/Build Player Animations")]
        public static void Build()
        {
            var data = LoadOrCreateData();
            var anchorHead = HeadOf(AnchorPath, (Canvas - 48) / 2);
            // Reuse the controller on a rebuild: replacing the asset would break the prefab's reference.
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath)
                ?? AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var layer = controller.layers[0].stateMachine;
            foreach (var old in layer.states) layer.RemoveState(old.state);

            foreach (var (folder, state, loop) in Clips)
            {
                var dir = $"{FramesRoot}/{folder}";
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    Debug.LogWarning($"Player animations: no frames in {dir}, the {state} state stays empty.");
                    layer.AddState(state);
                    continue;
                }
                var clip = BuildClip(dir, folder, FpsOf(data, folder), loop, anchorHead);
                var s = layer.AddState(state);
                s.motion = clip;
                s.writeDefaultValues = false;
                if (state == "Idle") layer.defaultState = s;
            }

            AssetDatabase.SaveAssets();
            WirePrefab(controller, data);
            Debug.Log("Player animations built.");
        }

        private static AnimationClip BuildClip(string dir, string name, float fps, bool loop, Vector2 anchorHead)
        {
            var paths = Directory.GetFiles(dir, "*.png").Select(p => p.Replace('\\', '/')).OrderBy(p => p).ToArray();
            foreach (var path in paths) Import(path);

            var clip = new AnimationClip { frameRate = fps };
            var sprites = new ObjectReferenceKeyframe[paths.Length + 1];
            var hatX = new AnimationCurve();
            var hatY = new AnimationCurve();
            for (var i = 0; i < paths.Length; i++)
            {
                var t = i / fps;
                sprites[i] = new ObjectReferenceKeyframe { time = t, value = AssetDatabase.LoadAssetAtPath<Sprite>(paths[i]) };
                var offset = (HeadOf(paths[i], 0) - anchorHead) / PixelsPerUnit;
                hatX.AddKey(new Keyframe(t, offset.x, float.PositiveInfinity, float.PositiveInfinity));
                hatY.AddKey(new Keyframe(t, offset.y, float.PositiveInfinity, float.PositiveInfinity));
            }
            // Repeat the last frame so it gets its full length before the loop wraps.
            sprites[paths.Length] = new ObjectReferenceKeyframe { time = paths.Length / fps, value = sprites[paths.Length - 1].value };

            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("Body", typeof(SpriteRenderer), "m_Sprite"), sprites);
            clip.SetCurve("Hat", typeof(Transform), "m_LocalPosition.x", hatX);
            clip.SetCurve("Hat", typeof(Transform), "m_LocalPosition.y", hatY);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var clipPath = $"{dir}/{name}.anim";
            AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
            return clip;
        }

        private static void Import(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Centre and top of the head, in pixels from the bottom-left, shifted right by padX: the
        /// highest row with a run of body blue at least <see cref="HeadRun"/> wide, so raised
        /// gloves, smear lines and shake marks don't count as the head.
        /// </summary>
        private static Vector2 HeadOf(string path, int padX)
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(path));
            var px = tex.GetPixels32();
            int w = tex.width, h = tex.height;
            Object.DestroyImmediate(tex);
            for (var y = h - 1; y >= 0; y--)
            {
                int run = 0, best = 0, bestEnd = 0;
                for (var x = 0; x < w; x++)
                {
                    run = IsBodyBlue(px[y * w + x]) ? run + 1 : 0;
                    if (run > best) { best = run; bestEnd = x; }
                }
                if (best >= HeadRun) return new Vector2(bestEnd - (best - 1) / 2f + padX, y);
            }
            return new Vector2(w / 2f, h);
        }

        private static bool IsBodyBlue(Color32 c) => c.a > 0 && c.b > 150 && c.b > c.r + 20;

        private static float FpsOf(PlayerAnimData data, string folder) => folder switch
        {
            "idle" => data.IdleFps,
            "run" => data.RunFps,
            "jump" or "fall" => data.AirFps,
            "dodge" => data.DodgeFps,
            "hurt" => data.HurtFps,
            "ghost" => data.GhostFps,
            _ => data.AttackFps,
        };

        private static PlayerAnimData LoadOrCreateData()
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerAnimData>(DataPath);
            if (data != null) return data;
            data = ScriptableObject.CreateInstance<PlayerAnimData>();
            AssetDatabase.CreateAsset(data, DataPath);
            return data;
        }

        private static void WirePrefab(RuntimeAnimatorController controller, PlayerAnimData data)
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPath);
            try
            {
                var visuals = root.transform.Find("Visuals");
                var rig = visuals.Find("Rig");
                if (rig == null)
                {
                    rig = new GameObject("Rig").transform;
                    rig.SetParent(visuals, false);
                    foreach (var child in new[] { "Body", "Eye", "Hat" })
                    {
                        var part = visuals.Find(child);
                        if (part != null) part.SetParent(rig, false);
                    }
                }
                var animator = GetOrAdd<Animator>(rig.gameObject);
                animator.runtimeAnimatorController = controller;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                var movement = root.GetComponent<PlayerMovement>();
                var sync = GetOrAdd<PlayerAnimSync>(root);
                var syncSo = new SerializedObject(sync);
                SerializedFields.Set(syncSo, "_data", data);
                SerializedFields.Set(syncSo, "_movement", movement);
                syncSo.ApplyModifiedPropertiesWithoutUndo();

                var view = GetOrAdd<PlayerAnimator>(root);
                var viewSo = new SerializedObject(view);
                SerializedFields.Set(viewSo, "_data", data);
                SerializedFields.Set(viewSo, "_stats", AssetDatabase.LoadAssetAtPath<PlayerStatsData>(StatsPath));
                SerializedFields.Set(viewSo, "_animator", animator);
                SerializedFields.Set(viewSo, "_rig", rig);
                SerializedFields.Set(viewSo, "_sync", sync);
                SerializedFields.Set(viewSo, "_dodge", root.GetComponent<PlayerDodge>());
                SerializedFields.Set(viewSo, "_attack", root.GetComponent<PlayerLightAttack>());
                SerializedFields.Set(viewSo, "_health", root.GetComponent<Health>());
                SerializedFields.Set(viewSo, "_ghost", root.GetComponent<Ghost>());
                viewSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, PlayerPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
