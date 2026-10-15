using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Turns a set of sprites solid white, either for a moment (<see cref="Flash"/>) or for as
    /// long as something holds it (<see cref="SetHeld"/>). Local cosmetic only.
    /// Placeholder sprites are white shapes, so tinting the renderer color is enough for now;
    /// real art (M8) needs a flash shader driven by a material property block.
    /// </summary>
    public class SpriteFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] _renderers;
        [SerializeField] private Color _flashColor = Color.white;

        private Color[] _originalColors;
        private bool _held;
        private float _timeLeft;
        private bool _showing;

        private void Awake()
        {
            _originalColors = new Color[_renderers.Length];
        }

        private void Update()
        {
            if (_timeLeft <= 0f) return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) Refresh();
        }

        private void OnDisable()
        {
            _held = false;
            _timeLeft = 0f;
            Refresh();
        }

        /// <summary>Shows the flash for <paramref name="duration"/> seconds.</summary>
        public void Flash(float duration)
        {
            if (!isActiveAndEnabled) return;
            _timeLeft = Mathf.Max(_timeLeft, duration);
            Refresh();
        }

        /// <summary>Keeps the flash on until called again with false.</summary>
        public void SetHeld(bool held)
        {
            if (!isActiveAndEnabled) return;
            _held = held;
            Refresh();
        }

        private void Refresh()
        {
            var show = _held || _timeLeft > 0f;
            if (show == _showing) return;
            _showing = show;

            for (var i = 0; i < _renderers.Length; i++)
            {
                if (show)
                {
                    _originalColors[i] = _renderers[i].color;
                    _renderers[i].color = _flashColor;
                }
                else
                {
                    _renderers[i].color = _originalColors[i];
                }
            }
        }
    }
}
