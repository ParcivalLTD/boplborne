using GraceRun.Bosses;
using GraceRun.Run;
using GraceRun.UI.TitleCards;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Floor 4-5 Bosses: builds The Storm Wyrm and The Mirror Duelist and adds them to the run
    /// (RunConfig floors 4 and 5), the title cards and the skip-to-boss list. Safe to run again.
    /// </summary>
    public static class FloorFourFiveBuilder
    {
        private const string RunConfigPath = "Assets/_Project/Data/Config/RunConfig.asset";
        private const string CardsPath = "Assets/_Project/Resources/TitleCards.asset";

        [MenuItem("Grace Run/Build Floor 4-5 Bosses")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            var wyrm = StormWyrmBuilder.Build();
            var duelist = MirrorDuelistBuilder.Build();
            AddToRun(wyrm, duelist);
            AddTitleCards();
            AddToSpawner(wyrm, duelist);
            AssetDatabase.SaveAssets();
            Debug.Log("Floor 4-5 bosses built: The Storm Wyrm and The Mirror Duelist.");
        }

        private static void AddToRun(params BossData[] bosses)
        {
            var config = AssetDatabase.LoadAssetAtPath<RunConfig>(RunConfigPath);
            var so = new SerializedObject(config);
            var floors = so.FindProperty("_floors");
            foreach (var boss in bosses)
            {
                var index = boss.Floor - 1;
                if (floors.arraySize <= index) floors.arraySize = index + 1;
                var list = floors.GetArrayElementAtIndex(index).FindPropertyRelative("_bosses");
                list.arraySize = 1;
                list.GetArrayElementAtIndex(0).objectReferenceValue = boss;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void AddTitleCards()
        {
            var cards = AssetDatabase.LoadAssetAtPath<TitleCardData>(CardsPath);
            var so = new SerializedObject(cards);
            var entries = so.FindProperty("_entries");
            string[,] lines =
            {
                { "storm_wyrm", "Keep moving. The sky has aim.", "The clouds part." },
                { "mirror_duelist", "Every trick you show, it keeps.", "The glass goes quiet." },
            };
            for (var i = 0; i < lines.GetLength(0); i++)
            {
                if (HasEntry(entries, lines[i, 0])) continue;
                entries.arraySize++;
                var e = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                e.FindPropertyRelative("BossId").stringValue = lines[i, 0];
                e.FindPropertyRelative("Tagline").stringValue = lines[i, 1];
                e.FindPropertyRelative("VictoryTagline").stringValue = lines[i, 2];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cards);
        }

        private static bool HasEntry(SerializedProperty entries, string id)
        {
            for (var i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("BossId").stringValue == id) return true;
            return false;
        }

        /// <summary>Adds the bosses to the Game scene's BossSpawner (the F1 "Skip to boss" list) and saves the scene.</summary>
        private static void AddToSpawner(params BossData[] bosses)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game.unity");
            var spawner = Object.FindFirstObjectByType<BossSpawner>(FindObjectsInactive.Include);
            if (spawner == null) return;
            var so = new SerializedObject(spawner);
            var list = so.FindProperty("_bosses");
            foreach (var boss in bosses)
            {
                var known = false;
                for (var i = 0; i < list.arraySize; i++) known |= list.GetArrayElementAtIndex(i).objectReferenceValue == boss;
                if (known) continue;
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = boss;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
    }
}
