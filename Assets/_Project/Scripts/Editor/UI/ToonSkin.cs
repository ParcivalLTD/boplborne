using GraceRun.UI;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GraceRun.Editor.UI.SkinHelpers;

namespace GraceRun.Editor.UI
{
    /// <summary>
    /// The cartoon restyle of the Game scene's overlays and the PlayerPanel prefab (docs/ART_PIPELINE.md step 6):
    /// bouncy candy buttons, posters with ink shadows, ribbons, badges, marquee bulbs. It runs after the cinema
    /// skin in Grace Run → Build UI and again in Build Toon UI; every step is idempotent.
    /// </summary>
    internal static class ToonSkin
    {
        private const float S = ToonKit.Scale;

        // ---------- helpers ----------

        private static void Candy(Transform root, string path, ToonTint tint, int size, float tilt, Vector2? place = null, Vector2? dims = null)
        {
            var node = Find(root, path);
            if (node == null) return;
            var rect = (RectTransform)node;
            if (place.HasValue && dims.HasValue) Place(rect, place.Value, dims.Value);
            else if (dims.HasValue) rect.sizeDelta = dims.Value;
            var selectable = node.GetComponent<Selectable>();
            var label = node.GetComponentInChildren<Text>(true);
            ToonKit.StyleButton(selectable, label, tint, size, tilt);
            rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }

        private static T Ensure<T>(GameObject go) where T : Component => go.GetComponent<T>() != null ? go.GetComponent<T>() : go.AddComponent<T>();

