using GraceRun.Core;
using GraceRun.Core.Settings;
using GraceRun.Net;
using GraceRun.Player;
using GraceRun.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GraceRun.UI
{
    /// <summary>
    /// In-game menu on Esc / Start: Resume, Settings, Controls, Leave game, Quit. The game is online,
    /// so nothing stops: the menu only blocks your own gameplay input while it is open
    /// (<see cref="PlayerInputReader.GameplayBlocked"/>). Created on every PC when the Game scene loads;
    /// never on a server-only build.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private const int SortingOrder = 200;

        private MenuPages _pages;
        private GameObject _canvas;
        private GameObject _root;
        private UnityEngine.UI.Text _roomCode;

        /// <summary>True while the menu is on screen.</summary>
        public bool IsOpen => _canvas != null && _canvas.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SceneManager.sceneLoaded += OnSceneLoaded;

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != Scenes.Game || Application.isBatchMode) return;
            if (FindAnyObjectByType<PauseMenu>() != null) return;
            new GameObject("PauseMenu").AddComponent<PauseMenu>();
        }

        private void Awake()
        {
            var connection = ConnectionManager.Instance;
            if (connection != null) GameSettings.EnsureLoaded(connection.Config.Settings);
            MenuKit.EnsureEventSystem();
            Build();
            _canvas.SetActive(false);
        }

        private void OnEnable() => PlayerInputReader.LocalPausePressed += OnPausePressed;

        private void OnDisable()
        {
            PlayerInputReader.LocalPausePressed -= OnPausePressed;
            PlayerInputReader.GameplayBlocked = false;
        }

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("PauseCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            _canvas = canvas.gameObject;
            MenuKit.Fill(canvas.transform, "Backdrop", MenuStyle.Backdrop);
            _pages = _canvas.AddComponent<MenuPages>();
            _pages.BackedOutOfRoot += Close;

            var settings = SettingsPage.Build(canvas.transform, _pages, out var settingsFirst);
            var controls = ControlsPage.Build(canvas.transform, _pages, out var controlsFirst);

            var column = MenuKit.Column(canvas.transform, "Pause", MenuStyle.PanelWidth - 160f);
            MenuKit.Label(column, "PAUSED", MenuStyle.HeadingSize, MenuStyle.Gold);
            MenuKit.Label(column, "The fight goes on while this is open.", MenuStyle.SmallSize, MenuStyle.Fog);
            _roomCode = MenuKit.Label(column, "", MenuStyle.BodySize, MenuStyle.Bone);
            var resume = MenuKit.Button(column, "Resume", Close);
            MenuKit.Button(column, "Settings", () => _pages.Open(settings));
            MenuKit.Button(column, "Controls", () => _pages.Open(controls));
            MenuKit.Button(column, "Leave game", Leave);
            MenuKit.Button(column, "Quit to desktop", Quit);
            MenuKit.ChainVertical(column);
            _root = column.gameObject;

            _pages.Add(_root, resume);
            _pages.Add(settings, settingsFirst);
            _pages.Add(controls, controlsFirst);
        }

        private void OnPausePressed()
        {
            if (!IsOpen) Open();
            else if (Time.frameCount != _pages.LastBackFrame) Close();
        }

        private void Open()
        {
            _canvas.SetActive(true);
            var code = ConnectionManager.Instance != null ? ConnectionManager.Instance.JoinCode : null;
            _roomCode.text = string.IsNullOrEmpty(code) ? "" : $"Room code: {code}";
            _pages.Reset(_root);
            PlayerInputReader.GameplayBlocked = true;
        }

        private void Close()
        {
            _pages.Close();
            _canvas.SetActive(false);
            PlayerInputReader.GameplayBlocked = false;
        }

        private void Leave()
        {
            Close();
            if (ConnectionManager.Instance != null) ConnectionManager.Instance.Leave();
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
