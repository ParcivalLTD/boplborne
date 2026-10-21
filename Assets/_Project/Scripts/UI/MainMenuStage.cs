using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// The main menu's set dressing: a dark theatre stage with burgundy curtains and a scalloped
    /// valance, a warm spotlight, boards on the floor, a marquee sign (the title ringed by chasing
    /// bulbs, an orange ribbon under it), floating stars and the blue blob mascot hopping at the side.
    /// Built in code from <see cref="ToonTheme"/>; without the theme it is a plain dark backdrop. Cosmetic only.
    /// </summary>
    public static class MainMenuStage
    {
        private static readonly Color StageColor = new Color32(36, 20, 42, 255);
        private static readonly Color FloorColor = new Color32(104, 70, 48, 255);

        /// <summary>Builds the stage and returns the header (marquee and ribbon) so a sub-page can hide it for room.</summary>
        public static GameObject Build(Transform root, string title, string ribbon, string version)
        {
            var theme = ToonTheme.Current;
            Fill(root, "Stage", StageColor);
            if (theme == null || !ToonKit.Available) return null;

            Spotlight(root, theme);
            Curtains(root, theme);
            Floor(root);
            var header = Marquee(root, title, ribbon, version);
            Stars(root, theme);
            ToonMascot.Create(root, new Vector2(-640f, 96f), 6f);
            return header;
        }

        private static void Spotlight(Transform root, ToonTheme theme)
        {
            var image = NewImage(root, "Spotlight", theme.Spotlight, new Color(1f, 0.93f, 0.75f, 1f));
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(1500f, 1080f);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void Curtains(Transform root, ToonTheme theme)
        {
            foreach (var left in new[] { true, false })
            {
                var image = NewImage(root, left ? "CurtainLeft" : "CurtainRight", theme.Curtain, Color.white);
                image.type = Image.Type.Tiled;
                image.pixelsPerUnitMultiplier = 1f / 5f;
                var rect = image.rectTransform;
                rect.anchorMin = new Vector2(left ? 0f : 1f, 0f);
                rect.anchorMax = new Vector2(left ? 0f : 1f, 1f);
                rect.pivot = new Vector2(left ? 0f : 1f, 0.5f);
                rect.sizeDelta = new Vector2(330f, 0f);
                rect.anchoredPosition = Vector2.zero;
            }
            var valance = NewImage(root, "Valance", theme.Valance, Color.white);
            valance.type = Image.Type.Tiled;
            valance.pixelsPerUnitMultiplier = 1f / 5f;
            var vr = valance.rectTransform;
            vr.anchorMin = new Vector2(0f, 1f);
            vr.anchorMax = new Vector2(1f, 1f);
            vr.pivot = new Vector2(0.5f, 1f);
            vr.sizeDelta = new Vector2(0f, 120f);
            vr.anchoredPosition = Vector2.zero;
        }

        private static void Floor(Transform root)
        {
            var floor = Fill(root, "Floor", FloorColor);
            var rect = floor.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, 96f);
            var edge = Fill(floor.transform, "Edge", UiPalette.Ink);
            var er = edge.rectTransform;
            er.anchorMin = new Vector2(0f, 1f);
            er.anchorMax = new Vector2(1f, 1f);
            er.pivot = new Vector2(0.5f, 1f);
            er.sizeDelta = new Vector2(0f, 8f);
            er.anchoredPosition = Vector2.zero;
        }

        private static GameObject Marquee(Transform root, string title, string ribbon, string version)
        {
            var theme = ToonTheme.Current;
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(root, false);
            var headerRect = (RectTransform)headerGo.transform;
            headerRect.anchorMin = Vector2.zero;
            headerRect.anchorMax = Vector2.one;
            headerRect.offsetMin = headerRect.offsetMax = Vector2.zero;
            var panel = ToonKit.NewPoster(headerRect, "Marquee", theme.PosterPlum);
            var rect = panel.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(1180f, 250f);
            rect.anchoredPosition = new Vector2(0f, -100f);
            ToonKit.AddBulbs(rect, 22f, 40f);

            var text = UiKit.NewText(rect, "Title", title, UiKit.DisplayBig, UiPalette.Cream, TextAnchor.MiddleCenter, true);
            UiKit.InkOutline(text, 6f);
            var tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(30f, 40f);
            tr.offsetMax = new Vector2(-30f, -10f);
            var motion = text.gameObject.AddComponent<ToonFloat>();
            motion.Configure(4f, 1.2f, 1.1f);

            var banner = ToonKit.NewRibbon(headerRect, ribbon, ToonTint.Orange, new Vector2(900f, 78f));
            var br = banner.rectTransform;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
            br.pivot = new Vector2(0.5f, 1f);
            br.anchoredPosition = new Vector2(0f, -312f);
            banner.gameObject.AddComponent<ToonPopIn>().Configure(0.25f, 0.5f);

            var v = UiKit.NewText(root, "Version", version, UiKit.BodySmall, UiPalette.Cream, TextAnchor.LowerRight);
            var vr = v.rectTransform;
            vr.anchorMin = vr.anchorMax = new Vector2(1f, 0f);
            vr.pivot = new Vector2(1f, 0f);
            vr.sizeDelta = new Vector2(500f, 40f);
            vr.anchoredPosition = new Vector2(-24f, 12f);
            return headerGo;
        }

        private static void Stars(Transform root, ToonTheme theme)
        {
            var spots = new[] { new Vector2(-620f, 330f), new Vector2(640f, 300f), new Vector2(-520f, 60f), new Vector2(560f, 20f), new Vector2(700f, 400f) };
            for (var i = 0; i < spots.Length; i++)
            {
                var image = NewImage(root, "Star", i % 2 == 0 ? theme.Star : theme.StarOrange, Color.white);
                var rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                var size = 56f + i * 8f;
                rect.sizeDelta = new Vector2(size, size);
                rect.anchoredPosition = spots[i];
                image.gameObject.AddComponent<ToonFloat>().Configure(10f, 8f, 1.3f + i * 0.2f);
            }
        }

        private static Image NewImage(Transform parent, string name, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Image Fill(Transform parent, string name, Color color)
        {
            var image = NewImage(parent, name, null, color);
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return image;
        }
    }
}
