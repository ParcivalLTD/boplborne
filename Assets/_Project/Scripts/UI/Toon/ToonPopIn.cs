using UnityEngine;

namespace GraceRun.UI.Toon
{
    /// <summary>
    /// Pops an element in with a bouncy overshoot when it appears (after an optional delay). If the element also
    /// has a <see cref="ToonButton"/> the button reads <see cref="Factor"/> instead of this scaling itself. Cosmetic.
    /// </summary>
    public class ToonPopIn : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _delay;
        [SerializeField, Min(0.05f)] private float _duration = 0.45f;

        private Vector3 _scale = Vector3.one;
        private float _start;
        private bool _captured;
        private bool _drives;

        /// <summary>1 once the pop is over; before that the bouncy 0 to about 1.15 factor.</summary>
        public float Factor { get; private set; } = 1f;

        public void Configure(float delay, float duration = 0.45f)
        {
            _delay = delay;
            _duration = duration;
        }

        private void OnEnable()
        {
            _drives = true;
            if (TryGetComponent(out ToonButton button))
            {
                button.SetPop(this);
                _drives = false;
            }
            if (_drives && !_captured)
            {
                _scale = transform.localScale;
                _captured = true;
            }
            _start = Time.unscaledTime + _delay;
            Factor = 0f;
            if (_drives) transform.localScale = Vector3.zero;
        }

        private void OnDisable()
        {
            Factor = 1f;
            if (_drives && _captured) transform.localScale = _scale;
        }

        private void Update()
        {
            var t = (Time.unscaledTime - _start) / _duration;
            Factor = t <= 0f ? 0f : ToonMotion.PopCurve(t);
            if (_drives) transform.localScale = _scale * Factor;
            if (t >= 1f)
            {
                Factor = 1f;
                if (_drives) transform.localScale = _scale;
                enabled = false;
            }
        }
    }
}
