using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Toon
{
    /// <summary>A ring of marquee bulbs around a rect that chase each other, like a theatre sign. Cosmetic.</summary>
    public class ToonBulbs : MonoBehaviour
    {
        [SerializeField, Min(8f)] private float _spacing = 34f;
        [SerializeField] private float _inset = 0f;
        [SerializeField, Min(1f)] private float _fps = 5f;
        [SerializeField, Min(2)] private int _period = 3;

        private readonly List<Image> _bulbs = new();
        private int _frame = -1;

        /// <summary>Sets the ring's look; the bulbs are created when it starts (or by <see cref="Build"/>).</summary>
        public void Configure(float spacing, float inset)
        {
            _spacing = spacing;
            _inset = inset;
        }

        private void Start()
        {
            if (_bulbs.Count == 0) Build();
        }

        public void Build(float scale = 2f)
        {
            var theme = ToonTheme.Current;
            if (theme == null || theme.BulbOn == null) return;

            var rect = (RectTransform)transform;
            var size = rect.rect.size;
            if (size.x < 10f) size = rect.sizeDelta;
            var half = size * 0.5f - new Vector2(_inset, _inset);
            var perimeter = 4f * (half.x + half.y);
            var count = Mathf.Max(8, Mathf.RoundToInt(perimeter / _spacing));
            var side = 18f * scale * 0.5f;
            for (var i = 0; i < count; i++)
            {
                var go = new GameObject("Bulb", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var image = go.GetComponent<Image>();
                image.sprite = theme.BulbOn;
                image.raycastTarget = false;
                image.preserveAspect = true;
                var bulb = (RectTransform)go.transform;
                bulb.sizeDelta = new Vector2(side * 2f, side * 2f);
                bulb.anchorMin = bulb.anchorMax = new Vector2(0.5f, 0.5f);
                bulb.anchoredPosition = PointOnRect(half, i / (float)count);
                _bulbs.Add(image);
            }
        }

        private void Update()
        {
            if (_bulbs.Count == 0) return;
            var frame = Mathf.FloorToInt(Time.unscaledTime * _fps);
            if (frame == _frame) return;
            _frame = frame;
            var theme = ToonTheme.Current;
            for (var i = 0; i < _bulbs.Count; i++)
                _bulbs[i].sprite = (i + frame) % _period == 0 ? theme.BulbOff : theme.BulbOn;
        }

        /// <summary>A point on the perimeter of a rect centred on the origin, <paramref name="t"/> 0-1 once around, starting top-left.</summary>
        private static Vector2 PointOnRect(Vector2 half, float t)
        {
            var w = half.x * 2f;
            var h = half.y * 2f;
            var d = t * 2f * (w + h);
            if (d < w) return new Vector2(-half.x + d, half.y);
            d -= w;
            if (d < h) return new Vector2(half.x, half.y - d);
            d -= h;
            if (d < w) return new Vector2(half.x - d, -half.y);
            d -= w;
            return new Vector2(-half.x, -half.y + d);
        }
    }
}
