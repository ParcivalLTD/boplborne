using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GraceRun.UI.Toon
{
    /// <summary>
    /// Rubber-hose button motion: it swells and tilts toward the pointer (or the keyboard / gamepad
    /// selection), squashes when pressed and springs back with an overshoot, and sways a little while
    /// idle, held "on twos" like a drawn cartoon. Works on any <see cref="Selectable"/>; uses
    /// unscaled time so menus move while the game is paused. Purely cosmetic.
    /// </summary>
    [DisallowMultipleComponent]
    public class ToonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField, Min(1f)] private float _hoverScale = 1.07f;
        [SerializeField, Tooltip("Degrees the button tilts toward the cursor when hovered or selected.")]
        private float _hoverTilt = 2.5f;
        [SerializeField, Range(0f, 0.3f)] private float _pressSquash = 0.12f;
        [SerializeField, Tooltip("Degrees of idle sway (0 = still).")]
        private float _idleSway = 0.8f;
        [SerializeField] private float _restTilt;

        // Once the mouse has touched any button, a leftover keyboard / gamepad selection (the draft opens with
        // card 1 selected) stops lifting its button, so only the hovered one reacts. Arrow keys / stick bring it back.
        private static bool s_pointerDriven;

        private Spring _scaleX = new(1f);
        private Spring _scaleY = new(1f);
        private Spring _tilt;
        private Vector3 _baseScale = Vector3.one;
        private float _phase;
        private bool _over;
        private bool _selected;
        private bool _pressed;
        private Selectable _selectable;
        private ToonPopIn _pop;

        /// <summary>The pop-in that scales this button while it appears (set by <see cref="ToonPopIn"/>).</summary>
        public void SetPop(ToonPopIn pop) => _pop = pop;

        public float RestTilt
        {
            get => _restTilt;
            set => _restTilt = value;
        }

        public void Configure(float restTilt, float idleSway, float hoverScale = 1.07f)
        {
            _restTilt = restTilt;
            _idleSway = idleSway;
            _hoverScale = hoverScale;
        }

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            _baseScale = transform.localScale;
            _phase = Random.value * 6.28f;
            _tilt = new Spring(_restTilt);
        }

        private void OnEnable()
        {
            _over = _selected = _pressed = false;
            _scaleX = new Spring(1f);
            _scaleY = new Spring(1f);
            _tilt = new Spring(_restTilt);
            Apply();
        }

        private void Update()
        {
            var interactable = _selectable == null || _selectable.IsInteractable();
            var lifted = interactable && (_over || (_selected && !s_pointerDriven));
            var targetX = lifted ? _hoverScale : 1f;
            var targetY = targetX;
            if (_pressed && interactable)
            {
                targetX = _hoverScale + _pressSquash;
                targetY = _hoverScale - _pressSquash;
            }

            var sway = lifted ? 0f : Mathf.Sin(ToonMotion.Stepped(Time.unscaledTime, 12f) * 2.1f + _phase) * _idleSway;
            var targetTilt = lifted ? (_restTilt > 0f ? -_hoverTilt : _hoverTilt) : _restTilt + sway;

            var dt = Time.unscaledDeltaTime;
            _scaleX.Step(targetX, dt);
            _scaleY.Step(targetY, dt);
            _tilt.Step(targetTilt, dt, 8f, 0.45f);
            Apply();
        }

        private void Apply()
        {
            var pop = _pop != null ? _pop.Factor : 1f;
            transform.localScale = new Vector3(_baseScale.x * _scaleX.Value * pop, _baseScale.y * _scaleY.Value * pop, _baseScale.z);
            transform.localRotation = Quaternion.Euler(0f, 0f, _tilt.Value);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _over = true;
            s_pointerDriven = true;
        }

        public void OnPointerExit(PointerEventData eventData) { _over = false; _pressed = false; }
        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;

        public void OnSelect(BaseEventData eventData)
        {
            _selected = true;
            if (eventData is AxisEventData) s_pointerDriven = false;
        }

        public void OnDeselect(BaseEventData eventData) => _selected = false;
    }
}
