using GraceRun.Core.Settings;
using GraceRun.Net;
using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Main menu (uGUI, built in code, cinema look): a cream title card, then Host, Join by IP (accepts
    /// "ip:port"), Settings, Controls and Quit, navigable with mouse, keyboard and gamepad (Esc / B goes
    /// back). While a host or join is starting it shows the progress with a Cancel button. The status
    /// line shows refusals, timeouts and "Host left the game.". Loads the saved settings on first show.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        private const string Title = "BOPLBORNE";
        private const string Tagline = "Climb the tower. Bop the bosses.";
        private const int SortingOrder = 50;

        private MenuPages _pages;
        private GameObject _main;
        private GameObject _busy;
        private Text _status;
        private Text _busyText;
        private InputField _address;
        private InputField _nickname;
        private ConnectionManager _connection;
        private (GameObject join, GameObject settings, GameObject controls, GameObject host, GameObject rooms) _mainPages;
        private GameObject _header;
        private RectTransform _area;

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
            ShowHeader(_pages.Current != _mainPages.settings && _pages.Current != _mainPages.controls);
        }

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("MainMenuCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            var root = canvas.transform;
            _pages = canvas.gameObject.AddComponent<MenuPages>();

            if (ToonKit.Available)
            {
                var version = _connection != null ? $"v{_connection.Config.GameVersion}" : "";
                _header = MainMenuStage.Build(root, Title, Tagline.ToUpperInvariant(), version);
            }
            else
            {
                var shade = UiPalette.SepiaDark;
                MenuKit.Fill(root, "Backdrop", new Color(shade.r, shade.g, shade.b, 0.94f));
                BuildTitleCard(root);
            }
            _status = MenuKit.Label(root, "", MenuStyle.SmallSize, MenuStyle.OnBackdrop);
            Place(_status.rectTransform, 0.03f, 44f);

            if (_connection == null)
            {
                _status.text = "Start from the Bootstrap scene.";
                return;
            }

            var pages = Area(root);
            _area = (RectTransform)pages;
            _main = BuildMain(pages);
            var join = BuildJoin(pages, out var joinFirst);
            _busy = BuildBusy(pages, out var cancel);
            var settings = SettingsPage.Build(pages, _pages, out var settingsFirst);
            var controls = ControlsPage.Build(pages, _pages, out var controlsFirst);

            var host = HostPage.Build(pages, _pages, _connection, CommitNickname, out var hostFirst);
            var rooms = RoomBrowserPage.Build(pages, _pages, _connection, CommitNickname, out var roomsFirst);

            _pages.Add(join, joinFirst);
            _pages.Add(host, hostFirst);
            _pages.Add(rooms, roomsFirst);
            _pages.Add(_busy, cancel);
            _pages.Add(settings, settingsFirst);
            _pages.Add(controls, controlsFirst);
            _mainPages = (join, settings, controls, host, rooms);
            _pages.Reset(_main);
        }

        /// <summary>The cream title card at the top: the game name in the display font, the tagline and version below.</summary>
        private void BuildTitleCard(Transform root)
        {
            var theme = UiTheme.Current;
            var card = UiKit.NewImage(root, "TitleCard", theme != null ? theme.TitleCard : null, Color.white);
            var rect = card.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.875f);
            rect.sizeDelta = new Vector2(1100f, 250f);
            rect.anchoredPosition = Vector2.zero;
            var title = UiKit.NewText(card.transform, "Title", Title, MenuStyle.TitleSize, UiPalette.Ink, TextAnchor.MiddleCenter, true);
            Place(title.rectTransform, 0.58f, 120f);
            var version = _connection != null ? $"   /   v{_connection.Config.GameVersion}" : "";
            var tagline = UiKit.NewText(card.transform, "Tagline", Tagline + version, MenuStyle.SmallSize, UiPalette.Sepia);
            Place(tagline.rectTransform, 0.22f, 40f);
        }

        private GameObject BuildMain(Transform parent)
        {
            var toon = ToonKit.Available;
            var column = toon ? MenuKit.Stack(parent, "Main", 700f, 10f) : MenuKit.Column(parent, "Main", MenuStyle.PanelWidth - 120f);
            if (!toon) MenuKit.Label(column, "Your name (shown to the others)", MenuStyle.SmallSize, MenuStyle.Fog);
            _nickname = NameTagField.Build(column, GameSettings.Nickname, NicknameRules.MaxLength);
            _nickname.onEndEdit.AddListener(_ => CommitNickname());
            var host = MenuKit.Button(column, "Host game", () => _pages.Open(_mainPages.host), ToonTint.Orange, -1f, UiKit.DisplayMedium, 96f);
            var play = MenuKit.Columns(column, "PlayRow", 10f);
            MenuKit.Button(play, "Join code", () => _pages.Open(_mainPages.join), ToonTint.Mint, 1f, UiKit.DisplaySmall, 88f);
            MenuKit.Button(play, "Rooms", () => _pages.Open(_mainPages.rooms), ToonTint.Pink, -1f, UiKit.DisplaySmall, 88f);
            play.gameObject.AddComponent<LayoutElement>().minHeight = 88f;
            var more = MenuKit.Columns(column, "MoreRow", 10f);
            MenuKit.Button(more, "Settings", () => _pages.Open(_mainPages.settings), ToonTint.Sky, -1f, UiKit.BodySmall + 4, 72f);
            MenuKit.Button(more, "Controls", () => _pages.Open(_mainPages.controls), ToonTint.Plum, 1f, UiKit.BodySmall + 4, 72f);
            MenuKit.Button(more, "Quit", Quit, ToonTint.Stone, -1f, UiKit.BodySmall + 4, 72f);
            more.gameObject.AddComponent<LayoutElement>().minHeight = 72f;
            MenuKit.ChainVertical(column);
            _pages.Add(column.gameObject, host);
            return column.gameObject;
        }

        private GameObject BuildJoin(Transform parent, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Join", MenuStyle.PanelWidth);
            MenuKit.Label(column, "JOIN WITH A CODE", MenuStyle.HeadingSize, MenuStyle.Gold);
            MenuKit.Label(column, "Room code, or host address (ip or ip:port)", MenuStyle.SmallSize, MenuStyle.Fog);
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

        /// <summary>The title sign hides on the tall sub-pages (settings, controls) so they have the room.</summary>
        private void ShowHeader(bool show)
        {
            if (_header == null || _area == null || _header.activeSelf == show) return;
            _header.SetActive(show);
            _area.offsetMax = new Vector2(0f, show ? -400f : -40f);
        }

        private static Transform Area(Transform root)
        {
            var go = new GameObject("Pages", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(root, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, 40f);
            rect.offsetMax = new Vector2(0f, ToonKit.Available ? -400f : -290f);
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
