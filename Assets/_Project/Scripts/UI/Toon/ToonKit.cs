using GraceRun.UI.Skin;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Toon
{
    /// <summary>
    /// Builders and stylers for the cartoon look: chunky candy buttons that bounce, posters, ribbons,
    /// badges. Every screen goes through here so the whole game feels like one cartoon. Without a
    /// <see cref="ToonTheme"/> asset everything falls back to the plain cinema UI.
    /// </summary>
    public static class ToonKit
    {
        /// <summary>UI pixels per art pixel of the toon sprites.</summary>
        public const float Scale = 2f;

        public static bool Available => ToonTheme.Current != null && ToonTheme.Current.Button(ToonTint.Cream) != null;

        /// <summary>Ink for light buttons, cream for the dark ones.</summary>
        public static Color LabelColor(ToonTint tint) =>
            tint is ToonTint.Plum or ToonTint.Stone or ToonTint.Sky ? UiPalette.Cream : UiPalette.Ink;

        /// <summary>
        /// Makes <paramref name="button"/> a bouncy candy button of <paramref name="tint"/>: sprite, label colour and lift,
        /// and the <see cref="ToonButton"/> motion with a rest tilt of <paramref name="tilt"/> degrees.
        /// </summary>
        public static void StyleButton(Selectable button, Text label, ToonTint tint, int size = UiKit.BodyNormal, float tilt = 0f, float sway = 0.8f, bool display = true)
        {
            var theme = ToonTheme.Current;
            var image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
                var sprite = theme != null ? theme.Button(tint) : null;
                UiKit.Slice(image, sprite, Scale);
            }
            button.transition = Selectable.Transition.None;
            if (label != null)
            {
                UiKit.StyleText(label, display, size, LabelColor(tint));
                UiKit.RemoveOutline(label);
                // The candy has a lip under the face: nudge the label up so it sits on the face.
                label.rectTransform.offsetMin = new Vector2(label.rectTransform.offsetMin.x, 10f);
                label.rectTransform.offsetMax = new Vector2(label.rectTransform.offsetMax.x, 10f);
            }
            var motion = button.GetComponent<ToonButton>();
            if (motion == null) motion = button.gameObject.AddComponent<ToonButton>();
            motion.Configure(tilt, sway);
        }

        public static Image NewPoster(Transform parent, string name, Sprite sprite = null)
        {
            var theme = ToonTheme.Current;
            return UiKit.NewImage(parent, name, sprite != null ? sprite : theme != null ? theme.Poster : null, Color.white, Scale);
        }

        /// <summary>A ribbon banner with a label, for headings and rarity.</summary>
        public static Image NewRibbon(Transform parent, string text, ToonTint tint, Vector2 size, int fontSize = UiKit.DisplaySmall)
        {
            var theme = ToonTheme.Current;
            var ribbon = UiKit.NewImage(parent, "Ribbon", theme != null ? theme.Ribbon(tint) : null, Color.white, Scale);
            ribbon.rectTransform.sizeDelta = size;
            var label = UiKit.NewText(ribbon.transform, "Label", text, fontSize, LabelColor(tint), TextAnchor.MiddleCenter, true);
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(24f, 2f);
            rect.offsetMax = new Vector2(-24f, 2f);
            return ribbon;
        }

        /// <summary>Adds chasing marquee bulbs around <paramref name="target"/> (call after its size is set).</summary>
        public static ToonBulbs AddBulbs(RectTransform target, float inset = 8f, float spacing = 38f)
        {
            var bulbs = target.gameObject.AddComponent<ToonBulbs>();
            bulbs.Configure(spacing, inset);
            return bulbs;
        }
    }
}
