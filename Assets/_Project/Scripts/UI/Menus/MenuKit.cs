using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// Builds the uGUI pieces every menu uses (canvas, panel, column, label, button, option row,
    /// text field) in code, in the shared <see cref="MenuStyle"/>. Buttons are keyboard- and
    /// gamepad-navigable: the selected one turns gold.
    /// </summary>
    public static class MenuKit
    {
        private static Font _font;

        public static Font Font
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

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
            scaler.matchWidthOrHeight = 0.5f;
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
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        /// <summary>A centred panel <paramref name="width"/> wide that grows to fit a vertical column of rows.</summary>
        public static RectTransform Column(Transform parent, string name, float width)
        {
            var rect = Rect(parent, name);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 0f);
            rect.gameObject.AddComponent<Image>().color = MenuStyle.Panel;
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(MenuStyle.Padding, MenuStyle.Padding, MenuStyle.Padding, MenuStyle.Padding);
            layout.spacing = MenuStyle.Spacing;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var rect = Rect(parent, "Label");
            var label = rect.gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            rect.gameObject.AddComponent<LayoutElement>().minHeight = size * 1.4f;
            return label;
        }

        public static Button Button(Transform parent, string text, UnityAction onClick)
        {
            var button = Row<Button>(parent, text, out var label);
            label.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(onClick);
            return button;
        }

        /// <summary>A settings row: <paramref name="text"/> on the left, the value with arrows on the right.</summary>
        public static MenuOptionRow Option(Transform parent, string text, Func<string> read, Action<int> adjust)
        {
            var row = Row<MenuOptionRow>(parent, text, out var label);
            label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.offsetMin = new Vector2(24f, 0f);
            var value = Label(row.transform, "", MenuStyle.BodySize, MenuStyle.Gold, TextAnchor.MiddleCenter);
            SideRect(value.rectTransform, ValueWidth, ArrowWidth + 8f);
            Arrow(row, "<", -1, ValueWidth + ArrowWidth + 8f);
            Arrow(row, ">", 1, 8f);
            row.Bind(value, read, adjust);
            return row;
        }

        private const float ArrowWidth = 32f;
        private const float ValueWidth = 170f;

        /// <summary>A clickable arrow at the right end of an option row; a click steps the value by <paramref name="delta"/>.</summary>
        private static void Arrow(MenuOptionRow row, string glyph, int delta, float fromRight)
        {
            var text = Label(row.transform, glyph, MenuStyle.BodySize + 6, MenuStyle.Bone, TextAnchor.MiddleCenter);
            SideRect(text.rectTransform, ArrowWidth, fromRight);
            text.raycastTarget = true;
            text.gameObject.AddComponent<OptionArrow>().Setup(row, delta);
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
        public static InputField Field(Transform parent, string value, int characterLimit = 0)
        {
            var rect = Rect(parent, "Field");
            rect.gameObject.AddComponent<LayoutElement>().minHeight = MenuStyle.RowHeight;
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var field = rect.gameObject.AddComponent<InputField>();
            field.targetGraphic = image;
            field.colors = Colors();
            var text = Label(rect, "", MenuStyle.BodySize, MenuStyle.Bone);
            text.raycastTarget = true;
            text.supportRichText = false;
            Stretch(text.rectTransform, new Vector2(16f, 0f), new Vector2(-16f, 0f));
            field.textComponent = text;
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

        private static T Row<T>(Transform parent, string text, out Text label) where T : Selectable
        {
            var rect = Rect(parent, text);
            rect.gameObject.AddComponent<LayoutElement>().minHeight = MenuStyle.RowHeight;
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            var selectable = rect.gameObject.AddComponent<T>();
            selectable.targetGraphic = image;
            selectable.colors = Colors();
            label = Label(rect, text, MenuStyle.BodySize, MenuStyle.Bone);
            Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
            return selectable;
        }

        private static ColorBlock Colors() => new()
        {
            normalColor = MenuStyle.Stone,
            highlightedColor = MenuStyle.StoneLight,
            selectedColor = Color.Lerp(MenuStyle.StoneLight, MenuStyle.Gold, 0.45f),
            pressedColor = MenuStyle.Gold,
            disabledColor = new Color(MenuStyle.Stone.r, MenuStyle.Stone.g, MenuStyle.Stone.b, 0.4f),
            colorMultiplier = 1f,
            fadeDuration = 0.08f,
        };

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
