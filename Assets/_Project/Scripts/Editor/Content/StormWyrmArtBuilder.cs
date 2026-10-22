using System.IO;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Storm Wyrm Art (docs/ART_PIPELINE.md step 4): imports the cleaned PNGs in
    /// Art/Bosses/StormWyrm, fills <c>StormWyrmPoses.asset</c>, wires <see cref="BossPoseView"/> into the
    /// StormWyrm prefab (the placeholder shapes are disabled, not deleted), sizes the collider to the drawn
    /// body, refits the attack hitboxes and weak points to the poses and gives the lightning column its
    /// drawn bolt. Safe to run again. NOTE: re-running <see cref="StormWyrmBuilder"/> rebuilds the prefab
    /// shapes and drops this wiring; run this builder again afterwards.
    /// </summary>
    public static class StormWyrmArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Bosses/StormWyrm";
        private const string DataDir = "Assets/_Project/Data/Bosses/StormWyrm";
        private const string PrefabPath = "Assets/_Project/Prefabs/Bosses/StormWyrm.prefab";
        private const string ColumnPrefab = "Assets/_Project/Prefabs/Bosses/LightningColumn.prefab";

        // The art is 168x112 px (7 x 4.67 units), facing left, pivot bottom centre, 9 transparent rows under the feet.
        // The root collider is centred on the root, so the root sits half its height above the floor.
        private static readonly Vector2 BodySize = new Vector2(6.6f, 4.2f);
        private const float FeetOffset = -2.1f - 0.375f;

        /// <summary>A hitbox and weak point fitted to the drawn poses (units from the boss root, +x forward, floor at y = -2.1).</summary>
        private struct Fit
        {
            public string File;
            public bool HasHitbox;
            public Vector2 HitOffset;
            public Vector2 HitSize;
            public Vector2 WeakPoint;
            public float MaxRange;
        }

        private static Fit Melee(string file, Vector2 offset, Vector2 size, Vector2 weak, float range) => new Fit
        {
            File = file, HasHitbox = true, HitOffset = offset, HitSize = size, WeakPoint = weak, MaxRange = range,
        };

        private static Fit Spell(string file, Vector2 weak) => new Fit { File = file, WeakPoint = weak };

        private static Fit[] Fits() => new[]
        {
            // Tail lash: the tail runs along the floor from 3.2 units ahead to 2.5 behind the centre.
            Melee("TailSweep", new Vector2(0.3f, -1.35f), new Vector2(7f, 1.5f), new Vector2(1.6f, 1f), 5f),
            Melee("TailSweepSecond", new Vector2(0.3f, -1.35f), new Vector2(7f, 1.5f), new Vector2(1.6f, 1f), 5.5f),
            Melee("TailSweepDouble", new Vector2(0.3f, -1.35f), new Vector2(7f, 1.5f), new Vector2(1.6f, 1f), 5f),
            // Lightning: head raised and crackling, no melee hitbox.
            Spell("LightningColumns", new Vector2(1.4f, 1.1f)),
            Spell("LightningFlanks", new Vector2(1.4f, 1.1f)),
            // Lunge: the head and neck reach 3.2 units ahead, from the floor up to head height.
            Melee("ThunderLunge", new Vector2(1.9f, -0.2f), new Vector2(3.8f, 3.6f), new Vector2(2f, 1f), 14f),
            // Slam: the head smashes onto the floor 1.4 to 3.2 units ahead.
            Melee("StormSlam", new Vector2(2.4f, -1.4f), new Vector2(3.6f, 1.4f), new Vector2(2.2f, -1.2f), 7f),
        };

        [MenuItem("Grace Run/Build Storm Wyrm Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var path in Directory.GetFiles(ArtDir, "*.png")) Import(path.Replace('\\', '/'));
            var poses = BuildPoseSet();
            WirePrefab(poses);
            FitAttacks();
            WireColumn();
            AssetDatabase.SaveAssets();
            Debug.Log("Storm Wyrm art built.");
        }

        private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/stormwyrm_{name}.png");

        private static BossPoseSet BuildPoseSet()
        {
            var path = $"{DataDir}/StormWyrmPoses.asset";
            var set = AssetDatabase.LoadAssetAtPath<BossPoseSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<BossPoseSet>();
                AssetDatabase.CreateAsset(set, path);
            }
            var so = new SerializedObject(set);
            SetSprites(so.FindProperty("_idle"), S("idle_0"), S("idle_1"), S("idle_2"), S("idle_3"));
            so.FindProperty("_stagger").objectReferenceValue = S("stagger");
            SetSprites(so.FindProperty("_death"), S("death_0"), S("death_1"), S("death_2"), S("death_3"));
            var list = so.FindProperty("_attacks");
            list.arraySize = 4;
            SetAttack(list.GetArrayElementAtIndex(0), "sweep", "tail_sweep", "tail_sweep_double", "tail_sweep_second");
            SetAttack(list.GetArrayElementAtIndex(1), "lightning", "lightning_columns", "lightning_flanks");
            SetAttack(list.GetArrayElementAtIndex(2), "lunge", "thunder_lunge");
            SetAttack(list.GetArrayElementAtIndex(3), "slam", "storm_slam");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void SetAttack(SerializedProperty e, string pose, params string[] ids)
        {
            var list = e.FindPropertyRelative("AttackIds");
            list.arraySize = ids.Length;
            for (var i = 0; i < ids.Length; i++) list.GetArrayElementAtIndex(i).stringValue = ids[i];
            e.FindPropertyRelative("Windup").objectReferenceValue = S(pose + "_windup");
            e.FindPropertyRelative("Strike").objectReferenceValue = S(pose + "_strike");
            e.FindPropertyRelative("Recovery").objectReferenceValue = S(pose + "_recovery");
        }

        private static void SetSprites(SerializedProperty list, params Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void WirePrefab(BossPoseSet poses)
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var box = root.GetComponent<BoxCollider2D>();
                box.size = BodySize;
                box.offset = Vector2.zero;

                var visuals = root.transform.Find("Visuals");
                foreach (var placeholder in new[] { "Tail", "Crest", "Head", "Eye" })
                {
                    var child = visuals.Find(placeholder);
                    if (child != null) child.gameObject.SetActive(false);
                }
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                body.sprite = poses.Idle[0];
                body.flipX = true; // the art faces left, the Visuals root is written for a boss facing right
                body.color = Color.white; // the placeholder accent tint would multiply the art's colours down
                body.transform.localPosition = new Vector3(0f, FeetOffset, 0f);
                body.transform.localScale = Vector3.one;

                var view = root.GetComponent<BossPoseView>() ?? root.AddComponent<BossPoseView>();
                var so = new SerializedObject(view);
                so.FindProperty("_poses").objectReferenceValue = poses;
                so.FindProperty("_runner").objectReferenceValue = root.GetComponent<BossAttackRunner>();
                so.FindProperty("_brain").objectReferenceValue = root.GetComponent<BossBrain>();
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = body;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void FitAttacks()
        {
            foreach (var fit in Fits())
            {
                var attack = AssetDatabase.LoadAssetAtPath<AttackData>($"{DataDir}/{fit.File}.asset");
                if (attack == null) continue;
                var so = new SerializedObject(attack);
                if (fit.HasHitbox)
                {
                    so.FindProperty("_hitboxOffset").vector2Value = fit.HitOffset;
                    so.FindProperty("_hitboxSize").vector2Value = fit.HitSize;
                    so.FindProperty("_maxRange").floatValue = fit.MaxRange;
                }
                so.FindProperty("_weakPointOffset").vector2Value = fit.WeakPoint;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(attack);
            }
        }

        private static void WireColumn()
        {
            var root = PrefabUtility.LoadPrefabContents(ColumnPrefab);
            try
            {
                var column = root.GetComponent<LightningColumn>();
                var so = new SerializedObject(column);
                so.FindProperty("_telegraphSprite").objectReferenceValue = S("bolt_telegraph");
                so.FindProperty("_strikeSprite").objectReferenceValue = S("bolt_strike");
                so.ApplyModifiedPropertiesWithoutUndo();
                var beam = (SpriteRenderer)so.FindProperty("_beam").objectReferenceValue;
                beam.sprite = S("bolt_strike");
                beam.drawMode = SpriteDrawMode.Tiled;
                beam.tileMode = SpriteTileMode.Continuous;
                PrefabUtility.SaveAsPrefabAsset(root, ColumnPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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
            var bolt = Path.GetFileName(path).Contains("_bolt_");
            settings.spriteAlignment = (int)(bolt ? SpriteAlignment.Center : SpriteAlignment.BottomCenter);
            if (bolt)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect; // tiled sprites need a full rect mesh
                importer.wrapMode = TextureWrapMode.Repeat;
            }
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
