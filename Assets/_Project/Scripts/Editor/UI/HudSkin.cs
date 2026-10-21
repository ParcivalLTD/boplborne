using GraceRun.UI;
using GraceRun.UI.Skin;
using UnityEngine;
using UnityEngine.UI;
using static GraceRun.Editor.UI.SkinHelpers;

namespace GraceRun.Editor.UI
{
    /// <summary>Restyles the HUD in the cinema look: the four player panels (and the PlayerPanel prefab) and the boss bar.</summary>
    internal static class HudSkin
    {
        private const float PanelHeight = 150f;
        private const float BarsLeft = 66f;
        private const float BarsWidth = 208f;

        public static void PlayerPanel(Transform panel, UiTheme theme)
        {
            Slice(panel.GetComponent<Image>(), theme.PanelCream, new Color(1f, 1f, 1f, 0.97f));
            var element = panel.GetComponent<LayoutElement>();
            if (element != null) element.preferredHeight = element.minHeight = PanelHeight;
            var rect = (RectTransform)panel;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, PanelHeight);

            var strip = Find(panel, "ColorStrip");
            Slice(strip.GetComponent<Image>(), theme.Slot, Color.white);
            Inset((RectTransform)strip, 16f, 16f, 0f, 16f);
            var stripRect = (RectTransform)strip;
            stripRect.anchorMin = new Vector2(0f, 0f);
            stripRect.anchorMax = new Vector2(0f, 1f);
            stripRect.offsetMin = new Vector2(16f, 16f);
            stripRect.offsetMax = new Vector2(60f, -16f);
            var hat = Find(strip, "Hat");
            Place((RectTransform)hat, new Vector2(0f, -8f), new Vector2(34f, 34f));
            hat.GetComponent<Image>().color = Color.white;

            Style(TextAt(panel, "Content/Name"), false, UiKit.BodyNormal, UiPalette.Ink, 0f, true, 16);
            Place((RectTransform)Find(panel, "Content/Name"), new Vector2(BarsLeft, -12f), new Vector2(BarsWidth, 34f));

            Bar(panel, "Content/HPBar", -48f, 28f, theme);
            Bar(panel, "Content/StaminaBar", -80f, 18f, theme);
            Place((RectTransform)Find(panel, "Content/FlaskPips"), new Vector2(BarsLeft, -104f), new Vector2(BarsWidth, 26f));
            var pip = ImageAt(panel, "Content/FlaskPips/PipTemplate");
            if (pip != null)
            {
                pip.sprite = theme.FlaskPip;
                pip.preserveAspect = true;
                pip.rectTransform.sizeDelta = new Vector2(20f, 24f);
            }
            var pips = Find(panel, "Content/FlaskPips").GetComponent<HorizontalLayoutGroup>();
            if (pips != null) pips.spacing = 6f;

            var slots = (RectTransform)Find(panel, "Content/AbilitySlots");
            Place(slots, new Vector2(-16f, 0f), new Vector2(150f, 46f));
            for (var i = 1; i <= 3; i++) Slot(panel, $"Content/AbilitySlots/Slot{i}", theme);

            var down = TextAt(panel, "DownLabel");
            Style(down, true, UiKit.DisplayMedium, UiPalette.Brick);
            var downRect = (RectTransform)Find(panel, "DownLabel");
            downRect.anchorMin = new Vector2(0f, 0f);
            downRect.anchorMax = new Vector2(0f, 1f);
            downRect.pivot = new Vector2(0f, 0.5f);
            downRect.anchoredPosition = new Vector2(BarsLeft, 0f);
            downRect.sizeDelta = new Vector2(BarsWidth, 0f);

            var view = panel.GetComponent<GraceRun.UI.PlayerPanel>();
            if (view != null)
            {
                var so = new UnityEditor.SerializedObject(view);
                so.FindProperty("_flaskFull").colorValue = Color.white;
                so.FindProperty("_flaskEmpty").colorValue = new Color(0.55f, 0.47f, 0.40f, 0.85f);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void Bar(Transform panel, string path, float y, float height, UiTheme theme)
        {
            var bar = Find(panel, path);
            Slice(bar.GetComponent<Image>(), theme.BarFrame, Color.white);
            Place((RectTransform)bar, new Vector2(BarsLeft, y), new Vector2(BarsWidth, height));
            Inset((RectTransform)Find(bar, "Fill"), 6f, 6f, 6f, 6f);
            if (path.EndsWith("HPBar")) ImageAt(bar, "Fill").color = UiPalette.Cream;
        }

        private static void Slot(Transform panel, string path, UiTheme theme)
        {
            var slot = Find(panel, path);
            Slice(slot.GetComponent<Image>(), theme.Slot, Color.white);
            var empty = Find(slot, "Empty");
            empty.GetComponent<Image>().color = UiPalette.Paper;
            Inset((RectTransform)empty, 8f, 8f, 8f, 8f);
        }

        public static void BossBar(Transform canvas, UiTheme theme)
        {
            var panel = Find(canvas, "Panel");
            var rect = (RectTransform)panel;
            var image = panel.GetComponent<Image>();
            if (image == null) image = panel.gameObject.AddComponent<Image>();
            Slice(image, theme.PanelInk, Color.white);
            image.raycastTarget = false;
            rect.sizeDelta = new Vector2(840f, 116f);
            rect.anchoredPosition = new Vector2(0f, -20f);

            var name = TextAt(canvas, "Panel/Name");
            Style(name, true, UiKit.DisplaySmall, UiPalette.Cream, 0f, true, 24);
            var nameRect = (RectTransform)name.transform;
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, -16f);
            nameRect.sizeDelta = new Vector2(-48f, 44f);

            var back = Find(canvas, "Panel/Back");
            Slice(back.GetComponent<Image>(), theme.BarFrame, Color.white);
            var backRect = (RectTransform)back;
            backRect.anchorMin = new Vector2(0f, 0f);
            backRect.anchorMax = new Vector2(1f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 16f);
            backRect.sizeDelta = new Vector2(-48f, 36f);
            Inset((RectTransform)Find(back, "Fill"), 6f, 6f, 6f, 6f);
            ImageAt(back, "Fill").color = UiPalette.Cream;
            var tick = ImageAt(canvas, "Panel/Back/Phase2Tick");
            if (tick != null) tick.color = UiPalette.Brass;
        }
    }
}
