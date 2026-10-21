using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Splash
{
    /// <summary>The game's logo, the Bopped Bell (<c>Resources/Brand/BellLogo.png</c>), for the splash and loading screens. Missing file = no logo, nothing breaks.</summary>
    public static class BrandKit
    {
        private const string LogoPath = "Brand/BellLogo";

        private static Sprite _logo;

        public static Sprite Logo
        {
            get
            {
                if (_logo == null) _logo = Resources.Load<Sprite>(LogoPath);
                return _logo;
            }
        }

        /// <summary>Adds the bell tile to <paramref name="parent"/> at <paramref name="anchoredPosition"/> (centre anchored); returns null without the art.</summary>
        public static Image AddLogo(Transform parent, Vector2 anchoredPosition, float size)
        {
            var sprite = Logo;
            if (sprite == null) return null;
            var go = new GameObject("Logo", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = anchoredPosition;
            return image;
        }
    }
}
