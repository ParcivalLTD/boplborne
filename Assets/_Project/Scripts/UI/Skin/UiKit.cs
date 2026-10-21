using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Skin
{
    /// <summary>
    /// Small helpers every screen uses to build or restyle widgets in the cinema look: pixel fonts,
    /// 9-slice panels drawn at 2x (one art pixel = two UI pixels at the 1920×1080 reference), ink-outlined text
    /// and the button colour states. Works without a <see cref="UiTheme"/> asset (plain UI).
    /// </summary>
    public static class UiKit
    {
        /// <summary>UI pixels per art pixel of the 9-slice sprites.</summary>
        public const float PixelScale = 2f;

        public const int DisplayBig = 96;
        public const int DisplayLarge = 64;
        public const int DisplayMedium = 48;
        public const int DisplaySmall = 32;
        public const int BodyNormal = 32;
        public const int BodySmall = 24;

        private static Font _fallback;

        private static Font Fallback
        {
            get
            {
                if (_fallback == null) _fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _fallback;
            }
        }

        public static Font BodyFont => UiTheme.Current != null && UiTheme.Current.Body != null ? UiTheme.Current.Body : Fallback;

        public static Font DisplayFont => UiTheme.Current != null && UiTheme.Current.Display != null ? UiTheme.Current.Display : Fallback;

        /// <summary>The 9-slice sprite for a content card: the cartoon poster, or the cream cinema panel without the toon theme.</summary>
        public static Sprite CardPanel(bool paper = false)
        {
            if (Toon.ToonKit.Available) return paper ? Toon.ToonTheme.Current.PosterPaper : Toon.ToonTheme.Current.Poster;
            var theme = UiTheme.Current;
            return theme == null ? null : paper ? theme.PanelPaper : theme.PanelCream;
        }

        /// <summary>Gives <paramref name="image"/> a 9-slice <paramref name="sprite"/> drawn at <paramref name="scale"/>x; a null sprite leaves a flat colour.</summary>
        public static void Slice(Image image, Sprite sprite, float scale = PixelScale)
        {
            image.sprite = sprite;
            if (sprite == null) return;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f / scale;
        }

        public static Image NewImage(Transform parent, string name, Sprite sprite, Color color, float scale = PixelScale)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            Slice(image, sprite, scale);
            return image;
        }

        /// <summary>Applies a pixel font, size and colour to <paramref name="text"/> (the one way every screen styles text).</summary>
        public static void StyleText(Text text, bool display, int size, Color color)
        {
            text.font = display ? DisplayFont : BodyFont;
            text.fontSize = size;
            text.fontStyle = FontStyle.Normal;
            text.color = color;
            text.lineSpacing = 1f;
        }

        public static Text NewText(Transform parent, string name, string content, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter, bool display = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            StyleText(text, display, size, color);
            text.text = content;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Replaces any Outline on <paramref name="graphic"/> with an ink one, for text drawn straight over the game (no panel behind it).</summary>
        public static void InkOutline(Graphic graphic, float distance)
        {
            var outline = graphic.GetComponent<Outline>();
            if (outline == null) outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = UiPalette.Ink;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        public static void RemoveOutline(Graphic graphic)
        {
            var outline = graphic.GetComponent<Outline>();
            if (outline == null) return;
            if (Application.isPlaying) Object.Destroy(outline);
            else Object.DestroyImmediate(outline);
        }

        /// <summary>The colour states of a cream button: normal, hover, keyboard / gamepad selection (sepia), pressed.</summary>
        public static ColorBlock ButtonColors() => new()
        {
            normalColor = Color.white,
            highlightedColor = new Color(0.93f, 0.86f, 0.70f, 1f),
            selectedColor = new Color(0.86f, 0.72f, 0.50f, 1f),
            pressedColor = new Color(0.68f, 0.55f, 0.38f, 1f),
            disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f),
            colorMultiplier = 1f,
            fadeDuration = 0.06f,
        };

        /// <summary>Makes an existing button a cream 9-slice button with the cinema colour states and ink label.</summary>
        public static void StyleButton(Selectable button, Text label, int size = BodyNormal, Toon.ToonTint tint = Toon.ToonTint.Cream, float tilt = 0f)
        {
            if (Toon.ToonKit.Available)
            {
                Toon.ToonKit.StyleButton(button, label, tint, size, tilt, 0.4f, size >= DisplaySmall);
                return;
            }
            var theme = UiTheme.Current;
            if (button.targetGraphic == null) button.targetGraphic = button.GetComponent<Image>();
            var image = button.targetGraphic as Image;
            if (image != null)
            {
                image.color = Color.white;
                Slice(image, theme != null ? theme.Button : null);
            }
            button.colors = ButtonColors();
            if (label == null) return;
            StyleText(label, false, size, UiPalette.Ink);
            RemoveOutline(label);
        }
    }
}
