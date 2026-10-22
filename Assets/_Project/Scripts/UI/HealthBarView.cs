using GraceRun.UI.Skin;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Makes a plain left-pivot fill bar read better: a pale chip that lingers where the HP just
    /// was, a "current / max" number, a colour that warms up at low HP, and a pulse when critical.
    /// <see cref="PlayerPanel"/> creates it on the HP bar and feeds it every frame.
    /// </summary>
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private float _chipHoldSeconds = 0.35f;
        [SerializeField, Tooltip("Chip drain, in bar widths per second.")]
        private float _chipDrainPerSecond = 0.6f;
        [SerializeField, Range(0f, 1f)] private float _warnBelow = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _criticalBelow = 0.25f;
        [SerializeField] private Color _lowColor = new Color32(232, 76, 61, 255);
        [SerializeField] private Color _chipColor = new Color32(255, 243, 214, 255);
        [SerializeField] private float _pulseSpeed = 9f;
        [SerializeField, Range(0f, 0.5f)] private float _pulseDepth = 0.22f;

        private Image _fillImage;
        private RectTransform _chip;
        private Text _label;
        private Color _fullColor;
        private float _chipValue = 1f;
        private float _hold;
        private float _previous = 1f;

        /// <summary>Wraps the bar whose <paramref name="fill"/> is scaled on x by the HP fraction.</summary>
        public static HealthBarView Attach(RectTransform fill, float labelBottomInset = 0f)
        {
            var view = fill.parent.gameObject.AddComponent<HealthBarView>();
            view.Build(fill, labelBottomInset);
            return view;
        }

        /// <summary>0..1 beat while the HP is below the critical mark (0 otherwise); lets the panel pulse with the bar.</summary>
        public float Pulse01 { get; private set; }

        /// <summary>Shows <paramref name="current"/> of <paramref name="max"/> HP.</summary>
        public void Show(int current, int max)
        {
            var hp = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            var dt = Time.unscaledDeltaTime;
            _hold = HealthBarRules.Hold(_hold, _previous, hp, _chipHoldSeconds, dt);
            _previous = hp;
            _chipValue = HealthBarRules.Chip(_chipValue, hp, _hold, _chipDrainPerSecond, dt);
            _chip.localScale = new Vector3(_chipValue, 1f, 1f);

            var color = HealthBarRules.FillColor(hp, _warnBelow, _criticalBelow, _fullColor, _lowColor);
            Pulse01 = hp > 0f && hp < _criticalBelow ? Mathf.Sin(Time.unscaledTime * _pulseSpeed) * 0.5f + 0.5f : 0f;
            color = Color.Lerp(color, Color.white, Pulse01 * _pulseDepth);
            _fillImage.color = color;
            _label.text = $"{current}/{max}";
        }

        private void Build(RectTransform fill, float labelBottomInset)
        {
            _fillImage = fill.GetComponent<Image>();
            _fullColor = _fillImage.color;

            var chip = Instantiate(fill.gameObject, fill.parent);
            chip.name = "Chip";
            chip.transform.SetSiblingIndex(fill.GetSiblingIndex());
            _chip = (RectTransform)chip.transform;
            var chipImage = chip.GetComponent<Image>();
            chipImage.color = _chipColor;
            chipImage.raycastTarget = false;

            var go = new GameObject("Value", typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(fill.parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, labelBottomInset);
            rect.offsetMax = Vector2.zero;
            _label = go.GetComponent<Text>();
            UiKit.StyleText(_label, false, 16, UiPalette.Cream);
            UiKit.InkOutline(_label, 1.5f);
            _label.alignment = TextAnchor.MiddleCenter;
            _label.raycastTarget = false;
            _label.resizeTextForBestFit = true;
            _label.resizeTextMinSize = 10;
            _label.resizeTextMaxSize = 16;
        }
    }
}
