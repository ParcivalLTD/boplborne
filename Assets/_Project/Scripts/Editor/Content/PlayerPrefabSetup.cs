using GraceRun.Abilities;
using GraceRun.Combat;
using GraceRun.Player;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Adds what the extra abilities need to existing prefabs: <see cref="AbilityMotion"/> on the
    /// Player, and <see cref="StatusEffects"/> on the Player, the dummy and every boss or minion
    /// that has a Health on its root. Skips what is already there.
    /// </summary>
    public static class PlayerPrefabSetup
    {
        private const string PlayerPath = "Assets/_Project/Prefabs/Player.prefab";
        private const string DummyPath = "Assets/_Project/Prefabs/TrainingDummy.prefab";
        private const string BossFolder = "Assets/_Project/Prefabs/Bosses";

        [MenuItem("Grace Run/Set Up Player And Boss Prefabs")]
        public static void Apply()
        {
            Edit(PlayerPath, root =>
            {
                if (root.GetComponent<AbilityMotion>() == null)
                {
                    var motion = new SerializedObject(root.AddComponent<AbilityMotion>());
                    SerializedFields.Set(motion, "_movement", root.GetComponent<PlayerMovement>());
                    SerializedFields.Set(motion, "_health", root.GetComponent<Health>());
                    motion.ApplyModifiedPropertiesWithoutUndo();
                }
                AddStatus(root);
                AddPlatformDrop(root);
            });
            Edit(DummyPath, AddStatus);
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { BossFolder }))
                Edit(AssetDatabase.GUIDToAssetPath(guid), AddStatus);
        }

        private static void AddPlatformDrop(GameObject root)
        {
            var movement = new SerializedObject(root.GetComponent<PlayerMovement>());
            var drop = root.GetComponent<PlayerPlatformDrop>();
            if (drop == null)
            {
                drop = root.AddComponent<PlayerPlatformDrop>();
                var dropSo = new SerializedObject(drop);
                SerializedFields.Set(dropSo, "_data", movement.FindProperty("_data").objectReferenceValue);
                SerializedFields.Set(dropSo, "_groundCheck", movement.FindProperty("_groundCheck").objectReferenceValue);
                dropSo.ApplyModifiedPropertiesWithoutUndo();
            }
            SerializedFields.Set(movement, "_platformDrop", drop);
            movement.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddStatus(GameObject root)
        {
            if (root.GetComponent<Health>() != null && root.GetComponent<StatusEffects>() == null)
                root.AddComponent<StatusEffects>();
        }

        private static void Edit(string path, System.Action<GameObject> change)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                change(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
