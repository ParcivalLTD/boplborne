using GraceRun.Abilities;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Builds Floor 5, The Mirror Duelist (placeholder shapes in the arena accent): rapier lunge, quick cuts and an
    /// echo that throws back the kind of the last ability that hit it; in phase 2 two reflections copy its cuts.
    /// Safe to run again; it rewrites the tuning.
    /// </summary>
    public static class MirrorDuelistBuilder
    {
        private const string Dir = "Assets/_Project/Data/Bosses/MirrorDuelist";
        private const string PrefabDir = "Assets/_Project/Prefabs/Bosses/";
        private static readonly Color Accent = new(0.725f, 0.651f, 1f);

        public static BossData Build()
        {
            var duelist = BossBuildKit.Asset<MirrorDuelistData>($"{Dir}/MirrorDuelistData.asset");
            BossBuildKit.SetObject(duelist, "_catalog", AssetDatabase.LoadAssetAtPath<AbilityCatalog>("Assets/_Project/Data/Config/AbilityCatalog.asset"));
            var echo = BossBuildKit.Asset<MirrorEchoBehaviour>($"{Dir}/MirrorEchoBehaviour.asset");
            BossBuildKit.SetObject(echo, "_data", duelist);
            BossBuildKit.SetObject(echo, "_shardPrefab", ShockPrefab("MirrorShard", 15f, 1.7f, 26, new Vector2(1f, 0.8f), 0.7f).GetComponent<BellShockwave>());
            BossBuildKit.SetObject(echo, "_wavePrefab", ShockPrefab("MirrorWave", 11f, 2.4f, 24, new Vector2(1.5f, 1.7f), 1f).GetComponent<BellShockwave>());

            var data = BossBuildKit.Asset<BossData>($"{Dir}/MirrorDuelistBossData.asset");
            var prefabPath = BossBuildKit.CopyBossPrefab("TestBoss", "MirrorDuelist");
            var arena = BossBuildKit.CopyArena("TestBossArena", "MirrorDuelistArena");
            ArenaLayoutSetup.LayOut("MirrorDuelistArena");
            var template = BuildReflectionTemplate();

            var lunge = Attack("RapierLunge", "rapier_lunge", new AttackSpec
            {
                Windup = 0.6f, Active = 0.25f, Recovery = 0.95f, Punish = 0.8f, Damage = 28, Knockback = new(9f, 6f),
                HitOffset = new(2.9f, 0f), HitSize = new(4f, 2.2f), WeakPoint = new(2.1f, 1.4f), Lunge = 5f, LungeTime = 0.2f,
                Weight = 1.2f, MinRange = 2f, MaxRange = 10f, WindupCue = "Windup_ShieldBashCue", StrikeCue = "Strike_st_gk_bashCue",
            });
            var cut = Attack("MirrorCut", "mirror_cut", Cut(0.55f, 0.9f, null, 1f));
            var cutSecond = Attack("MirrorCutSecond", "mirror_cut_second", Cut(0.35f, 0.95f, null, 1f));
            var cutDouble = Attack("MirrorCutDouble", "mirror_cut_double", Cut(0.55f, 0.25f, cutSecond, 1.1f));
            var mirrorEcho = Attack("MirrorEcho", "mirror_echo", new AttackSpec
            {
                Windup = 1.15f, Active = 0.3f, Recovery = 1.2f, Punish = 1f, Damage = 26, Knockback = new(8f, 6f),
                HitOffset = new(2f, 0.6f), HitSize = new(3.2f, 2.8f), WeakPoint = new(-0.4f, 2.1f), Behaviour = echo,
                Weight = 1.5f, Cooldown = 3f, MaxRange = 18f, WindupCue = "Windup_RingingTollCue", StrikeCue = "Strike_st_bt_tollCue",
            });

            var so = new SerializedObject(data);
            so.FindProperty("_id").stringValue = "mirror_duelist";
            so.FindProperty("_displayName").stringValue = "The Mirror Duelist";
            so.FindProperty("_floor").intValue = 5;
            so.FindProperty("_accentColor").colorValue = Accent;
            so.FindProperty("_baseHP").intValue = 680;
            so.FindProperty("_idleTime").floatValue = 0.5f;
            so.FindProperty("_targetSwitchInterval").floatValue = 7f;
            so.FindProperty("_walkSpeed").floatValue = 4f;
            BossBuildKit.SetAttacks(so.FindProperty("_phase1Attacks"), lunge, cut, mirrorEcho);
            BossBuildKit.SetAttacks(so.FindProperty("_phase2Attacks"), lunge, cutDouble, mirrorEcho);
            so.FindProperty("_arenaPrefab").objectReferenceValue = arena;
            so.ApplyModifiedPropertiesWithoutUndo();

            BossPrefabShell.Build(prefabPath, data, new Vector2(2.2f, 4.4f), BuildShapes, root => WireComponents(root, duelist, template));
            so = new SerializedObject(data);
            so.FindProperty("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            return data;
        }

        private static AttackSpec Cut(float windup, float recovery, AttackData followUp, float weight) => new()
        {
            Windup = windup, Active = 0.2f, Recovery = recovery, Punish = followUp != null ? 0.2f : 0.8f, Damage = 24, Knockback = new(8f, 6f),
            HitOffset = new(1.5f, -0.9f), HitSize = new(3.2f, 2.6f), WeakPoint = new(-0.8f, 1.9f), FollowUp = followUp,
            Weight = weight, MaxRange = 5f, WindupCue = "Windup_SweepingCutCue", StrikeCue = "Strike_st_gk_sweepCue",
        };

        private static AttackData Attack(string name, string id, AttackSpec spec) => BossBuildKit.Attack(Dir, name, id, spec);

        private static void BuildShapes(Transform visuals, Sprite square)
        {
            const int layer = 11;
            BossBuildKit.Shape(visuals, "Body", Vector2.zero, new(1.9f, 4.2f), BossBuildKit.Muted(Accent, 0.75f), square, 0, layer);
            BossBuildKit.Shape(visuals, "Rapier", new(2.3f, -0.2f), new(2.4f, 0.18f), new(0.9f, 0.9f, 0.95f), square, 1, layer);
            BossBuildKit.Shape(visuals, "Mirror", new(1.35f, 0.3f), new(0.5f, 2.2f), Color.white, square, 2, layer);
            BossBuildKit.Shape(visuals, "Eye", new(0.5f, 1.4f), new(0.7f, 0.25f), new(0.95f, 0.98f, 1f), square, 3, layer);
        }

        private static void WireComponents(GameObject root, MirrorDuelistData data, GameObject template)
        {
            var memory = root.GetComponent<MirrorMemory>() ?? root.AddComponent<MirrorMemory>();
            BossBuildKit.SetObject(memory, "_data", data);
            BossBuildKit.SetObject(memory, "_mirror", root.transform.Find("Visuals/Mirror").GetComponent<SpriteRenderer>());
            var reflections = root.GetComponent<MirrorReflections>() ?? root.AddComponent<MirrorReflections>();
            BossBuildKit.SetObject(reflections, "_data", data);
            BossBuildKit.SetObject(reflections, "_template", template);
        }

        private static GameObject BuildReflectionTemplate()
        {
            var path = PrefabDir + "MirrorReflection.prefab";
            if (System.IO.File.Exists(path)) return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "TestBoss.prefab");
            var go = new GameObject("MirrorReflection");
            go.transform.localScale = new Vector3(1.9f, 4.2f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = source.GetComponentInChildren<SpriteRenderer>(true).sprite;
            sr.sortingOrder = 5;
            var hit = new GameObject("Hit") { layer = Core.Layers.BossAttackIndex };
            hit.transform.SetParent(go.transform, false);
            hit.AddComponent<BoxCollider2D>().isTrigger = true;
            var hso = new SerializedObject(hit.AddComponent<Hitbox>());
            hso.FindProperty("_team").enumValueIndex = 2;
            hso.FindProperty("_detection").enumValueIndex = 1;
            hso.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        /// <summary>A copy of the Bell Twins shockwave prefab with its own data, tinted for the Duelist.</summary>
        private static GameObject ShockPrefab(string name, float speed, float lifetime, int damage, Vector2 hitSize, float scale)
        {
            var path = PrefabDir + name + ".prefab";
            var dataPath = $"{Dir}/{name}Data.asset";
            if (!System.IO.File.Exists(path)) AssetDatabase.CopyAsset(PrefabDir + "BellShockwave.prefab", path);
            if (!System.IO.File.Exists(dataPath)) AssetDatabase.CopyAsset("Assets/_Project/Data/Bosses/BellTwins/ShockwaveData.asset", dataPath);
            var data = AssetDatabase.LoadAssetAtPath<ShockwaveData>(dataPath);
            var dso = new SerializedObject(data);
            dso.FindProperty("_speed").floatValue = speed;
            dso.FindProperty("_lifetime").floatValue = lifetime;
            dso.FindProperty("_damage").intValue = damage;
            dso.FindProperty("_bossDamage").intValue = 0;
            dso.ApplyModifiedPropertiesWithoutUndo();

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                BossBuildKit.SetObject(root.GetComponent<BellShockwave>(), "_data", data);
                var box = root.GetComponentInChildren<BoxCollider2D>();
                box.size = hitSize;
                box.offset = Vector2.zero;
                foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    r.color = new Color(0.78f, 0.7f, 1f, r.color.a);
                    r.transform.localScale = new Vector3(0.9f * scale, 0.9f * scale, 1f);
                    r.transform.localPosition = Vector3.zero;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
