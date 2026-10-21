using System;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// Builds the uGUI pieces every menu uses (canvas, panel, section, label, button, option row,
    /// text field) in code, in the shared <see cref="MenuStyle"/> and the cinema <see cref="UiTheme"/>:
    /// cream 9-slice panels with ink borders, pixel fonts, ink text. Buttons are keyboard- and
    /// gamepad-navigable: the selected one turns sepia.
    /// </summary>
    public static class MenuKit
    {
        public static Font Font => UiKit.BodyFont;

        /// <summary>A full-screen overlay canvas scaled for 1920×1080.</summary>
        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return canvas;
        }

        /// <summary>Makes sure the scene has an EventSystem driven by the Input System, so menus navigate.</summary>
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null || UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        /// <summary>A rect that fills its parent, tinted <paramref name="color"/>.</summary>
        public static RectTransform Fill(Transform parent, string name, Color color)
        {
            var rect = Rect(parent, name);
            Stretch(rect, Vector2.zero, Vector2.zero);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        /// <summary>A centred cream panel <paramref name="width"/> wide that grows to fit a vertical column of rows.</summary>
        public static RectTransform Column(Transform parent, string name, float width)
        {
            var rect = Rect(parent, name);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 0f);
            var image = rect.gameObject.AddComponent<Image>();
            UiKit.Slice(image, ToonKit.Available ? ToonTheme.Current.Poster : UiTheme.Current != null ? UiTheme.Current.PanelCream : null);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(MenuStyle.Padding, MenuStyle.Padding, MenuStyle.Padding, MenuStyle.Padding);
            layout.spacing = MenuStyle.Spacing;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
        }

        /// <summary>Like <see cref="Column"/> but without a panel behind it (buttons standing on the stage).</summary>
        public static RectTransform Stack(Transform parent, string name, float width, float spacing = 18f)
        {
            var rect = Column(parent, name, width);
            rect.GetComponent<Image>().enabled = false;
            var layout = rect.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = spacing;
            return rect;
        }

        /// <summary>A transparent row that lays its children out side by side, each taking an equal share (settings columns).</summary>
        public static RectTransform Columns(Transform parent, string name, float spacing)
        {
            var rect = Rect(parent, name);
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            return rect;
        }

        /// <summary>A headed block of rows without a panel of its own: a sepia section title above a vertical stack.</summary>
        public static RectTransform Section(Transform parent, string title)
        {
            var rect = Rect(parent, title);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = MenuStyle.Spacing;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            rect.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Label(rect, title.ToUpperInvariant(), UiKit.DisplaySmall, MenuStyle.Fog, TextAnchor.MiddleLeft, true);
            return rect;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, bool display = false)
        {
            var label = UiKit.NewText(parent, "Label", text, size, color, anchor, display || size > UiKit.BodyNormal);
            label.gameObject.AddComponent<LayoutElement>().minHeight = size * 1.3f;
            return label;
        }

        public static Button Button(Transform parent, string text, UnityAction onClick, ToonTint tint = ToonTint.Cream, float tilt = 0f, int size = UiKit.DisplaySmall, float height = 0f)
        {
            var button = Row<Button>(parent, text, out var label, tint, tilt, size, height);
            label.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(onClick);
            return button;
        }

        /// <summary>A settings row: <paramref name="text"/> on the left, the value with arrows on the right.</summary>
        public static MenuOptionRow Option(Transform parent, string text, Func<string> read, Action<int> adjust)
        {
            var row = Row<MenuOptionRow>(parent, text, out var label, ToonTint.Cream, 0f, UiKit.BodyNormal, 0f, false);
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.offsetMin = new Vector2(28f, label.rectTransform.offsetMin.y);
            label.rectTransform.offsetMax = new Vector2(-(LeftArrowFromRight + ArrowWidth + 16f), label.rectTransform.offsetMax.y);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = UiKit.BodySmall - 4;
            label.resizeTextMaxSize = UiKit.BodyNormal;
            var value = Label(row.transform, "", MenuStyle.BodySize, MenuStyle.Gold, TextAnchor.MiddleCenter);
            value.resizeTextForBestFit = true;
            value.resizeTextMinSize = UiKit.BodySmall - 8;
            value.resizeTextMaxSize = MenuStyle.BodySize;
            SideRect(value.rectTransform, ValueWidth, ValueFromRight);
            Arrow(row, -1, LeftArrowFromRight);
            Arrow(row, 1, RightArrowFromRight);
            row.Bind(value, read, adjust);
            return row;
        }

        private const float ArrowWidth = 68f;
        private const float ArrowHeight = 62f;
        private const float ValueWidth = 230f;
        private const float RightArrowFromRight = 20f;
        private const float ValueFromRight = RightArrowFromRight + ArrowWidth + 10f;
        private const float LeftArrowFromRight = ValueFromRight + ValueWidth + 10f;

        /// <summary>A candy arrow button at the right end of an option row; a click steps the value by <paramref name="delta"/>.</summary>
        private static void Arrow(MenuOptionRow row, int delta, float fromRight)
        {
            var rect = Rect(row.transform, delta < 0 ? "ArrowLeft" : "ArrowRight");
            var image = rect.gameObject.AddComponent<Image>();
            image.raycastTarget = true;
            if (ToonKit.Available) UiKit.Slice(image, ToonTheme.Current.Button(ToonTint.Orange), ToonKit.Scale);
            else UiKit.Slice(image, UiTheme.Current != null ? UiTheme.Current.Button : null);
            SideRect(rect, ArrowWidth, fromRight);
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(ArrowWidth, ArrowHeight);
            var glyph = Label(rect, delta < 0 ? "<" : ">", UiKit.DisplayMedium, UiPalette.Ink, TextAnchor.MiddleCenter, true);
            glyph.raycastTarget = false;
            glyph.GetComponent<LayoutElement>().ignoreLayout = true;
            Stretch(glyph.rectTransform, new Vector2(0f, 6f), Vector2.zero);
            rect.gameObject.AddComponent<OptionArrow>().Setup(row, delta);
        }

        /// <summary>Pins <paramref name="rect"/> to the right edge of its row: <paramref name="width"/> wide, <paramref name="fromRight"/> in from the edge.</summary>
        private static void SideRect(RectTransform rect, float width, float fromRight)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(width, 0f);
            rect.anchoredPosition = new Vector2(-fromRight, 0f);
            if (rect.TryGetComponent(out LayoutElement element)) element.ignoreLayout = true;
        }

        /// <summary>A single-line text field (the join address).</summary>
        public static InputField Field(Transform parent, string value, int characterLimit = 0, float height = 0f)
        {
            var rect = Rect(parent, "Field");
            rect.gameObject.AddComponent<LayoutElement>().minHeight = height > 0f ? height : MenuStyle.RowHeight;
            var image = rect.gameObject.AddComponent<Image>();
            UiKit.Slice(image, ToonKit.Available ? ToonTheme.Current.PosterPaper : UiTheme.Current != null ? UiTheme.Current.Field : null);
            var field = rect.gameObject.AddComponent<InputField>();
            field.targetGraphic = image;
            field.colors = UiKit.ButtonColors();
            var text = Label(rect, "", MenuStyle.BodySize, MenuStyle.Bone, TextAnchor.MiddleLeft);
            text.raycastTarget = true;
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            Stretch(text.rectTransform, new Vector2(20f, 0f), new Vector2(-20f, 0f));
            field.textComponent = text;
            field.caretColor = UiPalette.Ink;
            field.selectionColor = new Color(UiPalette.Sepia.r, UiPalette.Sepia.g, UiPalette.Sepia.b, 0.45f);
            field.characterLimit = characterLimit;
            field.text = value;
            return field;
        }

        /// <summary>Turns every selectable under <paramref name="root"/> into a plain vertical list (up/down only, wrapping).</summary>
        public static void ChainVertical(Transform root)
        {
            var items = root.GetComponentsInChildren<Selectable>(false);
            for (var i = 0; i < items.Length; i++)
            {
                var navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = items[(i + items.Length - 1) % items.Length],
                    selectOnDown = items[(i + 1) % items.Length],
                };
                items[i].navigation = navigation;
            }
        }

        private static T Row<T>(Transform parent, string text, out Text label, ToonTint tint = ToonTint.Cream, float tilt = 0f, int size = UiKit.DisplaySmall, float height = 0f, bool display = true) where T : Selectable
        {
            var rect = Rect(parent, text);
            rect.gameObject.AddComponent<LayoutElement>().minHeight = height > 0f ? height : MenuStyle.RowHeight;
            var image = rect.gameObject.AddComponent<Image>();
            var selectable = rect.gameObject.AddComponent<T>();
            selectable.targetGraphic = image;
            label = Label(rect, text, MenuStyle.BodySize, MenuStyle.Bone);
            Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
            if (ToonKit.Available)
            {
                if (display) label.text = text.ToUpperInvariant();
                ToonKit.StyleButton(selectable, label, tint, size, tilt, 0.8f, display);
            }
            else UiKit.StyleButton(selectable, label, MenuStyle.BodySize);
            return selectable;
        }

        private static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            if (rect.TryGetComponent(out LayoutElement element)) element.ignoreLayout = true;
        }
    }
}
