using GraceRun.Core;
using GraceRun.Net;
using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraceRun.UI.Splash
{
    /// <summary>
    /// The loading window: while a host or join is working (opening the room, finding it, connecting,
    /// loading the game scene) it covers the menu with the dark stage, the bobbing Bopped Bell, the current
    /// step and a tip, with a Cancel button. Hides itself once the game scene is up or the attempt ends.
    /// Created once by <see cref="ConnectionManager"/>; never on a server.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        private const int SortingOrder = 900;
        private const float BobPixels = 14f;
        private const float BobSpeed = 3f;
        private const string DefaultMessage = "Getting ready";

        private static readonly string[] Tips =
        {
            "Roll through an attack to dodge it. Friends' swings too.",
            "Friendly fire is on, but a friend's hit never takes you below 1 HP.",
            "A fallen friend floats as a ghost. Stand by them to bring them back.",
            "Pick your weapon in the hub or at a rest stop.",
            "Bosses show their next attack in their pose. Watch the wind-up.",
            "Some abilities love each other. Try pairing them for a combo.",
            "Hosting? Your room code is on your clipboard. Paste it to your friends.",
        };

        private ConnectionManager _connection;
        private GameConfig _config;
        private CanvasGroup _group;
        private RectTransform _logo;
        private Vector2 _logoHome;
        private Text _message;
        private Text _tip;
        private string _status = DefaultMessage;
        private float _alpha;
        private float _shownAt = -1f;

        /// <summary>Builds the loading window for <paramref name="connection"/>. Skipped in batch mode.</summary>
        public static void Create(ConnectionManager connection, GameConfig config)
        {
            if (Application.isBatchMode || FindAnyObjectByType<LoadingScreen>() != null) return;
            var go = new GameObject("LoadingScreen");
            DontDestroyOnLoad(go);
            var screen = go.AddComponent<LoadingScreen>();
            screen._connection = connection;
            screen._config = config;
            screen.Build();
        }

        private void OnEnable() => ConnectionManager.StatusAnnounced += OnStatus;

        private void OnDisable() => ConnectionManager.StatusAnnounced -= OnStatus;

        private void Update()
        {
            if (_connection == null) return;
            var inGame = SceneManager.GetActiveScene().name == Scenes.Game;
            var want = LoadingRules.ShouldShow(_connection.IsBusy, inGame);
            var now = Time.unscaledTime;
            if (want && _shownAt < 0f) _shownAt = now;
            // A quick connect still shows the window for the minimum time.
            var hold = !want && _shownAt >= 0f && !LoadingRules.CanHide(now - _shownAt, _config.LoadingMinSeconds);
            var target = want || hold ? 1f : 0f;
            if (target == 0f && _alpha <= 0f)
            {
                _shownAt = -1f;
                _status = DefaultMessage;
            }

            _alpha = LoadingRules.NextAlpha(_alpha, target, Time.unscaledDeltaTime, _config.LoadingFadeSeconds);
            _group.alpha = _alpha;
            _group.blocksRaycasts = _alpha > 0.01f;
            _group.interactable = _alpha > 0.5f;
            if (_alpha <= 0f) return;

            var elapsed = now - Mathf.Max(0f, _shownAt);
            _logo.anchoredPosition = _logoHome + new Vector2(0f, Mathf.Sin(elapsed * BobSpeed) * BobPixels);
            _message.text = LoadingRules.Dots(_status, elapsed).ToUpperInvariant();
            _tip.text = Tips[LoadingRules.TipIndex(elapsed, Tips.Length, _config.LoadingTipSeconds)];
        }

        private void OnStatus(string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) _status = message;
        }

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("LoadingCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            _group = canvas.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;

            var back = new GameObject("Stage", typeof(RectTransform), typeof(Image));
            back.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)back.transform);
            back.GetComponent<Image>().color = new Color32(26, 14, 32, 255);

            _logoHome = new Vector2(0f, 110f);
            var logo = BrandKit.AddLogo(canvas.transform, _logoHome, 300f);
            _logo = logo != null ? logo.rectTransform : new GameObject("NoLogo", typeof(RectTransform)).GetComponent<RectTransform>();
            if (logo == null) _logo.SetParent(canvas.transform, false);

            _message = UiKit.NewText(canvas.transform, "Message", DefaultMessage, UiKit.DisplayMedium, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            UiKit.InkOutline(_message, 4f);
            Place(_message.rectTransform, new Vector2(0f, -110f), new Vector2(1600f, 80f));

            _tip = UiKit.NewText(canvas.transform, "Tip", "", UiKit.BodyNormal, UiPalette.Cream, TextAnchor.MiddleCenter);
            Place(_tip.rectTransform, new Vector2(0f, -210f), new Vector2(1400f, 80f));

            var cancel = MenuKit.Button(canvas.transform, "Cancel", () => _connection.Leave(), ToonTint.Stone, 0f, UiKit.DisplaySmall, 80f);
            var cr = (RectTransform)cancel.transform;
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(320f, 80f);
            cr.anchoredPosition = new Vector2(0f, -330f);
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
