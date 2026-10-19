using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Draws the slice trail with a world-space <see cref="LineRenderer"/> and fades it out.
    /// Cosmetic only: the owner draws its stroke live; other PCs flash the finished stroke.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SliceView : MonoBehaviour
    {
        [SerializeField] private SliceData _data;

        private LineRenderer _line;
        private Color _baseStart;
        private Color _baseEnd;
        private float _fadeLeft;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.widthMultiplier = _data.TrailWidth;
            _line.positionCount = 0;
            _baseStart = _line.startColor;
            _baseEnd = _line.endColor;
        }

        private void Update()
        {
            if (_fadeLeft <= 0f) return;
            _fadeLeft -= Time.deltaTime;
            SetAlpha(Mathf.Clamp01(_fadeLeft / _data.TrailFadeTime));
            if (_fadeLeft <= 0f) _line.positionCount = 0;
        }

        /// <summary>Shows <paramref name="points"/> at full strength (the owner's live stroke).</summary>
        public void Draw(IReadOnlyList<Vector2> points)
        {
            var count = Mathf.Min(points.Count, _data.MaxTrailPoints);
            _line.positionCount = count;
            for (var i = 0; i < count; i++) _line.SetPosition(i, points[i]);
            _fadeLeft = 0f;
            SetAlpha(1f);
        }

        /// <summary>Starts fading whatever is drawn.</summary>
        public void FadeOut() => _fadeLeft = _data.TrailFadeTime;

        /// <summary>Draws a finished stroke and fades it at once (other players' slashes).</summary>
        public void Flash(IReadOnlyList<Vector2> points)
        {
            Draw(points);
            FadeOut();
        }

        private void SetAlpha(float alpha)
        {
            _line.startColor = new Color(_baseStart.r, _baseStart.g, _baseStart.b, _baseStart.a * alpha);
            _line.endColor = new Color(_baseEnd.r, _baseEnd.g, _baseEnd.b, _baseEnd.a * alpha);
        }
    }
}
