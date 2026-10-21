using GraceRun.UI.Skin;
using GraceRun.UI.TitleCards;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// The pictures of the ascent screen: a tower cross-section on the right edge with one window per floor and a
    /// lantern that climbs from the old floor's window to the new one (beaten floors go dark with a trophy), and a
    /// caption poster beside it (with the Hollow's silhouette and the twist line before the final fight).
    /// Fills the whole screen on an opaque backdrop. Missing sprites become flat coloured shapes. Cosmetic, no input.
    /// </summary>
    public class AscentTowerPanel
    {
        private const float Scale = 2f;
        private const float TowerX = -430f;
        private static readonly Color PlaceholderLit = new Color32(255, 214, 102, 255);

        private readonly Image[] _windows = new Image[32];
        private readonly Image[] _trophies = new Image[32];
        private GameObject _root;
        private RectTransform _group;
        private CanvasGroup _alpha;
        private Image _lantern;
        private StoryData _data;
        private Vector2[] _centres;
        private Vector2 _towerPixels;
        private int _from;
        private int _to;
        private int _count;

        public bool Visible => _root != null && _root.activeSelf;

        /// <summary>Builds the screen for a climb from <paramref name="from"/> to <paramref name="to"/> of <paramref name="total"/> floors.</summary>
        public void Show(Transform parent, StoryData data, int from, int to, int total, string caption, string twist)
        {
            Hide();
            _data = data;
            _from = from;
            _to = to;
            _count = Mathf.Min(total + 1, _windows.Length);
            _towerPixels = data.Tower != null ? data.Tower.rect.size : new Vector2(160f, 480f);
            _centres = new Vector2[_count];
            for (var i = 0; i < _count; i++) _centres[i] = StoryRules.WindowCentre(data.WindowCentres, i, total, _towerPixels.x, _towerPixels.y);

            _root = new GameObject("Ascent", typeof(RectTransform), typeof(CanvasGroup));
            _root.transform.SetParent(parent, false);
            _alpha = _root.GetComponent<CanvasGroup>();
            _alpha.blocksRaycasts = false;
            _alpha.interactable = false;
            _group = (RectTransform)_root.transform;
            _group.anchorMin = Vector2.zero;
            _group.anchorMax = Vector2.one;
            _group.offsetMin = _group.offsetMax = Vector2.zero;

            var backdrop = UiKit.NewImage(_root.transform, "Backdrop", null, UiPalette.Ink);
            backdrop.raycastTarget = false;
            var b = backdrop.rectTransform;
            b.anchorMin = Vector2.zero;
            b.anchorMax = Vector2.one;
            b.offsetMin = b.offsetMax = Vector2.zero;
            BuildTower();
            BuildCaption(caption, string.IsNullOrEmpty(twist) ? null : twist);
            Pose(0f, data.AscentHold);
        }

        public void Hide()
        {
            if (_root != null) Object.Destroy(_root);
            _root = null;
        }

        /// <summary>Moves the lantern, window states and slide for <paramref name="time"/> seconds into the ascent. Returns true when finished.</summary>
        public bool Pose(float time, float hold)
        {
            var d = _data;
            var p = TitleCardRules.Progress(time, d.AscentSlideIn, hold, d.AscentSlideOut, d.FramesPerSecond);
            if (p.done) return true;
            _alpha.alpha = Mathf.Clamp01(p.enter * 2f) * (1f - p.exit);

            var climb = StoryRules.LanternProgress(time, d.LanternDelay, d.LanternTravel, d.FramesPerSecond);
            var pos = StoryRules.LanternPosition(_centres[Mathf.Min(_from, _count - 1)], _centres[Mathf.Min(_to, _count - 1)], climb);
            _lantern.rectTransform.anchoredPosition = ToLocal(pos);
            for (var i = 0; i < _count; i++)
            {
                var state = StoryRules.StateOf(i, _from, _to, climb);
                var lit = state == WindowState.Lit;
                _windows[i].sprite = lit ? d.WindowLit : d.WindowDark;
                _windows[i].color = _windows[i].sprite != null ? Color.white : lit ? PlaceholderLit : UiPalette.Ink;
                _trophies[i].gameObject.SetActive(state == WindowState.Beaten);
            }
            return false;
        }

        private Vector2 ToLocal(Vector2 pixels) =>
            new((pixels.x - _towerPixels.x * 0.5f) * Scale, (_towerPixels.y * 0.5f - pixels.y) * Scale);

        private void BuildTower()
        {
            var tower = UiKit.NewImage(_root.transform, "Tower", _data.Tower, _data.Tower != null ? Color.white : UiPalette.Stone);
            tower.type = Image.Type.Simple;
            tower.raycastTarget = false;
            var r = tower.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = _towerPixels * Scale;
            r.anchoredPosition = new Vector2(TowerX, 0f);
            for (var i = 0; i < _count; i++)
            {
                _windows[i] = Sprite(tower.transform, "Window", null, 24f, ToLocal(_centres[i]));
                _trophies[i] = Sprite(tower.transform, "Trophy", _data.TrophyFor(i), 16f, ToLocal(_centres[i]));
                if (_data.TrophyFor(i) == null) _trophies[i].color = UiPalette.Brass;
            }
            _lantern = Sprite(tower.transform, "Lantern", _data.Lantern, 16f, Vector2.zero);
            if (_data.Lantern == null) _lantern.color = PlaceholderLit;
        }

        private void BuildCaption(string caption, string twist)
        {
            var poster = UiKit.NewImage(_root.transform, "Caption", UiKit.CardPanel(), UiKit.CardPanel() != null ? Color.white : UiPalette.Cream);
            poster.raycastTarget = false;
            var r = poster.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(720f, twist != null ? 520f : 360f);
            r.anchoredPosition = new Vector2(260f, 0f);
            r.localRotation = Quaternion.Euler(0f, 0f, 1.5f);

            if (twist == null)
            {
                Label(poster.transform, caption, new Vector2(0f, 0f), new Vector2(640f, 280f));
                return;
            }
            var silhouette = Sprite(poster.transform, "Hollow", _data.HollowSilhouette, 96f, new Vector2(-220f, 0f));
            silhouette.rectTransform.sizeDelta = new Vector2(96f, 128f) * 2.4f;
            silhouette.preserveAspect = true;
            if (_data.HollowSilhouette == null) silhouette.color = UiPalette.Ink;
            Label(poster.transform, caption, new Vector2(130f, 110f), new Vector2(380f, 200f));
            Label(poster.transform, twist, new Vector2(130f, -120f), new Vector2(380f, 200f));
        }

        private static Text Label(Transform parent, string text, Vector2 position, Vector2 size)
        {
            var label = UiKit.NewText(parent, "Text", text, UiKit.BodyNormal, UiPalette.Ink);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = UiKit.BodyNormal;
            var r = label.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = position;
            return label;
        }

        private static Image Sprite(Transform parent, string name, Sprite sprite, float artSize, Vector2 position)
        {
            var image = UiKit.NewImage(parent, name, sprite, sprite != null ? Color.white : UiPalette.Ink);
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            var r = image.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(artSize, artSize) * Scale;
            r.anchoredPosition = position;
            return image;
        }
    }
}
