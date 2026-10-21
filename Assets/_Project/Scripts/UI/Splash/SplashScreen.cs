using GraceRun.Core;
using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace GraceRun.UI.Splash
{
    /// <summary>
    /// The start-up splash: a dark stage, the marquee sign with the game's name and the blue blob knight,
    /// over the main menu, which is already loaded underneath. Any key, click or button skips it.
    /// Plays once per launch, never on a server or a command-line join.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        private const string Title = "BOPLBORNE";
        private const string Tagline = "Climb the tower. Bop the bosses.";
        private const string Hint = "PRESS ANY KEY";
        private const int SortingOrder = 1000;
        private const float SkipGraceSeconds = 0.4f;
        private const float HintDelaySeconds = 1.0f;

        private static bool _played;

        // With domain reload off the static would survive a stop / play in the Editor.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _played = false;

        private CanvasGroup _panel;
        private Image _cover;
        private Text _hint;
        private float _fadeSeconds;
        private float _total;
        private float _start;
        private System.IDisposable _listener;
        private bool _skipped;
        private float _skippedAt;

        /// <summary>Shows the splash unless it already played or there is no screen.</summary>
        public static void PlayOnce(GameConfig config)
        {
            if (_played || Application.isBatchMode || config == null) return;
            _played = true;
            var go = new GameObject("Splash");
            DontDestroyOnLoad(go);
            go.AddComponent<SplashScreen>().Begin(config.SplashSeconds, config.SplashFadeSeconds);
        }

        private void Begin(float seconds, float fadeSeconds)
        {
            _total = seconds;
            _fadeSeconds = fadeSeconds;
            _start = Time.unscaledTime;
            Build();
            _listener = InputSystem.onAnyButtonPress.Call(_ => OnAnyButton());
        }

        private void OnDestroy() => _listener?.Dispose();

        private void Update()
        {
            var time = Time.unscaledTime - _start;
            if (_skipped) _total = Mathf.Min(_total, _skippedAt + _fadeSeconds);
            _cover.color = new Color(0f, 0f, 0f, SplashRules.CoverAlpha(time, _fadeSeconds));
            _panel.alpha = SplashRules.PanelAlpha(time, _total, _fadeSeconds);
            _hint.enabled = time >= HintDelaySeconds && Mathf.Repeat(time, 1.2f) < 0.8f;
            if (SplashRules.IsDone(time, _total)) Destroy(gameObject);
        }

        private void OnAnyButton()
        {
            var time = Time.unscaledTime - _start;
            if (this == null || _skipped || !SplashRules.CanSkip(time, SkipGraceSeconds)) return;
            _skipped = true;
            _skippedAt = time;
            _total = time + _fadeSeconds;
        }

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("SplashCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            _panel = canvas.gameObject.AddComponent<CanvasGroup>();

            var back = new GameObject("Stage", typeof(RectTransform), typeof(Image));
            back.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)back.transform);
            back.GetComponent<Image>().color = new Color32(26, 14, 32, 255);

            if (ToonKit.Available) BuildLogo(canvas.transform);
            else BuildPlainLogo(canvas.transform);

            _hint = UiKit.NewText(canvas.transform, "Hint", Hint, UiKit.BodySmall, UiPalette.Cream, TextAnchor.LowerCenter, true);
            var hr = _hint.rectTransform;
            hr.anchorMin = new Vector2(0f, 0f);
            hr.anchorMax = new Vector2(1f, 0f);
            hr.pivot = new Vector2(0.5f, 0f);
            hr.sizeDelta = new Vector2(0f, 60f);
            hr.anchoredPosition = new Vector2(0f, 40f);
            _hint.raycastTarget = false;

            var cover = new GameObject("Cover", typeof(RectTransform), typeof(Image));
            cover.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)cover.transform);
            _cover = cover.GetComponent<Image>();
            _cover.color = Color.black;
            _cover.raycastTarget = false;
        }

        private static void BuildLogo(Transform root)
        {
            var theme = ToonTheme.Current;
            var sign = ToonKit.NewPoster(root, "Sign", theme.PosterPlum);
            var rect = sign.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1180f, 280f);
            rect.anchoredPosition = new Vector2(0f, 110f);
            sign.raycastTarget = false;
            ToonKit.AddBulbs(rect, 22f, 40f);
            rect.gameObject.AddComponent<ToonPopIn>().Configure(0.2f, 0.6f);

            var title = UiKit.NewText(rect, "Title", Title, UiKit.DisplayBig + 24, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            UiKit.InkOutline(title, 6f);
            Fill(title.rectTransform, new Vector2(30f, 20f), new Vector2(-30f, -20f));

            var ribbon = ToonKit.NewRibbon(root, Tagline.ToUpperInvariant(), ToonTint.Orange, new Vector2(900f, 78f));
            var rr = ribbon.rectTransform;
            rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(0.5f, 0.5f);
            rr.anchoredPosition = new Vector2(0f, -70f);
            ribbon.raycastTarget = false;
            ribbon.gameObject.AddComponent<ToonPopIn>().Configure(0.7f, 0.5f);

            ToonMascot.Create(root, new Vector2(0f, 60f), 6f);

            var logo = BrandKit.AddLogo(root, new Vector2(0f, 400f), 220f);
            if (logo == null) return;
            logo.gameObject.AddComponent<ToonFloat>().Configure(8f, 1.5f, 1.3f);
            logo.gameObject.AddComponent<ToonPopIn>().Configure(0f, 0.5f);
        }

        private static void BuildPlainLogo(Transform root)
        {
            var title = UiKit.NewText(root, "Title", Title, UiKit.DisplayBig, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            Fill(title.rectTransform, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect) => Fill(rect, Vector2.zero, Vector2.zero);

        private static void Fill(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
