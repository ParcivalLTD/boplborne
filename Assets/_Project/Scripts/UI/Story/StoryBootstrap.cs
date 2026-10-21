using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// Builds the story views (opening, ascent screen, ending extras) when the Game scene loads, like the title
    /// cards. Without <c>Resources/StoryData.asset</c> nothing is built and one info line is logged.
    /// </summary>
    public static class StoryBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != "Game" || Application.isBatchMode) return;
            if (Object.FindAnyObjectByType<StoryCardPlayer>() != null) return;
            if (Resources.Load<StoryData>(StoryData.ResourceName) == null)
            {
                Debug.Log("Story: Resources/StoryData.asset is missing; run Grace Run > Build Story Assets. Story screens are off.");
                return;
            }
            var go = new GameObject("StoryViews");
            go.AddComponent<StoryCardPlayer>();
            go.AddComponent<EndingView>();
            go.AddComponent<AscentView>();
            go.AddComponent<StorySequenceView>();
        };
    }
}
