using System.IO;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Mirror Duelist Art (docs/ART_PIPELINE.md step 4): imports the pose PNGs in
    /// Art/Bosses/MirrorDuelist, fills a <see cref="BossPoseSet"/>, wires <see cref="BossPoseView"/> and
    /// <see cref="MirrorGlassView"/> into the MirrorDuelist prefab (the hand-mirror glass stays a separate,
    /// tintable sprite layer), hides the placeholder shapes, draws the shard / wave / reflection / echo-icon
    /// art, and refits the attack hitboxes to the drawn poses. Safe to run again.
    /// </summary>
    public static class MirrorDuelistArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Bosses/MirrorDuelist";
        private const string DataDir = "Assets/_Project/Data/Bosses/MirrorDuelist";
        private const string PrefabDir = "Assets/_Project/Prefabs/Bosses/";
        private const float FeetOffset = -3f; // the boss root is 3 units above the floor
        private const string Glass = "_glass";
        private static readonly string[] Poses =
        {
            "idle_0", "idle_1", "idle_2", "idle_3", "stagger", "death_0", "death_1", "death_2", "death_3",
            "lunge_windup", "lunge_strike", "lunge_recovery", "cut_windup", "cut_strike", "cut_recovery",
            "echo_windup", "echo_strike", "echo_recovery",
        };

        [MenuItem("Grace Run/Build Mirror Duelist Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var path in Directory.GetFiles(ArtDir, "*.png"))
            {
                var name = Path.GetFileName(path);
                Import(path.Replace('\\', '/'), name.StartsWith("echo_icon") ? 16 : 24);
            }
            var set = BuildPoseSet();
            var data = BuildData();
            WireBoss(set);
            WireReflection(set);
            WireShock("MirrorShard", "mirror_shard", new Vector2(1.5f, 0.8f));
            WireShock("MirrorWave", "mirror_wave", new Vector2(1.5f, 1.7f));
            Refit();
            AssetDatabase.SaveAssets();
            Debug.Log("Mirror Duelist art built (" + data.name + ").");
        }

        private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.png");

        private static Sprite Pose(string name) => S("duelist_" + name);

        private static Sprite GlassOf(string name) => S("duelist_" + name + Glass);

        private static BossPoseSet BuildPoseSet()
        {
            var path = $"{DataDir}/MirrorDuelistPoses.asset";
            var set = AssetDatabase.LoadAssetAtPath<BossPoseSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<BossPoseSet>();
                AssetDatabase.CreateAsset(set, path);
            }
            var so = new SerializedObject(set);
            SetSprites(so.FindProperty("_idle"), Pose("idle_0"), Pose("idle_1"), Pose("idle_2"), Pose("idle_3"));
            so.FindProperty("_stagger").objectReferenceValue = Pose("stagger");
            SetSprites(so.FindProperty("_death"), Pose("death_0"), Pose("death_1"), Pose("death_2"), Pose("death_3"));
            var list = so.FindProperty("_attacks");
            list.arraySize = 3;
            SetAttack(list.GetArrayElementAtIndex(0), "lunge", "rapier_lunge");
            SetAttack(list.GetArrayElementAtIndex(1), "cut", "mirror_cut", "mirror_cut_second", "mirror_cut_double");
            SetAttack(list.GetArrayElementAtIndex(2), "echo", "mirror_echo");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void SetAttack(SerializedProperty e, string pose, params string[] ids)
        {
            var list = e.FindPropertyRelative("AttackIds");
            list.arraySize = ids.Length;
            for (var i = 0; i < ids.Length; i++) list.GetArrayElementAtIndex(i).stringValue = ids[i];
            e.FindPropertyRelative("Windup").objectReferenceValue = Pose(pose + "_windup");
            e.FindPropertyRelative("Strike").objectReferenceValue = Pose(pose + "_strike");
            e.FindPropertyRelative("Recovery").objectReferenceValue = Pose(pose + "_recovery");
        }

        private static void SetSprites(SerializedProperty list, params Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static MirrorDuelistData BuildData()
        {
            var data = AssetDatabase.LoadAssetAtPath<MirrorDuelistData>($"{DataDir}/MirrorDuelistData.asset");
            var so = new SerializedObject(data);
            so.FindProperty("_slashIcon").objectReferenceValue = S("echo_icon_slash");
            so.FindProperty("_shardIcon").objectReferenceValue = S("echo_icon_shard");
            so.FindProperty("_burstIcon").objectReferenceValue = S("echo_icon_burst");
            so.FindProperty("_dashIcon").objectReferenceValue = S("echo_icon_dash");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            return data;
        }

        private static void WireBoss(BossPoseSet set)
        {
            var path = PrefabDir + "MirrorDuelist.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visuals = root.transform.Find("Visuals");
                foreach (var placeholder in new[] { "Rapier", "Eye" }) visuals.Find(placeholder).gameObject.SetActive(false);
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                var glass = visuals.Find("Mirror").GetComponent<SpriteRenderer>();
                Place(body, set.Idle[0], 0);
                Place(glass, GlassOf("idle_0"), 1);

                var box = root.GetComponent<BoxCollider2D>();
                box.size = new Vector2(2.2f, 5f);
                box.offset = new Vector2(0f, -0.5f);

                var view = root.GetComponent<BossPoseView>() ?? root.AddComponent<BossPoseView>();
                var vso = new SerializedObject(view);
                vso.FindProperty("_poses").objectReferenceValue = set;
                vso.FindProperty("_runner").objectReferenceValue = root.GetComponent<BossAttackRunner>();
                vso.FindProperty("_brain").objectReferenceValue = root.GetComponent<BossBrain>();
                vso.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                vso.FindProperty("_body").objectReferenceValue = body;
                vso.ApplyModifiedProperties();

                var glassView = root.GetComponent<MirrorGlassView>() ?? root.AddComponent<MirrorGlassView>();
                var gso = new SerializedObject(glassView);
                gso.FindProperty("_body").objectReferenceValue = body;
                gso.FindProperty("_glass").objectReferenceValue = glass;
                var pairs = gso.FindProperty("_pairs");
                pairs.arraySize = Poses.Length;
                for (var i = 0; i < Poses.Length; i++)
                {
                    var p = pairs.GetArrayElementAtIndex(i);
                    p.FindPropertyRelative("Body").objectReferenceValue = Pose(Poses[i]);
                    p.FindPropertyRelative("Glass").objectReferenceValue = GlassOf(Poses[i]);
                }
                gso.ApplyModifiedProperties();

                var icon = visuals.Find("EchoIcon");
                if (icon == null)
                {
                    icon = new GameObject("EchoIcon").transform;
                    icon.SetParent(visuals, false);
                    icon.gameObject.AddComponent<SpriteRenderer>();
                }
                var iconRenderer = icon.GetComponent<SpriteRenderer>();
                iconRenderer.sortingLayerID = body.sortingLayerID;
                iconRenderer.sortingOrder = body.sortingOrder + 2;
                icon.localPosition = new Vector3(0f, 3.7f, 0f);
                icon.localScale = Vector3.one * 1.3f;

                var mso = new SerializedObject(root.GetComponent<MirrorMemory>());
                mso.FindProperty("_mirror").objectReferenceValue = glass;
                mso.FindProperty("_icon").objectReferenceValue = iconRenderer;
                mso.ApplyModifiedProperties();

                var rso = new SerializedObject(root.GetComponent<MirrorReflections>());
                rso.FindProperty("_poses").objectReferenceValue = set;
                rso.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>The reflection template: an empty root with the drawn art on an "Art" child and the Hit child kept.</summary>
        private static void WireReflection(BossPoseSet set)
        {
            var path = PrefabDir + "MirrorReflection.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var rootRenderer = root.GetComponent<SpriteRenderer>();
                var sortingOrder = rootRenderer != null ? rootRenderer.sortingOrder : 5;
                if (rootRenderer != null) Object.DestroyImmediate(rootRenderer);
                root.transform.localScale = Vector3.one;
                var art = root.transform.Find("Art");
                if (art == null)
                {
                    art = new GameObject("Art").transform;
                    art.SetParent(root.transform, false);
                    art.gameObject.AddComponent<SpriteRenderer>();
                }
                var r = art.GetComponent<SpriteRenderer>();
                r.sprite = set.Idle[0];
                r.sortingOrder = sortingOrder;
                r.flipX = true;
                r.color = new Color(0.9f, 0.88f, 1f, 0.5f);
                art.localPosition = new Vector3(0f, FeetOffset, 0f);
                art.localScale = Vector3.one;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>Swaps the placeholder sprite of a shard / wave prefab for the drawn one.</summary>
        private static void WireShock(string prefab, string sprite, Vector2 hitSize)
        {
            var path = PrefabDir + prefab + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    r.sprite = S(sprite);
                    r.color = new Color(1f, 1f, 1f, r.color.a);
                    r.transform.localScale = Vector3.one;
                    r.transform.localPosition = Vector3.zero;
                }
                root.GetComponentInChildren<BoxCollider2D>().size = hitSize;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>Hit boxes and weak points follow the drawn poses (the art faces left; the Visuals root mirrors it).</summary>
        private static void Refit()
        {
            SetBoxes("RapierLunge", new Vector2(2.9f, 0f), new Vector2(4f, 2.2f), new Vector2(2.1f, 1.4f));
            foreach (var name in new[] { "MirrorCut", "MirrorCutSecond", "MirrorCutDouble" })
                SetBoxes(name, new Vector2(1.5f, -0.9f), new Vector2(3.2f, 2.6f), new Vector2(-0.8f, 1.9f));
            SetBoxes("MirrorEcho", new Vector2(2f, 0.6f), new Vector2(3.2f, 2.8f), new Vector2(-0.4f, 2.1f));
        }

        private static void SetBoxes(string asset, Vector2 offset, Vector2 size, Vector2 weakPoint)
        {
            var attack = AssetDatabase.LoadAssetAtPath<AttackData>($"{DataDir}/{asset}.asset");
            var so = new SerializedObject(attack);
            so.FindProperty("_hitboxOffset").vector2Value = offset;
            so.FindProperty("_hitboxSize").vector2Value = size;
            so.FindProperty("_weakPointOffset").vector2Value = weakPoint;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(attack);
        }

        /// <summary>The art faces left and the Visuals root is written for a boss facing right, so the sprites are flipped.</summary>
        private static void Place(SpriteRenderer renderer, Sprite sprite, int orderAbove)
        {
            renderer.sprite = sprite;
            renderer.flipX = true;
            renderer.color = Color.white; // the placeholder accent tint would multiply the art's colours down
            renderer.sortingOrder = orderAbove;
            renderer.transform.localPosition = new Vector3(0f, FeetOffset, 0f);
            renderer.transform.localScale = Vector3.one;
        }

        private static void Import(string path, int pixelsPerUnit)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            var centre = path.Contains("mirror_") || path.Contains("echo_icon");
            settings.spriteAlignment = (int)(centre ? SpriteAlignment.Center : SpriteAlignment.BottomCenter);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
