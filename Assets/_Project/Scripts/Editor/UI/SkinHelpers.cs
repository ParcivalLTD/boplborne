using GraceRun.UI.Skin;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.Editor.UI
{
    /// <summary>Small edit-time helpers the UI skin builders share: find a child by path, restyle text and images, place rects.</summary>
    internal static class SkinHelpers
    {
        public static Transform Find(Transform root, string path)
        {
            var found = root.Find(path);
            if (found == null) Debug.LogWarning($"UI skin: '{path}' not found under {root.name}.");
            return found;
        }

        public static Text TextAt(Transform root, string path)
        {
            var t = Find(root, path);
            return t != null ? t.GetComponent<Text>() : null;
        }

        public static Image ImageAt(Transform root, string path)
        {
            var t = Find(root, path);
            return t != null ? t.GetComponent<Image>() : null;
        }

        /// <summary>Pixel font, size and colour; drops any Outline unless <paramref name="outline"/> is above 0 (then an ink one).</summary>
        public static void Style(Text text, bool display, int size, Color color, float outline = 0f, bool bestFit = false, int minSize = 16)
        {
            if (text == null) return;
            UiKit.StyleText(text, display, size, color);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextForBestFit = bestFit;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = size;
            if (outline > 0f) UiKit.InkOutline(text, outline);
            else
            {
                var existing = text.GetComponent<Outline>();
                if (existing != null) Object.DestroyImmediate(existing);
            }
        }

        public static void Slice(Image image, Sprite sprite, Color color)
        {
            if (image == null) return;
            image.color = color;
            UiKit.Slice(image, sprite);
        }

        public static void Place(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            if (rect == null) return;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        public static void Inset(RectTransform rect, float left, float bottom, float right, float top)
        {
            if (rect == null) return;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>A cream button: 9-slice sprite, cinema colour states, ink label.</summary>
        public static void Button(Transform root, string path, int labelSize = UiKit.BodyNormal, bool bestFit = true)
        {
            var t = Find(root, path);
            if (t == null) return;
            var button = t.GetComponent<Button>();
            var label = t.GetComponentInChildren<Text>(true);
            UiKit.StyleButton(button, label, labelSize);
            if (label != null) Style(label, false, labelSize, UiPalette.Ink, 0f, bestFit);
        }
    }
}
