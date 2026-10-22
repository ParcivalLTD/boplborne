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

        // Panel layout, UI px. The slim plate's border eats about 14 px at the top and 18 at the bottom.
        private const float PanelWidth = 340f;
        private const float PanelHeight = 86f;
        private const float Margin = 14f;
        private const float BadgeSize = 56f;
        private const float ColumnLeft = Margin + BadgeSize + 10f;
        private const float ColumnWidth = 132f;
        private const float SlotSize = 32f;
        private const float SlotGap = 4f;
        private const float BarHeight = 32f;
        private const float StaminaStrip = 6f;

        public static void PlayerPanel(Transform panel)
        {
            var theme = ToonTheme.Current;
            UiKit.Slice(panel.GetComponent<Image>(), LoadSprite("hud_plate"), S);
            panel.GetComponent<Image>().color = Color.white;
            var element = panel.GetComponent<LayoutElement>();
            if (element != null)
            {
                element.preferredWidth = element.minWidth = PanelWidth;
                element.preferredHeight = element.minHeight = PanelHeight;
            }
            ((RectTransform)panel).sizeDelta = new Vector2(PanelWidth, PanelHeight);

            var strip = Find(panel, "ColorStrip");
            var stripImage = strip.GetComponent<Image>();
            UiKit.Slice(stripImage, theme.Badge, S);
            stripImage.type = Image.Type.Simple;
            stripImage.preserveAspect = true;
            var stripRect = (RectTransform)strip;
            stripRect.anchorMin = stripRect.anchorMax = new Vector2(0f, 0.5f);
            stripRect.pivot = new Vector2(0f, 0.5f);
            stripRect.sizeDelta = new Vector2(BadgeSize, BadgeSize);
            stripRect.anchoredPosition = new Vector2(Margin, 2f);
            var hat = (RectTransform)Find(strip, "Hat");
            hat.anchorMin = hat.anchorMax = new Vector2(0.5f, 0.5f);
            // The hat art is only ~20 px wide inside a 48 x 64 body-sized frame, sitting above its middle:
            // blow the frame up and slide it down so the hat itself fills the badge.
            hat.pivot = new Vector2(0.5f, 0.5f);
            hat.sizeDelta = new Vector2(104f, 139f);
            hat.anchoredPosition = new Vector2(0f, -25f);

            var name = TextAt(panel, "Content/Name");
            Style(name, true, 18, UiPalette.Ink, 0f, true, 11);
            Place((RectTransform)Find(panel, "Content/Name"), new Vector2(ColumnLeft, -14f), new Vector2(72f, 20f));

            // One frame, two fills: the HP fill on top, the stamina strip fused under it.
            var hpBar = Bar(panel, "Content/HPBar", -38f, theme.BarFrame);
            var hpFill = (RectTransform)Find(hpBar, "Fill");
            hpFill.offsetMin = new Vector2(9f, StaminaStrip + 7f);
            hpFill.offsetMax = new Vector2(-9f, -5f);
            ImageAt(hpBar, "Fill").color = HpFill;
            var staminaBar = Bar(panel, "Content/StaminaBar", -38f, null);
            var staminaFill = (RectTransform)Find(staminaBar, "Fill");
            staminaFill.offsetMin = new Vector2(9f, 5f);
            staminaFill.offsetMax = new Vector2(-9f, -(BarHeight - StaminaStrip - 5f));
            ImageAt(staminaBar, "Fill").color = StaminaFill;

            // Flasks sit at the right end of the name row.
            Place((RectTransform)Find(panel, "Content/FlaskPips"), new Vector2(ColumnLeft + 76f, -15f), new Vector2(56f, 20f));
            var pip = ImageAt(panel, "Content/FlaskPips/PipTemplate");
            if (pip != null)
            {
                pip.sprite = LoadSprite("flask");
                pip.preserveAspect = true;
                pip.rectTransform.sizeDelta = new Vector2(17f, 20f);
            }
            var pips = Find(panel, "Content/FlaskPips").GetComponent<HorizontalLayoutGroup>();
            if (pips != null) pips.spacing = 3f;
            var view = panel.GetComponent<GraceRun.UI.PlayerPanel>();
            if (view != null)
            {
                var so = new SerializedObject(view);
                so.FindProperty("_flaskFull").colorValue = Color.white;
                so.FindProperty("_flaskEmpty").colorValue = new Color(0.3f, 0.26f, 0.32f, 0.45f);
                so.FindProperty("_hpLabelBottomInset").floatValue = StaminaStrip + 5f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var slots = (RectTransform)Find(panel, "Content/AbilitySlots");
            slots.anchorMin = slots.anchorMax = slots.pivot = new Vector2(1f, 1f);
            Place(slots, new Vector2(-Margin, -16f), new Vector2(SlotSize * 3f + SlotGap * 2f, SlotSize));
            var slotLayout = slots.GetComponent<HorizontalLayoutGroup>();
            if (slotLayout != null) slotLayout.spacing = SlotGap;
            for (var i = 1; i <= 3; i++)
            {
                var slot = Find(panel, $"Content/AbilitySlots/Slot{i}");
                ((RectTransform)slot).sizeDelta = new Vector2(SlotSize, SlotSize);
                var image = slot.GetComponent<Image>();
                UiKit.Slice(image, theme.Badge, S);
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
                var empty = Find(slot, "Empty");
                empty.GetComponent<Image>().color = new Color(0.8f, 0.72f, 0.6f, 0.0f);
                ((RectTransform)empty).offsetMin = new Vector2(4f, 4f);
                ((RectTransform)empty).offsetMax = new Vector2(-4f, -4f);
            }
            var down = (RectTransform)Find(panel, "DownLabel");
            down.anchoredPosition = new Vector2(ColumnLeft, 0f);
        }

        private static Sprite LoadSprite(string name) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/UI/Toon/{name}.png");

        /// <summary>Puts a bar in the column; <paramref name="frame"/> null hides its own frame (it shares another bar's).</summary>
        private static Transform Bar(Transform panel, string path, float y, Sprite frame)
        {
            var bar = Find(panel, path);
            var image = bar.GetComponent<Image>();
            if (frame != null)
            {
                UiKit.Slice(image, frame, S);
                image.color = Color.white;
            }
            image.enabled = frame != null;
            Place((RectTransform)bar, new Vector2(ColumnLeft, y), new Vector2(ColumnWidth, BarHeight));
            return bar;
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
