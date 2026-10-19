using GraceRun.Player;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Creates the starter weapons (Fists, Sword, Spear, Hammer) and the <see cref="WeaponCatalog"/>
    /// under Data/Weapons, and wires <see cref="PlayerWeapon"/> and <see cref="WeaponView"/> onto
    /// Player.prefab. Assets that already exist are left alone, so tuning survives a second run.
    /// </summary>
    public static class WeaponSetup
    {
        private const string Folder = "Assets/_Project/Data/Weapons";
        private const string LightAttackPath = "Assets/_Project/Data/Player/LightAttackData.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        [MenuItem("Grace Run/Set Up Weapons")]
        public static void Apply()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Data", "Weapons");
            var fists = Ensure("Fists", so => FillFists(so));
            var sword = Ensure("Sword", FillSword);
            var spear = Ensure("Spear", FillSpear);
            var hammer = Ensure("Hammer", FillHammer);
            var catalog = EnsureCatalog(new[] { fists, sword, spear, hammer });
            WirePlayer(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("[WeaponSetup] Weapons ready: Fists, Sword, Spear, Hammer.");
        }

        private static WeaponData Ensure(string name, System.Action<SerializedObject> fill)
        {
            var path = $"{Folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<WeaponData>();
            AssetDatabase.CreateAsset(asset, path);
            var so = new SerializedObject(asset);
            fill(so);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static WeaponCatalog EnsureCatalog(WeaponData[] weapons)
        {
            var path = $"{Folder}/WeaponCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>(path);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WeaponCatalog>();
                AssetDatabase.CreateAsset(catalog, path);
            }
            var so = new SerializedObject(catalog);
            var list = so.FindProperty("_weapons");
            list.arraySize = weapons.Length;
            for (var i = 0; i < weapons.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = weapons[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void WirePlayer(WeaponCatalog catalog)
        {
            var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                var weapon = root.GetComponent<PlayerWeapon>() ?? root.AddComponent<PlayerWeapon>();
                Apply(weapon, so => SerializedFields.Set(so, "_catalog", catalog));

                var attack = root.GetComponent<PlayerLightAttack>();
                Apply(attack, so => SerializedFields.Set(so, "_weapon", weapon));

                var view = root.GetComponent<WeaponView>() ?? root.AddComponent<WeaponView>();
                var body = root.transform.Find("Visuals/Body");
                Apply(view, so =>
                {
                    SerializedFields.Set(so, "_weapon", weapon);
                    SerializedFields.Set(so, "_attack", attack);
                    SerializedFields.Set(so, "_reference", body != null ? body.GetComponent<SpriteRenderer>() : null);
                });
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Apply(Object target, System.Action<SerializedObject> set)
        {
            var so = new SerializedObject(target);
            set(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---- weapon numbers (docs/GDD.md, Weapons) ----

        private static void FillFists(SerializedObject so)
        {
            Meta(so, "Fists", Color.white, Vector2.zero, 0.3f, 0.35f, 0.2f);
            var source = AssetDatabase.LoadAssetAtPath<LightAttackData>(LightAttackPath);
            if (source != null)
            {
                var from = new SerializedObject(source).FindProperty("_hits");
                var to = so.FindProperty("_combo");
                to.arraySize = from.arraySize;
                for (var i = 0; i < from.arraySize; i++) CopyHit(from.GetArrayElementAtIndex(i), to.GetArrayElementAtIndex(i));
            }
            Hit(so.FindProperty("_heavy"), 14, new Vector2(11f, 5f), new Vector2(1.6f, 1.2f), new Vector2(1f, 0f), 0.2f, 0.1f, 0.3f, 18f);
        }

        private static void FillSword(SerializedObject so)
        {
            Meta(so, "Sword", new Color(0.78f, 0.84f, 0.95f), new Vector2(1.5f, 0.14f), 0.3f, 0.35f, 0.25f);
            Combo(so,
                (7, new Vector2(2f, 1f), new Vector2(1.5f, 1.1f), new Vector2(1f, 0f), 0.05f, 0.08f, 0.14f, 8f),
                (7, new Vector2(2f, 1f), new Vector2(1.5f, 1.1f), new Vector2(1f, 0f), 0.05f, 0.08f, 0.14f, 8f),
                (10, new Vector2(9f, 5f), new Vector2(1.8f, 1.2f), new Vector2(1.1f, 0f), 0.08f, 0.1f, 0.26f, 8f));
            Hit(so.FindProperty("_heavy"), 17, new Vector2(12f, 6f), new Vector2(1.8f, 1.3f), new Vector2(1.1f, 0f), 0.2f, 0.1f, 0.3f, 20f);
            Move(so, "_hasAerial", "_aerial", 10, new Vector2(6f, 4f), new Vector2(1.6f, 1.2f), new Vector2(0.9f, 0f), 0.05f, 0.15f, 0.2f, 10f);
            Move(so, "_hasUp", "_up", 9, new Vector2(2f, 10f), new Vector2(1.2f, 1.6f), new Vector2(0.2f, 1f), 0.07f, 0.12f, 0.22f, 10f);
            Move(so, "_hasDown", "_down", 9, new Vector2(2f, -6f), new Vector2(1.2f, 1.4f), new Vector2(0.2f, -1f), 0.08f, 0.12f, 0.25f, 10f);
        }

        private static void FillSpear(SerializedObject so)
        {
            Meta(so, "Spear", new Color(0.6f, 0.85f, 0.7f), new Vector2(2.4f, 0.1f), 0.3f, 0.35f, 0.2f);
            Combo(so,
                (6, new Vector2(1.5f, 0.5f), new Vector2(2.6f, 0.6f), new Vector2(1.8f, 0f), 0.09f, 0.08f, 0.18f, 9f),
                (6, new Vector2(1.5f, 0.5f), new Vector2(2.6f, 0.6f), new Vector2(1.8f, 0f), 0.09f, 0.08f, 0.18f, 9f),
                (9, new Vector2(8f, 3f), new Vector2(3f, 0.7f), new Vector2(2f, 0f), 0.12f, 0.1f, 0.3f, 9f));
            Hit(so.FindProperty("_heavy"), 18, new Vector2(11f, 3f), new Vector2(3.4f, 0.7f), new Vector2(2.2f, 0f), 0.25f, 0.1f, 0.35f, 20f);
            Move(so, "_hasAerial", "_aerial", 9, new Vector2(5f, 3f), new Vector2(2.4f, 0.7f), new Vector2(1.4f, -0.2f), 0.08f, 0.14f, 0.22f, 10f);
            Move(so, "_hasUp", "_up", 10, new Vector2(1.5f, 10f), new Vector2(0.7f, 2.6f), new Vector2(0.2f, 1.6f), 0.1f, 0.12f, 0.25f, 10f);
            Move(so, "_hasDown", "_down", 10, new Vector2(1f, -6f), new Vector2(0.8f, 2.2f), new Vector2(0.3f, -1.4f), 0.1f, 0.12f, 0.25f, 10f);
        }

        private static void FillHammer(SerializedObject so)
        {
            Meta(so, "Hammer", new Color(0.85f, 0.6f, 0.4f), new Vector2(1.3f, 0.3f), 0.4f, 0.5f, 0.1f);
            Combo(so,
                (11, new Vector2(6f, 3f), new Vector2(1.6f, 1.6f), new Vector2(1f, 0.1f), 0.16f, 0.12f, 0.3f, 14f),
                (16, new Vector2(14f, 7f), new Vector2(1.8f, 1.7f), new Vector2(1.1f, 0.1f), 0.2f, 0.12f, 0.4f, 14f));
            Hit(so.FindProperty("_heavy"), 30, new Vector2(18f, 9f), new Vector2(2f, 1.8f), new Vector2(1.2f, 0.1f), 0.4f, 0.14f, 0.55f, 30f);
            Move(so, "_hasAerial", "_aerial", 14, new Vector2(4f, -4f), new Vector2(1.8f, 1.8f), new Vector2(0.7f, -0.2f), 0.15f, 0.15f, 0.3f, 16f);
            Move(so, "_hasUp", "_up", 14, new Vector2(2f, 13f), new Vector2(1.6f, 2f), new Vector2(0.2f, 1.1f), 0.18f, 0.12f, 0.35f, 16f);
            Move(so, "_hasDown", "_down", 14, new Vector2(2f, -8f), new Vector2(1.6f, 1.6f), new Vector2(0.2f, -0.9f), 0.2f, 0.12f, 0.35f, 16f);
        }

        private static void Meta(SerializedObject so, string name, Color color, Vector2 visual, float buffer, float chain, float speed)
        {
            so.FindProperty("_displayName").stringValue = name;
            so.FindProperty("_color").colorValue = color;
            so.FindProperty("_visualSize").vector2Value = visual;
            so.FindProperty("_inputBuffer").floatValue = buffer;
            so.FindProperty("_chainWindow").floatValue = chain;
            so.FindProperty("_moveSpeedMultiplier").floatValue = speed;
        }

        private static void Combo(SerializedObject so, params (int d, Vector2 k, Vector2 s, Vector2 o, float st, float a, float r, float c)[] hits)
        {
            var list = so.FindProperty("_combo");
            list.arraySize = hits.Length;
            for (var i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                Hit(list.GetArrayElementAtIndex(i), h.d, h.k, h.s, h.o, h.st, h.a, h.r, h.c);
            }
        }

        private static void Move(SerializedObject so, string flag, string field, int damage, Vector2 knockback, Vector2 size,
            Vector2 offset, float startup, float active, float recovery, float stamina)
        {
            so.FindProperty(flag).boolValue = true;
            Hit(so.FindProperty(field), damage, knockback, size, offset, startup, active, recovery, stamina);
        }

        private static void CopyHit(SerializedProperty from, SerializedProperty to)
        {
            Hit(to, from.FindPropertyRelative("_damage").intValue, from.FindPropertyRelative("_knockback").vector2Value,
                from.FindPropertyRelative("_hitboxSize").vector2Value, from.FindPropertyRelative("_hitboxOffset").vector2Value,
                from.FindPropertyRelative("_startup").floatValue, from.FindPropertyRelative("_active").floatValue,
                from.FindPropertyRelative("_recovery").floatValue, from.FindPropertyRelative("_staminaCost").floatValue);
        }

        private static void Hit(SerializedProperty p, int damage, Vector2 knockback, Vector2 size, Vector2 offset,
            float startup, float active, float recovery, float stamina)
        {
            p.FindPropertyRelative("_damage").intValue = damage;
            p.FindPropertyRelative("_knockback").vector2Value = knockback;
            p.FindPropertyRelative("_hitboxSize").vector2Value = size;
            p.FindPropertyRelative("_hitboxOffset").vector2Value = offset;
            p.FindPropertyRelative("_startup").floatValue = startup;
            p.FindPropertyRelative("_active").floatValue = active;
            p.FindPropertyRelative("_recovery").floatValue = recovery;
            p.FindPropertyRelative("_staminaCost").floatValue = stamina;
        }
    }
}
