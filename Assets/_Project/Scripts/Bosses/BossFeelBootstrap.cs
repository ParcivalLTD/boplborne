using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraceRun.Bosses
{
    /// <summary>Creates the <see cref="BossFeelView"/> when the Game scene loads, so no boss prefab needs changing.</summary>
    public static class BossFeelBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name != "Game" || Application.isBatchMode) return;
            if (Object.FindAnyObjectByType<BossFeelView>() != null) return;
            new GameObject("BossFeelView").AddComponent<BossFeelView>();
        };
    }
}
