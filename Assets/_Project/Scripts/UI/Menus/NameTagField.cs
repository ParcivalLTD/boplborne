using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// The main menu's name field drawn as a "HELLO, MY NAME IS" sticker: an orange ribbon over a paper
    /// tag with the name written big on a line, tilted a little and swaying. Falls back to the plain
    /// <see cref="MenuKit.Field"/> when the toon theme is missing.
    /// </summary>
    public static class NameTagField
    {
        private const string Header = "HELLO! MY NAME IS";
        private const string Placeholder = "WRITE YOUR NAME";
        private const float TagHeight = 150f;
        private const float RibbonHeight = 52f;
        private const float Tilt = -1.2f;
        private const int NameSize = UiKit.DisplayMedium;

        public static InputField Build(Transform parent, string value, int characterLimit)
        {
            if (!ToonKit.Available) return MenuKit.Field(parent, value, characterLimit);

            var root = new GameObject("NameTag", typeof(RectTransform), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            var element = root.GetComponent<LayoutElement>();
            element.minHeight = element.preferredHeight = TagHeight;
            var rect = (RectTransform)root.transform;
            rect.localRotation = Quaternion.Euler(0f, 0f, Tilt);

            var body = ToonKit.NewPoster(root.transform, "Paper", ToonTheme.Current.PosterPaper);
            Stretch(body.rectTransform, Vector2.zero, Vector2.zero);
            body.raycastTarget = false;

            var ribbon = ToonKit.NewRibbon(root.transform, Header, ToonTint.Orange, new Vector2(-48f, RibbonHeight), UiKit.BodySmall);
            var ribbonRect = ribbon.rectTransform;
            ribbonRect.anchorMin = new Vector2(0f, 1f);
            ribbonRect.anchorMax = new Vector2(1f, 1f);
            ribbonRect.pivot = new Vector2(0.5f, 0.5f);
            ribbonRect.anchoredPosition = new Vector2(0f, -6f);
            ribbon.raycastTarget = false;

            var line = UiKit.NewImage(root.transform, "Line", null, UiPalette.Ink);
            line.raycastTarget = false;
            var lineRect = line.rectTransform;
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = new Vector2(1f, 0f);
            lineRect.pivot = new Vector2(0.5f, 0f);
            lineRect.sizeDelta = new Vector2(-64f, 4f);
            lineRect.anchoredPosition = new Vector2(0f, 22f);

            return BuildField(root.transform, value, characterLimit);
        }

        private static InputField BuildField(Transform parent, string value, int characterLimit)
        {
            var go = new GameObject("Field", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var area = go.GetComponent<Image>();
            area.color = Color.white;
            Stretch((RectTransform)go.transform, new Vector2(28f, 22f), new Vector2(-28f, -(RibbonHeight + 4f)));

            var field = go.GetComponent<InputField>();
            field.targetGraphic = area;
            field.transition = Selectable.Transition.ColorTint;
            field.colors = new ColorBlock
            {
                normalColor = new Color(1f, 1f, 1f, 0f),
                highlightedColor = new Color(1f, 0.92f, 0.55f, 0.35f),
                selectedColor = new Color(1f, 0.85f, 0.35f, 0.5f),
                pressedColor = new Color(1f, 0.8f, 0.3f, 0.6f),
                disabledColor = new Color(1f, 1f, 1f, 0f),
                colorMultiplier = 1f,
                fadeDuration = 0.06f,
            };

            var text = UiKit.NewText(go.transform, "Text", "", NameSize, UiPalette.Ink, TextAnchor.MiddleCenter, true);
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            Stretch(text.rectTransform, new Vector2(12f, 0f), new Vector2(-12f, 0f));
            var hint = UiKit.NewText(go.transform, "Placeholder", Placeholder, NameSize - 8, UiPalette.Sepia, TextAnchor.MiddleCenter, true);
            hint.horizontalOverflow = HorizontalWrapMode.Overflow;
            Stretch(hint.rectTransform, new Vector2(12f, 0f), new Vector2(-12f, 0f));

            field.textComponent = text;
            field.placeholder = hint;
            field.caretColor = UiPalette.Ink;
            field.selectionColor = new Color(UiPalette.Sepia.r, UiPalette.Sepia.g, UiPalette.Sepia.b, 0.45f);
            field.characterLimit = characterLimit;
            field.text = value;
            return field;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
