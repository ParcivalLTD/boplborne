using GraceRun.Core.Framing;
using GraceRun.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Turns the static ArenaCamera of the Game scene into a follow camera: creates
    /// <c>Data/Config/CameraFollowData</c> and adds <see cref="ArenaCameraRig"/> and
    /// <see cref="OffscreenIndicators"/> to the camera. Safe to re-run.
    /// </summary>
    public static class CameraFollowSetup
    {
        private const string DataPath = "Assets/_Project/Data/Config/CameraFollowData.asset";
        private const string ScenePath = "Assets/_Project/Scenes/Game.unity";

        [MenuItem("Grace Run/Set Up Follow Camera")]
        public static void Run()
        {
            var data = AssetDatabase.LoadAssetAtPath<CameraFollowData>(DataPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<CameraFollowData>();
                AssetDatabase.CreateAsset(data, DataPath);
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var camera = GameObject.Find("ArenaCamera");
            if (camera == null)
            {
                Debug.LogError("No ArenaCamera in the Game scene.");
                return;
            }

            Wire(Ensure<ArenaCameraRig>(camera), data);
            Wire(Ensure<OffscreenIndicators>(camera), data);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Follow camera set up on " + camera.name + ".");
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        private static void Wire(Component component, CameraFollowData data)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty("_data").objectReferenceValue = data;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(component);
        }
    }
}
