using System.IO;
using GraceRun.Bosses;
using GraceRun.Core.Audio;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>The numbers of one <see cref="AttackData"/> asset; <see cref="BossBuildKit.Attack"/> writes them.</summary>
    public class AttackSpec
    {
        public float Windup = 0.8f, Active = 0.2f, Recovery = 1f, Punish = 0.8f;
        public int Damage = 30;
        public Vector2 Knockback = new(8f, 6f), HitOffset = new(2.5f, -0.5f), HitSize = new(3f, 3f), WeakPoint = new(0f, 2f);
        public float Lunge, LungeTime = 0.2f;
        public AttackData FollowUp;
        public BossAttackBehaviour Behaviour;
        public string WindupCue, StrikeCue;
        public float Weight = 1f, Cooldown, MinRange, MaxRange = 8f;
    }

    /// <summary>Shared helpers of the boss builders: assets from data, prefab copies, shapes.</summary>
    public static class BossBuildKit
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/Bosses/";
        private const string ArenaFolder = "Assets/_Project/Prefabs/Arenas/";

        /// <summary>Creates (or loads) the ScriptableObject at <paramref name="path"/>; the caller sets its fields.</summary>
        public static T Asset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>Creates or rewrites the attack asset <paramref name="folder"/>/<paramref name="name"/>.</summary>
        public static AttackData Attack(string folder, string name, string id, AttackSpec s)
        {
            var attack = Asset<AttackData>($"{folder}/{name}.asset");
            var so = new SerializedObject(attack);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_windup").floatValue = s.Windup;
            so.FindProperty("_active").floatValue = s.Active;
            so.FindProperty("_recovery").floatValue = s.Recovery;
            so.FindProperty("_punishWindow").floatValue = s.Punish;
            so.FindProperty("_damage").intValue = s.Damage;
            so.FindProperty("_knockback").vector2Value = s.Knockback;
            so.FindProperty("_hitboxOffset").vector2Value = s.HitOffset;
            so.FindProperty("_hitboxSize").vector2Value = s.HitSize;
            so.FindProperty("_weakPointOffset").vector2Value = s.WeakPoint;
            so.FindProperty("_lungeDistance").floatValue = s.Lunge;
            so.FindProperty("_lungeTime").floatValue = s.LungeTime;
            so.FindProperty("_followUp").objectReferenceValue = s.FollowUp;
            so.FindProperty("_behaviour").objectReferenceValue = s.Behaviour;
            so.FindProperty("_windupCue").objectReferenceValue = Cue(s.WindupCue);
            so.FindProperty("_strikeCue").objectReferenceValue = Cue(s.StrikeCue);
            so.FindProperty("_weight").floatValue = s.Weight;
            so.FindProperty("_cooldown").floatValue = s.Cooldown;
            so.FindProperty("_minRange").floatValue = s.MinRange;
            so.FindProperty("_maxRange").floatValue = s.MaxRange;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(attack);
            return attack;
        }

        /// <summary>An existing audio cue asset by file name (no extension), or null.</summary>
        public static AudioCue Cue(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var guid in AssetDatabase.FindAssets(name + " t:AudioCue"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name) return AssetDatabase.LoadAssetAtPath<AudioCue>(path);
            }
            return null;
        }

        public static void SetObject(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        public static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        public static void SetAttacks(SerializedProperty list, params AttackData[] attacks)
        {
            list.arraySize = attacks.Length;
            for (var i = 0; i < attacks.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = attacks[i];
        }

        /// <summary>Copies <paramref name="sourceName"/>.prefab to <paramref name="newName"/>.prefab (once) and returns the new path.</summary>
        public static string CopyBossPrefab(string sourceName, string newName)
        {
            var path = PrefabFolder + newName + ".prefab";
            if (!File.Exists(path)) AssetDatabase.CopyAsset(PrefabFolder + sourceName + ".prefab", path);
            return path;
        }

        /// <summary>Copies an arena prefab (once) and returns the new asset.</summary>
        public static GameObject CopyArena(string sourceName, string newName)
        {
            var path = ArenaFolder + newName + ".prefab";
            if (!File.Exists(path)) AssetDatabase.CopyAsset(ArenaFolder + sourceName + ".prefab", path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        /// <summary>A flat coloured block under <paramref name="parent"/>, using the same square sprite as the placeholder bosses.</summary>
        public static SpriteRenderer Shape(Transform parent, string name, Vector2 position, Vector2 size, Color colour, Sprite sprite, int order, int layer)
        {
            var child = new GameObject(name) { layer = layer };
            child.transform.SetParent(parent, false);
            child.transform.localPosition = position;
            child.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = colour;
            renderer.sortingOrder = order;
            return renderer;
        }

        /// <summary>Removes the placeholder Test Boss shapes from <paramref name="visuals"/> and returns the square sprite they used.</summary>
        public static Sprite ClearShapes(Transform visuals)
        {
            var sprite = visuals.GetComponentInChildren<SpriteRenderer>(true).sprite;
            for (var i = visuals.childCount - 1; i >= 0; i--) Object.DestroyImmediate(visuals.GetChild(i).gameObject);
            return sprite;
        }

        public static Color Muted(Color accent, float brightness) => new(accent.r * brightness, accent.g * brightness, accent.b * brightness, 1f);
    }
}
