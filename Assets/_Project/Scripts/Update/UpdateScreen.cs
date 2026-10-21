using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using GraceRun.UI.Splash;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.Update
{
    /// <summary>The in-window update panel: title, a progress bar, the megabytes so far and a small "Play old version" button.</summary>
    public class UpdateScreen : MonoBehaviour
    {
        private const int SortingOrder = 1100;
        private const float BarWidth = 900f;
        private const float BarHeight = 44f;

        private GameUpdater _updater;
        private Text _title;
        private Text _detail;
        private RectTransform _fill;
        private GameObject _skip;
        private bool _closing;

        /// <summary>Builds the panel for <paramref name="updater"/>; it closes itself when the updater is done.</summary>
        public static UpdateScreen Create(GameUpdater updater)
        {
            var go = new GameObject("UpdateScreen");
            DontDestroyOnLoad(go);
            var screen = go.AddComponent<UpdateScreen>();
            screen._updater = updater;
            screen.Build();
            return screen;
        }

        /// <summary>Removes the panel.</summary>
        public void Close()
        {
            _closing = true;
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_closing || _updater == null) return;
            _title.text = Title(_updater.State, _updater.TargetVersion);
            _detail.text = _updater.Detail.ToUpperInvariant();
            _fill.anchorMax = new Vector2(Mathf.Clamp01(_updater.Progress), 1f);
            _skip.SetActive(_updater.State == UpdateState.Downloading);
        }

        private static string Title(UpdateState state, string version) => state switch
        {
            UpdateState.Downloading => $"NEW VERSION {version}: DOWNLOADING",
            UpdateState.Installing => $"NEW VERSION {version}: UNPACKING",
            UpdateState.Restarting => "RESTARTING",
            UpdateState.Failed => "UPDATE FAILED",
            _ => "LOOKING FOR UPDATES",
        };

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("UpdateCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);

            var back = new GameObject("Stage", typeof(RectTransform), typeof(Image));
            back.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)back.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            back.GetComponent<Image>().color = new Color32(26, 14, 32, 255);

            var logo = BrandKit.AddLogo(canvas.transform, new Vector2(0f, 170f), 280f);
            if (logo != null) logo.gameObject.AddComponent<ToonFloat>().Configure(8f, 1.5f, 1.3f);

            _title = UiKit.NewText(canvas.transform, "Title", string.Empty, UiKit.DisplayMedium, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            UiKit.InkOutline(_title, 4f);
            Place(_title.rectTransform, new Vector2(0f, -40f), new Vector2(1700f, 80f));

            var frame = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(canvas.transform, false);
            frame.GetComponent<Image>().color = new Color32(60, 38, 70, 255);
            Place((RectTransform)frame.transform, new Vector2(0f, -130f), new Vector2(BarWidth, BarHeight));

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(frame.transform, false);
            fill.GetComponent<Image>().color = new Color32(255, 170, 60, 255);
            _fill = (RectTransform)fill.transform;
            _fill.anchorMin = Vector2.zero;
            _fill.anchorMax = new Vector2(0f, 1f);
            _fill.offsetMin = new Vector2(4f, 4f);
            _fill.offsetMax = new Vector2(-4f, -4f);

            _detail = UiKit.NewText(canvas.transform, "Detail", string.Empty, UiKit.BodyNormal, UiPalette.Cream, TextAnchor.MiddleCenter);
            Place(_detail.rectTransform, new Vector2(0f, -200f), new Vector2(1400f, 60f));

            var skip = MenuKit.Button(canvas.transform, "Play old version", _updater.Skip, ToonTint.Stone, 0f, UiKit.DisplaySmall, 70f);
            Place((RectTransform)skip.transform, new Vector2(0f, -320f), new Vector2(520f, 70f));
            _skip = skip.gameObject;
            _skip.SetActive(false);
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
