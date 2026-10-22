using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Builds Floor 4, The Storm Wyrm (placeholder shapes in the arena accent): lightning columns, tail sweep,
    /// thunder lunge, and in phase 2 a storm slam that shatters half the ledges. Safe to run again; it rewrites the tuning.
    /// </summary>
    public static class StormWyrmBuilder
    {
        private const string Dir = "Assets/_Project/Data/Bosses/StormWyrm";
        private const string ColumnPrefab = "Assets/_Project/Prefabs/Bosses/LightningColumn.prefab";
        private static readonly Color Accent = new(0.373f, 0.722f, 0.69f);

        public static BossData Build()
        {
            var column = BuildColumnPrefab();
            var behaviours = BuildBehaviours(column);
            var data = BossBuildKit.Asset<BossData>($"{Dir}/StormWyrmData.asset");
            var prefabPath = BossBuildKit.CopyBossPrefab("TestBoss", "StormWyrm");
            var arena = BossBuildKit.CopyArena("TestBossArena", "StormWyrmArena");
            ArenaLayoutSetup.LayOut("StormWyrmArena");

            var sweep = Attack("TailSweep", "tail_sweep", new AttackSpec
            {
                Windup = 0.85f, Active = 0.2f, Recovery = 1f, Damage = 30, Knockback = new(9f, 7f),
                HitOffset = new(0f, -1f), HitSize = new(10f, 1.6f), WeakPoint = new(0.5f, 2.2f),
                Weight = 1.2f, MaxRange = 7.5f, WindupCue = "Windup_SweepingCutCue", StrikeCue = "Strike_st_gk_sweepCue",
            });
            var sweepSecond = Attack("TailSweepSecond", "tail_sweep_second", new AttackSpec
            {
                Windup = 0.5f, Active = 0.2f, Recovery = 1f, Damage = 30, Knockback = new(9f, 7f),
                HitOffset = new(0f, -1f), HitSize = new(10f, 1.6f), WeakPoint = new(0.5f, 2.2f),
                WindupCue = "Windup_SweepingCutCue", StrikeCue = "Strike_st_gk_sweepCue",
            });
            var sweepDouble = Attack("TailSweepDouble", "tail_sweep_double", new AttackSpec
            {
                Windup = 0.8f, Active = 0.2f, Recovery = 0.25f, Punish = 0.2f, Damage = 30, Knockback = new(9f, 7f),
                HitOffset = new(0f, -1f), HitSize = new(10f, 1.6f), FollowUp = sweepSecond,
                Weight = 1.1f, MaxRange = 7.5f, WindupCue = "Windup_SweepingCutCue", StrikeCue = "Strike_st_gk_sweepCue",
            });
            var columns = Attack("LightningColumns", "lightning_columns", Lightning(behaviours.Columns, 1.5f));
            var flanks = Attack("LightningFlanks", "lightning_flanks", Lightning(behaviours.Flanks, 1.3f));
            var lunge = Attack("ThunderLunge", "thunder_lunge", new AttackSpec
            {
                Windup = 0.7f, Active = 0.3f, Recovery = 1.1f, Punish = 0.9f, Damage = 36, Knockback = new(11f, 6f),
                HitOffset = new(2.6f, -0.3f), HitSize = new(3.6f, 3f), WeakPoint = new(-1f, 2.2f), Lunge = 6.5f, LungeTime = 0.22f,
                Weight = 1.1f, MinRange = 3.5f, MaxRange = 14f, WindupCue = "Windup_ShieldBashCue", StrikeCue = "Strike_st_gk_bashCue",
            });
            var slam = Attack("StormSlam", "storm_slam", new AttackSpec
            {
                Windup = 1.25f, Active = 0.25f, Recovery = 1.3f, Punish = 1f, Damage = 30, Knockback = new(8f, 8f),
                HitOffset = new(3.2f, -1f), HitSize = new(4.4f, 1.8f), WeakPoint = new(0.5f, 2.2f), Behaviour = behaviours.Break,
                Weight = 1f, Cooldown = 10f, MaxRange = 9f, WindupCue = "Windup_OverheadSlamCue", StrikeCue = "Strike_st_gk_slamCue",
            });

            var so = new SerializedObject(data);
            so.FindProperty("_id").stringValue = "storm_wyrm";
            so.FindProperty("_displayName").stringValue = "The Storm Wyrm";
            so.FindProperty("_floor").intValue = 4;
            so.FindProperty("_accentColor").colorValue = Accent;
            so.FindProperty("_baseHP").intValue = 720;
            so.FindProperty("_idleTime").floatValue = 0.55f;
            so.FindProperty("_targetSwitchInterval").floatValue = 7f;
            so.FindProperty("_walkSpeed").floatValue = 3.4f;
            BossBuildKit.SetAttacks(so.FindProperty("_phase1Attacks"), sweep, columns, lunge);
            BossBuildKit.SetAttacks(so.FindProperty("_phase2Attacks"), sweepDouble, columns, flanks, lunge, slam);
            so.FindProperty("_arenaPrefab").objectReferenceValue = arena;
            so.ApplyModifiedPropertiesWithoutUndo();

            BossPrefabShell.Build(prefabPath, data, new Vector2(7.5f, 3.4f), BuildShapes);
            so = new SerializedObject(data);
            so.FindProperty("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            return data;
        }

        private static AttackSpec Lightning(BossAttackBehaviour behaviour, float weight) => new()
        {
            Windup = 1.2f, Active = 0.3f, Recovery = 1.3f, Punish = 1f, Damage = 0, HitOffset = new(0f, -40f), HitSize = new(0.2f, 0.2f),
            WeakPoint = new(0f, 2.4f), Behaviour = behaviour, Weight = weight, Cooldown = 5f, MaxRange = 40f,
            WindupCue = "Windup_RingingTollCue", StrikeCue = "Strike_st_bt_tollCue",
        };

        private static AttackData Attack(string name, string id, AttackSpec spec) => BossBuildKit.Attack(Dir, name, id, spec);

        private static void BuildShapes(Transform visuals, Sprite square)
        {
            var body = BossBuildKit.Muted(Accent, 0.8f);
            const int layer = 11;
            BossBuildKit.Shape(visuals, "Body", new(0f, -0.2f), new(6.4f, 2.8f), body, square, 0, layer);
            BossBuildKit.Shape(visuals, "Tail", new(-4.2f, -0.6f), new(2.6f, 1.3f), BossBuildKit.Muted(Accent, 0.65f), square, -1, layer);
            BossBuildKit.Shape(visuals, "Crest", new(-0.5f, 1.5f), new(3.4f, 0.9f), BossBuildKit.Muted(Accent, 0.55f), square, 1, layer);
            BossBuildKit.Shape(visuals, "Head", new(3.4f, 0.7f), new(2.4f, 2.2f), BossBuildKit.Muted(Accent, 0.95f), square, 2, layer);
            BossBuildKit.Shape(visuals, "Eye", new(4f, 1.1f), new(0.7f, 0.3f), new(0.95f, 0.98f, 1f), square, 3, layer);
        }

        private static GameObject BuildColumnPrefab()
        {
            if (!System.IO.File.Exists(ColumnPrefab))
            {
                var go = new GameObject("LightningColumn");
                go.AddComponent<Unity.Netcode.NetworkObject>();
                go.AddComponent<LightningColumn>();
                var beam = new GameObject("Beam");
                beam.transform.SetParent(go.transform, false);
                var sr = beam.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Arenas/OneWayPlatform.prefab").GetComponent<SpriteRenderer>().sprite;
                sr.sortingOrder = 4;
                var hit = new GameObject("AttackHitbox") { layer = Core.Layers.BossAttackIndex };
                hit.transform.SetParent(go.transform, false);
                var shape = hit.AddComponent<BoxCollider2D>();
                shape.isTrigger = true;
                var hitbox = hit.AddComponent<Hitbox>();
                var hso = new SerializedObject(hitbox);
                hso.FindProperty("_attacker").objectReferenceValue = go.GetComponent<Unity.Netcode.NetworkObject>();
                hso.FindProperty("_team").enumValueIndex = 2;
                hso.FindProperty("_detection").enumValueIndex = 1;
                hso.ApplyModifiedPropertiesWithoutUndo();
                var cso = new SerializedObject(go.GetComponent<LightningColumn>());
                cso.FindProperty("_hitbox").objectReferenceValue = hitbox;
                cso.FindProperty("_shape").objectReferenceValue = shape;
                cso.FindProperty("_beam").objectReferenceValue = sr;
                cso.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(go, ColumnPrefab);
                Object.DestroyImmediate(go);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ColumnPrefab);
            BossBuildKit.SetObject(prefab.GetComponent<LightningColumn>(), "_data", BossBuildKit.Asset<LightningColumnData>($"{Dir}/LightningColumnData.asset"));
            return prefab;
        }

        private static (BossAttackBehaviour Columns, BossAttackBehaviour Flanks, BossAttackBehaviour Break) BuildBehaviours(GameObject column)
        {
            var columns = BossBuildKit.Asset<LightningColumnsBehaviour>($"{Dir}/LightningColumnsBehaviour.asset");
            BossBuildKit.SetObject(columns, "_columnPrefab", column.GetComponent<LightningColumn>());
            var flanks = BossBuildKit.Asset<LightningColumnsBehaviour>($"{Dir}/LightningFlanksBehaviour.asset");
            BossBuildKit.SetObject(flanks, "_columnPrefab", column.GetComponent<LightningColumn>());
            BossBuildKit.SetFloat(flanks, "_flankOffset", 4.6f);
            var shatter = BossBuildKit.Asset<PlatformBreakBehaviour>($"{Dir}/PlatformBreakBehaviour.asset");
            return (columns, flanks, shatter);
        }
    }
}