        private static void ClearChild(Transform parent, string name)
        {
            var old = parent.Find(name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        /// <summary>A stretched empty child that carries the marquee bulbs (rebuilt at run time).</summary>
        private static void Bulbs(RectTransform target, float inset, float spacing)
        {
            ClearChild(target, "Bulbs");
            var go = new GameObject("Bulbs", typeof(RectTransform));
            go.transform.SetParent(target, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var bulbs = go.AddComponent<ToonBulbs>();
            bulbs.Configure(spacing, inset);
        }

        private static Image Ribbon(Transform parent, string name, ToonTint tint, Vector2 size, Vector2 position, Transform behind)
        {
            ClearChild(parent, name);
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(behind.GetSiblingIndex());
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            UiKit.Slice(image, ToonTheme.Current.Ribbon(tint), S);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return image;
        }

        // ---------- fight overlay ----------

        public static void FightScreen(Transform canvas)
        {
            var theme = ToonTheme.Current;
            var title = TextAt(canvas, "Root/Title");
            Style(title, true, 128, UiPalette.Cream, 8f);
            Ensure<ToonFloat>(title.gameObject).Configure(6f, 1.5f, 1.2f);
            var sub = TextAt(canvas, "Root/Subtitle");
            Style(sub, false, UiKit.BodyNormal, UiPalette.Cream, 3f);

            var button = Find(canvas, "Root/Button");
            // The start button lives in the top-left corner with the room code under it, clear of the centred title.
            var buttonRect = (RectTransform)button;
            buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = new Vector2(0f, 1f);
            Candy(canvas, "Root/Button", ToonTint.Orange, UiKit.DisplayMedium, -1.5f, new Vector2(48f, -48f), new Vector2(660f, 140f));
            RoomCodeCard(canvas);
            var buttonLabel = TextAt(canvas, "Root/Button/Label");
            if (buttonLabel != null)
            {
                buttonLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
                buttonLabel.resizeTextForBestFit = true;
                buttonLabel.resizeTextMinSize = UiKit.DisplaySmall;
                buttonLabel.resizeTextMaxSize = UiKit.DisplayMedium;
            }
            Bulbs((RectTransform)button, 14f, 36f);
            Ensure<ToonPopIn>(button.gameObject).Configure(0.1f, 0.55f);
            _ = theme;
        }

        /// <summary>The paper tag under the hub's start button that shows the room code and whether the room is public.</summary>
        private static void RoomCodeCard(Transform canvas)
        {
            var root = Find(canvas, "Root");
            ClearChild(root, "RoomCode");
            var card = ToonKit.NewPoster(root, "RoomCode", ToonTheme.Current.PosterPaper);
            card.raycastTarget = false;
            var rect = card.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(620f, 190f);
            rect.anchoredPosition = new Vector2(68f, -206f);
            rect.localRotation = Quaternion.Euler(0f, 0f, 1.2f);
            Ensure<ToonFloat>(card.gameObject).Configure(2f, 0.5f, 0.8f);

            RoomCodeText(rect, "Heading", "ROOM CODE", UiKit.BodySmall, UiPalette.Sepia, new Vector2(0f, 0.72f), new Vector2(1f, 0.98f));
            var code = RoomCodeText(rect, "Code", "------", 80, UiPalette.Ink, new Vector2(0f, 0.28f), new Vector2(1f, 0.74f));
            var mode = RoomCodeText(rect, "Mode", "PRIVATE ROOM", UiKit.BodySmall, UiPalette.Sepia, new Vector2(0f, 0.02f), new Vector2(1f, 0.3f));

            var screen = canvas.GetComponent<GraceRun.UI.FightScreen>();
            if (screen == null) return;
            var so = new SerializedObject(screen);
            so.FindProperty("_roomCode").objectReferenceValue = card.gameObject;
            so.FindProperty("_roomCodeText").objectReferenceValue = code;
            so.FindProperty("_roomModeText").objectReferenceValue = mode;
            so.ApplyModifiedPropertiesWithoutUndo();
            card.gameObject.SetActive(false);
        }

        private static Text RoomCodeText(RectTransform parent, string name, string content, int size, Color color, Vector2 min, Vector2 max)
        {
            var text = UiKit.NewText(parent, name, content, size, color, TextAnchor.MiddleCenter, true);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            var rect = text.rectTransform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return text;
        }

        // ---------- draft ----------

        private static readonly float[] CardTilt = { -2.5f, 1.2f, 2.8f };

        public static void DraftScreen(Transform canvas)
        {
            var theme = ToonTheme.Current;
            Slice(ImageAt(canvas, "Root"), null, new Color(0.10f, 0.06f, 0.14f, 0.82f));

            // Title on an orange ribbon.
            var title = TextAt(canvas, "Root/Title");
            Style(title, true, UiKit.DisplayLarge, UiPalette.Ink);
            var titleRect = (RectTransform)title.transform;
            titleRect.sizeDelta = new Vector2(900f, 90f);
            titleRect.anchoredPosition = new Vector2(0f, -66f);
            var root = Find(canvas, "Root");
            var ribbon = Ribbon(root, "TitleRibbon", ToonTint.Orange, new Vector2(980f, 116f), new Vector2(0f, -72f), titleRect);
            Ensure<ToonFloat>(ribbon.gameObject).Configure(3f, 0.8f, 1f);
            Style(TextAt(canvas, "Root/Hint"), false, UiKit.BodyNormal, UiPalette.Cream, 3f);
            ((RectTransform)Find(canvas, "Root/Hint")).anchoredPosition = new Vector2(0f, -150f);

            for (var i = 1; i <= 4; i++)
            {
                var chip = $"Root/Chips/Chip{i}";
                UiKit.Slice(ImageAt(canvas, chip), theme.Button(ToonTint.Cream), S);
                ImageAt(canvas, chip).color = Color.white;
                Style(TextAt(canvas, chip + "/Label"), false, UiKit.BodySmall, UiPalette.Ink, 0f, true, 16);
                var selectable = Find(canvas, chip).GetComponent<Selectable>();
                if (selectable != null) selectable.transition = Selectable.Transition.None;
            }

            for (var i = 1; i <= 3; i++) Card(canvas, $"Root/Cards/Card{i}", i - 1);
            Candy(canvas, "Root/Reroll", ToonTint.Pink, UiKit.DisplaySmall, 1.5f, new Vector2(0f, -392f), new Vector2(380f, 96f));

            UiKit.Slice(ImageAt(canvas, "Root/ReplacePanel/Box"), theme.Poster, S);
            ImageAt(canvas, "Root/ReplacePanel/Box").color = Color.white;
            Style(TextAt(canvas, "Root/ReplacePanel/Box/Title"), true, UiKit.DisplaySmall, UiPalette.Ink, 0f, true, 24);
            for (var i = 1; i <= 3; i++) Candy(canvas, $"Root/ReplacePanel/Box/Slot{i}", ToonTint.Sky, UiKit.DisplaySmall, i % 2 == 0 ? 1f : -1f);
            Candy(canvas, "Root/ReplacePanel/Box/Cancel", ToonTint.Stone, UiKit.DisplaySmall, 0f);

            var screen = canvas.GetComponent<GraceRun.UI.DraftScreen>();
            if (screen == null) return;
            var so = new SerializedObject(screen);
            so.FindProperty("_commonFrame").colorValue = Color.white;
            so.FindProperty("_rareFrame").colorValue = new Color(0.72f, 0.80f, 1f);
            so.FindProperty("_legendaryFrame").colorValue = new Color(1f, 0.82f, 0.42f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Card(Transform canvas, string path, int index)
        {
            var theme = ToonTheme.Current;
            var card = Find(canvas, path);
            var cardImage = card.GetComponent<Image>();
            UiKit.Slice(cardImage, theme.PosterPaper, S);
            cardImage.color = Color.white;

            ImageAt(card, "Inner").enabled = false;
            var back = ImageAt(card, "Inner/IconBack");
            UiKit.Slice(back, theme.Badge, S);
            back.type = Image.Type.Simple;
            back.preserveAspect = true;
            back.color = Color.white;
            Place(back.rectTransform, new Vector2(0f, -84f), new Vector2(144f, 144f));
            var icon = ImageAt(card, "Inner/IconBack/Icon");
            icon.color = Color.white;
            icon.preserveAspect = true;
            icon.rectTransform.offsetMin = new Vector2(24f, 24f);
            icon.rectTransform.offsetMax = new Vector2(-24f, -24f);

            Style(TextAt(card, "Inner/Name"), true, UiKit.DisplaySmall, UiPalette.Ink, 0f, true, 24);
            Place((RectTransform)Find(card, "Inner/Name"), new Vector2(0f, -178f), new Vector2(370f, 50f));
            Style(TextAt(card, "Inner/Rarity"), true, UiKit.BodySmall, UiPalette.Sepia);
            Style(TextAt(card, "Inner/Tags"), false, UiKit.BodySmall, UiPalette.SepiaDark, 0f, true, 16);
            Style(TextAt(card, "Inner/Cooldown"), false, UiKit.BodySmall, UiPalette.Sepia);
            Style(TextAt(card, "Inner/Description"), false, UiKit.BodySmall, UiPalette.Ink, 0f, true, 16);
            Slice(ImageAt(card, "Inner/Divider"), null, UiPalette.Sepia);
            Style(TextAt(card, "Inner/Combo"), false, UiKit.BodySmall, UiPalette.Sepia, 0f, true, 16);

            // The rarity ribbon (tinted by DraftCardView) sits behind the rarity word.
            var inner = Find(card, "Inner");
            var rarity = Find(card, "Inner/Rarity");
            var ribbon = Ribbon(inner, "RarityRibbon", ToonTint.Cream, new Vector2(240f, 40f), new Vector2(0f, -218f), rarity);
            var view = card.GetComponent<DraftCardView>();
            if (view != null)
            {
                var so = new SerializedObject(view);
                so.FindProperty("_frame").objectReferenceValue = ribbon;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // PICKED stamp: an orange ribbon across the bottom of the card.
            var picked = Find(card, "PickedMark");
            UiKit.Slice(picked.GetComponent<Image>(), theme.Ribbon(ToonTint.Orange), S);
            picked.GetComponent<Image>().color = Color.white;
            Place((RectTransform)picked, new Vector2(0f, 14f), new Vector2(300f, 64f));
            Style(TextAt(card, "PickedMark/Label"), true, UiKit.DisplaySmall, UiPalette.Ink);
            TextAt(card, "PickedMark/Label").text = "PICKED!";

            var tilt = CardTilt[index % CardTilt.Length];
            var selectable = card.GetComponent<Selectable>();
            if (selectable != null) selectable.transition = Selectable.Transition.None;
            var motion = Ensure<ToonButton>(card.gameObject);
            motion.Configure(tilt, 0.6f, 1.05f);
            card.localRotation = Quaternion.Euler(0f, 0f, tilt);
            Ensure<ToonPopIn>(card.gameObject).Configure(0.12f * index, 0.5f);
        }

        // ---------- HUD ----------

        private static readonly Color HpFill = new Color32(60, 203, 127, 255);
        private static readonly Color StaminaFill = new Color32(255, 154, 60, 255);

        public static void PlayerPanel(Transform panel)
        {
            var theme = ToonTheme.Current;
            UiKit.Slice(panel.GetComponent<Image>(), theme.Poster, S);
            panel.GetComponent<Image>().color = Color.white;

            var strip = Find(panel, "ColorStrip");
            var stripImage = strip.GetComponent<Image>();
            UiKit.Slice(stripImage, theme.Badge, S);
            stripImage.type = Image.Type.Simple;
            stripImage.preserveAspect = true;
            var stripRect = (RectTransform)strip;
            stripRect.anchorMin = stripRect.anchorMax = new Vector2(0f, 0.5f);
            stripRect.pivot = new Vector2(0f, 0.5f);
            stripRect.sizeDelta = new Vector2(72f, 72f);
            stripRect.anchoredPosition = new Vector2(20f, 8f);
            var hat = (RectTransform)Find(strip, "Hat");
            hat.anchorMin = hat.anchorMax = new Vector2(0.5f, 0.5f);
            hat.sizeDelta = new Vector2(52f, 52f);
            hat.anchoredPosition = Vector2.zero;

            Style(TextAt(panel, "Content/Name"), true, UiKit.DisplaySmall, UiPalette.Ink, 0f, true, 16);
            Place((RectTransform)Find(panel, "Content/Name"), new Vector2(110f, -10f), new Vector2(150f, 34f));
            Bar(panel, "Content/HPBar", -50f, 28f, HpFill);
            Bar(panel, "Content/StaminaBar", -82f, 18f, StaminaFill);
            Place((RectTransform)Find(panel, "Content/FlaskPips"), new Vector2(110f, -106f), new Vector2(150f, 26f));

            var slots = (RectTransform)Find(panel, "Content/AbilitySlots");
            Place(slots, new Vector2(-18f, 0f), new Vector2(156f, 48f));
            for (var i = 1; i <= 3; i++)
            {
                var slot = Find(panel, $"Content/AbilitySlots/Slot{i}");
                var image = slot.GetComponent<Image>();
                UiKit.Slice(image, theme.Badge, S);
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
                var empty = Find(slot, "Empty");
                empty.GetComponent<Image>().color = new Color(0.8f, 0.72f, 0.6f, 0.0f);
                ((RectTransform)empty).offsetMin = new Vector2(7f, 7f);
                ((RectTransform)empty).offsetMax = new Vector2(-7f, -7f);
            }
            var down = (RectTransform)Find(panel, "DownLabel");
            down.anchoredPosition = new Vector2(110f, 0f);
        }

        private static void Bar(Transform panel, string path, float y, float height, Color fill)
        {
            var bar = Find(panel, path);
            UiKit.Slice(bar.GetComponent<Image>(), ToonTheme.Current.BarFrame, S);
            bar.GetComponent<Image>().color = Color.white;
            Place((RectTransform)bar, new Vector2(110f, y), new Vector2(150f, height));
            var fillRect = (RectTransform)Find(bar, "Fill");
            fillRect.offsetMin = new Vector2(7f, 5f);
            fillRect.offsetMax = new Vector2(-7f, -5f);
            ImageAt(bar, "Fill").color = fill;
        }

        public static void BossBar(Transform canvas)
        {
            var theme = ToonTheme.Current;
            var panel = Find(canvas, "Panel");
            UiKit.Slice(panel.GetComponent<Image>(), theme.PosterPlum, S);
            panel.GetComponent<Image>().color = Color.white;
            var name = TextAt(canvas, "Panel/Name");
            Style(name, true, UiKit.DisplaySmall, UiPalette.Cream, 3f, true, 24);
            var back = Find(canvas, "Panel/Back");
            UiKit.Slice(back.GetComponent<Image>(), theme.BarFrame, S);
            back.GetComponent<Image>().color = Color.white;
            var fill = (RectTransform)Find(back, "Fill");
            fill.offsetMin = new Vector2(7f, 6f);
            fill.offsetMax = new Vector2(-7f, -6f);
            ImageAt(back, "Fill").color = new Color32(255, 154, 60, 255);
        }
    }
}
