using GraceRun.Core.Settings;
using GraceRun.Net;
using GraceRun.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Main menu (uGUI, built in code): Host, Join by IP (accepts "ip:port"), Settings, Controls and
    /// Quit, navigable with mouse, keyboard and gamepad (Esc / B goes back). While a host or join is
    /// starting it shows the progress with a Cancel button. The status line shows refusals, timeouts
    /// and "Host left the game.". Loads the saved settings on first show.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        private const string Title = "GRACE RUN";
        private const string Tagline = "Co-op boss rush for 1–4 friends";
        private const int SortingOrder = 50;

        private MenuPages _pages;
        private GameObject _main;
        private GameObject _busy;
        private Text _status;
        private Text _busyText;
        private InputField _address;
        private InputField _nickname;
        private ConnectionManager _connection;
        private (GameObject join, GameObject settings, GameObject controls) _mainPages;

        private void Awake()
        {
            _connection = ConnectionManager.Instance;
            if (_connection != null) GameSettings.EnsureLoaded(_connection.Config.Settings);
            MenuKit.EnsureEventSystem();
            Build();
            ShowStatus(ConnectionManager.LastMessage);
            ConnectionManager.LastMessage = null;
        }

        private void OnEnable()
        {
            if (ConnectionManager.Instance != null) ConnectionManager.Instance.StatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            if (ConnectionManager.Instance != null) ConnectionManager.Instance.StatusChanged -= OnStatusChanged;
        }

        private void Update()
        {
            if (_connection == null) return;
            var busy = _connection.IsBusy;
            if (busy && _pages.Current != _busy) _pages.Reset(_busy);
            else if (!busy && _pages.Current == _busy) _pages.Reset(_main);
        }

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("MainMenuCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            var root = canvas.transform;
            MenuKit.Fill(root, "Backdrop", MenuStyle.Backdrop);
            _pages = canvas.gameObject.AddComponent<MenuPages>();

            Place(MenuKit.Label(root, Title, MenuStyle.TitleSize, MenuStyle.Gold).rectTransform, 0.86f, 110f);
            var version = _connection != null ? $"   ·   v{_connection.Config.GameVersion}" : "";
            Place(MenuKit.Label(root, Tagline + version, MenuStyle.SmallSize, MenuStyle.Fog).rectTransform, 0.79f, 40f);
            _status = MenuKit.Label(root, "", MenuStyle.SmallSize, MenuStyle.Bone);
            Place(_status.rectTransform, 0.07f, 60f);

            if (_connection == null)
            {
                _status.text = "Start from the Bootstrap scene.";
                return;
            }

            var pages = Area(root);
            _main = BuildMain(pages);
            var join = BuildJoin(pages, out var joinFirst);
            _busy = BuildBusy(pages, out var cancel);
            var settings = SettingsPage.Build(pages, _pages, out var settingsFirst);
            var controls = ControlsPage.Build(pages, _pages, out var controlsFirst);

            _pages.Add(join, joinFirst);
            _pages.Add(_busy, cancel);
            _pages.Add(settings, settingsFirst);
            _pages.Add(controls, controlsFirst);
            _mainPages = (join, settings, controls);
            _pages.Reset(_main);
        }

        private GameObject BuildMain(Transform parent)
        {
            var column = MenuKit.Column(parent, "Main", MenuStyle.PanelWidth - 160f);
            MenuKit.Label(column, "Your name (shown to the others)", MenuStyle.SmallSize, MenuStyle.Fog);
            _nickname = MenuKit.Field(column, GameSettings.Nickname, NicknameRules.MaxLength);
            _nickname.onEndEdit.AddListener(_ => CommitNickname());
            var host = MenuKit.Button(column, "Host game", () =>
            {
                CommitNickname();
                _connection.Host();
            });
            MenuKit.Button(column, "Join game", () => _pages.Open(_mainPages.join));
            MenuKit.Button(column, "Settings", () => _pages.Open(_mainPages.settings));
            MenuKit.Button(column, "Controls", () => _pages.Open(_mainPages.controls));
            MenuKit.Button(column, "Quit", Quit);
            MenuKit.ChainVertical(column);
            _pages.Add(column.gameObject, host);
            return column.gameObject;
        }

        private GameObject BuildJoin(Transform parent, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Join", MenuStyle.PanelWidth);
            MenuKit.Label(column, "JOIN A GAME", MenuStyle.HeadingSize, MenuStyle.Gold);
            MenuKit.Label(column, "Host address (ip or ip:port)", MenuStyle.SmallSize, MenuStyle.Fog);
            _address = MenuKit.Field(column, _connection.Config.DefaultAddress);
            var connect = MenuKit.Button(column, "Connect", () =>
            {
                CommitNickname();
                _connection.Join(_address.text);
            });
            MenuKit.Button(column, "Back", _pages.Back);
            MenuKit.ChainVertical(column);
            first = connect;
            return column.gameObject;
        }

        private GameObject BuildBusy(Transform parent, out Selectable cancel)
        {
            var column = MenuKit.Column(parent, "Busy", MenuStyle.PanelWidth);
            _busyText = MenuKit.Label(column, "Connecting...", MenuStyle.BodySize, MenuStyle.Bone);
            cancel = MenuKit.Button(column, "Cancel", () => _connection.Leave());
            return column.gameObject;
        }

        private static Transform Area(Transform root)
        {
            var go = new GameObject("Pages", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(root, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, -200f);
            return rect;
        }

        private static void Place(RectTransform rect, float anchorY, float height)
        {
            rect.anchorMin = new Vector2(0f, anchorY);
            rect.anchorMax = new Vector2(1f, anchorY);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        /// <summary>Saves what is in the name field (cleaned up) and shows the cleaned name back.</summary>
        private void CommitNickname()
        {
            if (_nickname == null) return;
            GameSettings.SetNickname(_nickname.text);
            _nickname.text = GameSettings.Nickname;
        }

        private void OnStatusChanged(string message) => ShowStatus(message);

        private void ShowStatus(string message)
        {
            if (_status != null) _status.text = message ?? "";
            if (_busyText != null && !string.IsNullOrEmpty(message)) _busyText.text = message;
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
