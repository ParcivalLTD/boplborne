using System;
using System.Collections.Generic;
using System.IO;
using GraceRun.Abilities;
using GraceRun.Abilities.Effects;
using GraceRun.Run;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Extra Abilities: writes the abilities in <see cref="ExtraAbilitySpecs"/>
    /// as AbilityData assets, effect prefabs and placeholder icons, then lists them in the catalog,
    /// the draft pool and the network prefab list. Safe to run again: existing assets keep their
    /// GUIDs and are updated in place. It overwrites tuning on the assets it owns.
    /// </summary>
    public static class ExtraAbilitiesBuilder
    {
        private const string DataFolder = "Assets/_Project/Data/Abilities";
        private const string ChildFolder = "Assets/_Project/Data/Abilities/Children";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Abilities";
        private const string IconFolder = "Assets/_Project/Art/UI/Placeholder";
        private const string CatalogPath = "Assets/_Project/Data/Config/AbilityCatalog.asset";
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string CombosPath = "Assets/_Project/Data/Config/ComboRules.asset";
        private const string NetworkPrefabsPath = "Assets/DefaultNetworkPrefabs.asset";
        private const string CirclePath = "Assets/_Project/Art/FX/Placeholder/circle.png";
        private const string BodyPath = "Assets/_Project/Art/Characters/Placeholder/player_body.png";
        private const string BurstPath = "Assets/_Project/Prefabs/FX/BombBurst.prefab";

        [MenuItem("Grace Run/Build Extra Abilities")]
        public static void Build()
        {
            Directory.CreateDirectory(ChildFolder);
            AssetDatabase.Refresh();
            PlayerPrefabSetup.Apply();

            var made = new List<AbilityData>();
            var prefabs = new List<GameObject>();
            foreach (var spec in ExtraAbilitySpecs.All()) made.Add(BuildAbility(spec, DataFolder, prefabs));

            AppendCatalog(made);
            AppendDraftPool(made);
            AppendNetworkPrefabs(prefabs);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built {made.Count} abilities and {prefabs.Count} effect prefabs.");
        }

        private static AbilityData BuildAbility(AbilitySpec spec, string folder, List<GameObject> prefabs)
        {
            var data = LoadOrCreate<AbilityData>($"{folder}/{spec.Name}.asset");
            AbilityData childData = null;
            if (spec.Child != null) childData = BuildAbility(spec.Child, ChildFolder, prefabs);

            var so = new SerializedObject(data);
            foreach (var pair in spec.Fields) SerializedFields.Set(so, pair.Key, Resolve(pair.Value));
            if (spec.InDraft) SerializedFields.Set(so, "_icon", PlaceholderIcons.Ensure(IconFolder, spec.Id, spec.Color));
            if (childData != null) SerializedFields.Set(so, "_childEffect", childData.EffectPrefab);
            so.ApplyModifiedPropertiesWithoutUndo();

            if (spec.Template != EffectTemplate.None)
            {
                var prefab = BuildPrefab(spec, data);
                prefabs.Add(prefab);
                so.Update();
                SerializedFields.Set(so, "_effectPrefab", prefab.GetComponent<AbilityEffect>());
                if (childData != null) SerializedFields.Set(so, "_childEffect", childData.EffectPrefab);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(data);
            return data;
        }

        private static GameObject BuildPrefab(AbilitySpec spec, AbilityData data)
        {
            var template = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{TemplateName(spec.Template)}.prefab");
            var go = UnityEngine.Object.Instantiate(template);
            go.name = spec.Name;

            var strip = new List<Type>(spec.Remove);
            if (spec.Template == EffectTemplate.Area) strip.Add(typeof(PushWave));
            foreach (var type in strip)
            {
                var existing = go.GetComponent(type);
                if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
            }
            foreach (var type in spec.Add) go.AddComponent(type);

            foreach (var pair in spec.ComponentFields)
            {
                var dot = pair.Key.IndexOf('.');
                var component = go.GetComponent(pair.Key.Substring(0, dot));
                var cso = new SerializedObject(component);
                SerializedFields.Set(cso, pair.Key.Substring(dot + 1), Resolve(pair.Value));
                cso.ApplyModifiedPropertiesWithoutUndo();
            }

            var effect = new SerializedObject(go.GetComponent<AbilityEffect>());
            SerializedFields.Set(effect, "_ability", data);
            SerializedFields.Set(effect, "_combos", AssetDatabase.LoadAssetAtPath<ComboRules>(CombosPath));
            effect.ApplyModifiedPropertiesWithoutUndo();

            StyleVisual(go, spec);
            var saved = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabFolder}/{spec.Name}.prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return saved;
        }

        private static void StyleVisual(GameObject go, AbilitySpec spec)
        {
            var core = go.transform.Find("Core");
            if (core != null) UnityEngine.Object.DestroyImmediate(core.gameObject);
            var visual = go.transform.Find("Visual");
            if (visual == null || !visual.TryGetComponent(out SpriteRenderer sprite)) return;
            sprite.color = spec.Color;
            if (spec.CircleVisual) sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            if (spec.PlayerVisual)
            {
                sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BodyPath);
                visual.localPosition = new Vector3(0f, 1f, 0f);
                visual.localScale = Vector3.one;
            }
            if (spec.VisualScale > 0f) visual.localScale = Vector3.one * spec.VisualScale;
        }

        private static string TemplateName(EffectTemplate template) => template switch
        {
            EffectTemplate.Arrow => "Arrow",
            EffectTemplate.Bomb => "Bomb",
            _ => "Gust",
        };

        private static object Resolve(object value)
        {
            if (value is not string text) return value;
            if (text == "BombBurst") return AssetDatabase.LoadAssetAtPath<GameObject>(BurstPath).GetComponent<BurstFx>();
            if (text.EndsWith("Action")) return LoadOrCreateAction(text);
            return value;
        }

        private static CasterAction LoadOrCreateAction(string name)
        {
            var path = $"{DataFolder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CasterAction>(path);
            if (existing != null) return existing;
            CasterAction action = name == "BlinkAction" ? ScriptableObject.CreateInstance<BlinkAction>() : ScriptableObject.CreateInstance<DashAction>();
            AssetDatabase.CreateAsset(action, path);
            return action;
        }

        private static TAsset LoadOrCreate<TAsset>(string path) where TAsset : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<TAsset>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void AppendCatalog(List<AbilityData> abilities) =>
            AppendToArray(AssetDatabase.LoadAssetAtPath<AbilityCatalog>(CatalogPath), "_abilities", abilities);

        private static void AppendDraftPool(List<AbilityData> abilities) =>
            AppendToArray(AssetDatabase.LoadAssetAtPath<RunConfig>(RunConfigPath), "_draftPool", abilities);

        private static void AppendToArray(UnityEngine.Object owner, string property, List<AbilityData> abilities)
        {
            var so = new SerializedObject(owner);
            var array = so.FindProperty(property);
            foreach (var ability in abilities)
            {
                var present = false;
                for (var i = 0; i < array.arraySize; i++) present |= array.GetArrayElementAtIndex(i).objectReferenceValue == ability;
                if (present) continue;
                array.InsertArrayElementAtIndex(array.arraySize);
                array.GetArrayElementAtIndex(array.arraySize - 1).objectReferenceValue = ability;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(owner);
        }

        private static void AppendNetworkPrefabs(List<GameObject> prefabs)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(NetworkPrefabsPath);
            foreach (var prefab in prefabs)
                if (!list.Contains(prefab)) list.Add(new NetworkPrefab { Prefab = prefab });
            EditorUtility.SetDirty(list);
        }
    }
}
