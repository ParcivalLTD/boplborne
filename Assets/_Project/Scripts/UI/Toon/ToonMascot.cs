using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Toon
{
    /// <summary>
    /// The blue blob knight on a menu: plays the idle frames with the plume hat on its head and now and
    /// then jumps with joy when poked. Frames come from <see cref="ToonTheme"/>. Cosmetic only.
    /// </summary>
    public class ToonMascot : MonoBehaviour
    {
        private const float Fps = 10f;
        private const float PixelScale = 6f;

        private Image _body;
        private Image _hat;
        private Spring _hop;
        private float _hopTarget;
        private float _baseY;
        private float _nextHop;

        public static ToonMascot Create(Transform parent, Vector2 anchoredPosition, float scale = PixelScale)
        {
            var theme = ToonTheme.Current;
            if (theme == null || theme.MascotFrames == null || theme.MascotFrames.Length == 0) return null;

            var go = new GameObject("Mascot", typeof(RectTransform), typeof(ToonMascot));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(64f * scale, 64f * scale);

            var mascot = go.GetComponent<ToonMascot>();
            mascot._body = NewImage(rect, "Body", theme.MascotFrames[0], new Vector2(64f * scale, 64f * scale));
            if (theme.MascotHat != null)
                mascot._hat = NewImage(rect, "Hat", theme.MascotHat, new Vector2(48f * scale, 64f * scale));
            mascot._baseY = anchoredPosition.y;
            mascot._nextHop = Time.unscaledTime + Random.Range(2f, 4f);
            return mascot;
        }

        private static Image NewImage(RectTransform parent, string name, Sprite sprite, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private void Update()
        {
            var theme = ToonTheme.Current;
            if (theme == null || _body == null) return;

            var frames = theme.MascotFrames;
            var index = Mathf.FloorToInt(Time.unscaledTime * Fps) % frames.Length;
            _body.sprite = frames[index];
            if (_hat != null)
            {
                var pixels = theme.MascotHatPixels;
                var offset = pixels != null && index < pixels.Length ? pixels[index] : Vector2.zero;
                ((RectTransform)_hat.transform).anchoredPosition = offset * PixelScale;
            }

            if (Time.unscaledTime >= _nextHop)
            {
                _hop.Velocity = 900f;
                _nextHop = Time.unscaledTime + Random.Range(3f, 6f);
            }
            _hop.Step(_hopTarget, Time.unscaledDeltaTime, 3f, 0.3f);
            if (_hop.Value < 0f) { _hop.Value = 0f; _hop.Velocity = Mathf.Max(0f, _hop.Velocity); }
            ((RectTransform)transform).anchoredPosition = new Vector2(((RectTransform)transform).anchoredPosition.x, _baseY + _hop.Value);
        }
    }
}
