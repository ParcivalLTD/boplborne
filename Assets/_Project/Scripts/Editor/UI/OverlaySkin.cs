using GraceRun.UI;
using GraceRun.UI.Skin;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GraceRun.Editor.UI.SkinHelpers;

namespace GraceRun.Editor.UI
{
    /// <summary>Restyles the fight overlay (hub prompt, countdown, victory / defeat) and the draft screen in the cinema look.</summary>
    internal static class OverlaySkin
    {
        private static readonly Color CommonFrame = Color.white;
        private static readonly Color RareFrame = new(0.80f, 0.74f, 1f);
        private static readonly Color LegendaryFrame = new(1f, 0.84f, 0.45f);

        public static void FightScreen(Transform canvas, UiTheme theme)
        {
            var title = TextAt(canvas, "Root/Title");
            Style(title, true, 128, UiPalette.Cream, 4f);
            Place((RectTransform)title.transform, new Vector2(0f, -30f), new Vector2(1600f, 220f));
            var sub = TextAt(canvas, "Root/Subtitle");
            Style(sub, false, UiKit.BodyNormal, UiPalette.Cream, 2f);
            Place((RectTransform)sub.transform, new Vector2(0f, -150f), new Vector2(1600f, 60f));
            var button = Find(canvas, "Root/Button");
            Place((RectTransform)button, new Vector2(0f, -270f), new Vector2(520f, 96f));
            Button(canvas, "Root/Button");
        }

        public static void DraftScreen(Transform canvas, UiTheme theme)
        {
            Slice(ImageAt(canvas, "Root"), null, UiPalette.Dim);
            Style(TextAt(canvas, "Root/Title"), true, 64, UiPalette.Cream);
            Style(TextAt(canvas, "Root/Hint"), false, UiKit.BodyNormal, UiPalette.Paper);
            for (var i = 1; i <= 4; i++)
            {
                var chip = $"Root/Chips/Chip{i}";
                Slice(ImageAt(canvas, chip), theme.Button, Color.white);
                Style(TextAt(canvas, chip + "/Label"), false, UiKit.BodySmall, UiPalette.Ink, 0f, true, 16);
            }
            for (var i = 1; i <= 3; i++) Card(canvas, $"Root/Cards/Card{i}", theme);
            Button(canvas, "Root/Reroll");

            Slice(ImageAt(canvas, "Root/ReplacePanel"), null, UiPalette.Dim);
            Slice(ImageAt(canvas, "Root/ReplacePanel/Box"), theme.PanelCream, Color.white);
            Style(TextAt(canvas, "Root/ReplacePanel/Box/Title"), true, UiKit.DisplaySmall, UiPalette.Ink, 0f, true, 24);
            for (var i = 1; i <= 3; i++) Button(canvas, $"Root/ReplacePanel/Box/Slot{i}");
            Button(canvas, "Root/ReplacePanel/Box/Cancel");

            var screen = canvas.GetComponent<GraceRun.UI.DraftScreen>();
            if (screen == null) return;
            var so = new SerializedObject(screen);
            so.FindProperty("_commonFrame").colorValue = CommonFrame;
            so.FindProperty("_rareFrame").colorValue = RareFrame;
            so.FindProperty("_legendaryFrame").colorValue = LegendaryFrame;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Card(Transform canvas, string path, UiTheme theme)
        {
            var card = Find(canvas, path);
            Slice(card.GetComponent<Image>(), theme.CardFrame, Color.white);
            var inner = ImageAt(card, "Inner");
            inner.enabled = false;
            Slice(ImageAt(card, "Inner/IconBack"), theme.Slot, Color.white);
            ImageAt(card, "Inner/IconBack/Icon").color = Color.white;
            Style(TextAt(card, "Inner/Name"), true, UiKit.DisplaySmall, UiPalette.Ink, 0f, true, 24);
            Style(TextAt(card, "Inner/Rarity"), false, UiKit.BodySmall, UiPalette.Sepia);
            Style(TextAt(card, "Inner/Tags"), false, UiKit.BodySmall, UiPalette.SepiaDark, 0f, true, 16);
            Style(TextAt(card, "Inner/Cooldown"), false, UiKit.BodySmall, UiPalette.Sepia);
            Style(TextAt(card, "Inner/Description"), false, UiKit.BodySmall, UiPalette.Ink, 0f, true, 16);
            Place((RectTransform)Find(card, "Inner/Description"), new Vector2(0f, -356f), new Vector2(340f, 112f));
            Slice(ImageAt(card, "Inner/Divider"), null, UiPalette.Sepia);
            Style(TextAt(card, "Inner/Combo"), false, UiKit.BodySmall, UiPalette.Sepia, 0f, true, 16);
            Place((RectTransform)Find(card, "Inner/Combo"), new Vector2(0f, -470f), new Vector2(340f, 96f));
            Slice(ImageAt(card, "PickedMark"), theme.Button, Color.white);
            Style(TextAt(card, "PickedMark/Label"), false, UiKit.BodyNormal, UiPalette.Ink);
        }
    }
}
