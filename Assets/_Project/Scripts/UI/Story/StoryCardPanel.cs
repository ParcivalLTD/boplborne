using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// The look of one story card: a plum marquee poster with a heading, a framed picture and an orange
    /// ribbon caption (the title-card look). A missing picture shows a flat panel instead. Cosmetic, no input.
    /// </summary>
    public class StoryCardPanel
    {
        private const float CardWidth = 1100f;
        private const float CardHeight = 860f;
        private const float PictureWidth = 972f;
        private const float PictureHeight = 552f;

        private readonly bool _shout = ToonKit.Available;
        private CanvasGroup _group;
        private RectTransform _card;
        private Text _title;
        private Text _caption;
        private Image _picture;

        /// <summary>The card's root object, shown and hidden by its owner.</summary>
        public GameObject Root => _group.gameObject;

        /// <summary>Builds the card under <paramref name="parent"/> (hidden until <see cref="Set"/> and <see cref="Pose"/>).</summary>
        public void Build(Transform parent)
        {
            var theme = ToonTheme.Current;
            var holder = new GameObject("StoryCard", typeof(RectTransform), typeof(CanvasGroup));
            holder.transform.SetParent(parent, false);
            _group = holder.GetComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;
            var holderRect = (RectTransform)holder.transform;
            holderRect.anchorMin = holderRect.anchorMax = holderRect.pivot = new Vector2(0.5f, 0.5f);
            holderRect.sizeDelta = new Vector2(CardWidth, CardHeight);

            var card = UiKit.NewImage(holder.transform, "Card", ToonKit.Available ? theme.PosterPlum : UiKit.CardPanel(), Color.white, ToonKit.Scale);
            if (!ToonKit.Available) card.color = UiPalette.Cream;
            card.raycastTarget = false;
            _card = card.rectTransform;
            _card.anchorMin = _card.anchorMax = _card.pivot = new Vector2(0.5f, 0.5f);
            _card.sizeDelta = new Vector2(CardWidth, CardHeight);

            _title = UiKit.NewText(_card, "Title", "", UiKit.DisplayMedium, ToonKit.Available ? UiPalette.Cream : UiPalette.Ink, TextAnchor.MiddleCenter, true);
            Place(_title.rectTransform, new Vector2(CardWidth - 120f, 90f), new Vector2(0f, 360f));
            if (ToonKit.Available) UiKit.InkOutline(_title, 4f);

            var frame = UiKit.NewImage(_card, "PictureFrame", null, UiPalette.Ink);
            frame.raycastTarget = false;
            Place(frame.rectTransform, new Vector2(PictureWidth + 16f, PictureHeight + 16f), new Vector2(0f, 20f));
            _picture = UiKit.NewImage(frame.transform, "Picture", null, UiPalette.Stone);
            _picture.raycastTarget = false;
            _picture.preserveAspect = true;
            Place(_picture.rectTransform, new Vector2(PictureWidth, PictureHeight), Vector2.zero);

            if (ToonKit.Available)
            {
                ToonKit.AddBulbs(_card, 16f, 40f);
                var ribbon = ToonKit.NewRibbon(_card, "", ToonTint.Orange, new Vector2(1000f, 104f), UiKit.DisplaySmall);
                Place(ribbon.rectTransform, new Vector2(1000f, 104f), new Vector2(0f, -352f));
                _caption = ribbon.GetComponentInChildren<Text>();
                _caption.resizeTextForBestFit = true;
                _caption.resizeTextMinSize = 18;
                _caption.resizeTextMaxSize = UiKit.DisplaySmall;
            }
            else
            {
                _caption = UiKit.NewText(_card, "Caption", "", UiKit.BodyNormal, UiPalette.Sepia);
                Place(_caption.rectTransform, new Vector2(CardWidth - 120f, 104f), new Vector2(0f, -352f));
            }
            holder.SetActive(false);
        }

        /// <summary>Fills the card. A null picture shows the flat placeholder.</summary>
        public void Set(StoryCard card)
        {
            _title.text = Shout(card.Title);
            _caption.text = Shout(card.Caption);
            _picture.sprite = card.Picture;
            _picture.color = card.Picture != null ? Color.white : UiPalette.Stone;
        }

        /// <summary>Slides and tilts the card for the animation values from <c>TitleCardRules.Progress</c>.</summary>
        public void Pose(float enter, float exit)
        {
            _group.alpha = Mathf.Clamp01(enter * 2f) * (1f - exit);
            var zoom = Mathf.Lerp(1.4f, 1f, enter);
            _card.localScale = new Vector3(zoom, zoom, 1f) * (1f - 0.1f * exit);
            _card.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-4f, 0f, enter) + 3f * exit);
            _card.anchoredPosition = new Vector2(Mathf.Lerp(-260f, 0f, enter) + 420f * exit, 0f);
        }

        private string Shout(string text) => _shout && text != null ? text.ToUpperInvariant() : text;

        private static void Place(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
