using GraceRun.UI.Skin;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// One ability slot in a <see cref="PlayerPanel"/>: an icon (hidden while the slot is empty)
    /// and a dark cooldown shade that drains from the top as the cooldown runs out.
    /// <see cref="PlayerPanel"/> feeds it from the player's ability loadout.
    /// </summary>
    public class AbilitySlotView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField, Tooltip("Filled image (vertical, origin bottom) over the icon.")]
        private Image _cooldownShade;

        private Text _levelBadge;
        private Text _seconds;
        private Text _key;
        private bool _wasCooling;
        private float _popStart = float.NegativeInfinity;

        private float _lastSeconds;

        /// <summary>A cooldown that falls by more than this between two updates was shaved by a weapon hit.</summary>
        private const float ShavePopSeconds = 0.2f;
        private const float PopSeconds = 0.3f;
        private const float PopSize = 0.22f;

        private void Awake() => Show(null, 0f, 1);

        /// <summary>
        /// Shows <paramref name="icon"/> (null = empty slot) with 0..1 cooldown left, the seconds left,
        /// a level badge and a key hint under the slot (empty hint = none). The slot pops when its cooldown ends.
        /// </summary>
        public void Show(Sprite icon, float cooldownRemaining01, int level = 1, float cooldownSeconds = 0f, string keyHint = null)
        {
            EnsureLevelBadge();
            _icon.enabled = icon != null;
            _icon.sprite = icon;
            var shade = icon != null ? Mathf.Clamp01(cooldownRemaining01) : 0f;
            if (_wasCooling && shade <= 0f) _popStart = Time.unscaledTime;
            // A weapon hit shaves the cooldown: it drops faster than time can drain it, so pop the slot.
            else if (_wasCooling && _lastSeconds - cooldownSeconds > ShavePopSeconds) _popStart = Time.unscaledTime;
            _lastSeconds = cooldownSeconds;
            _wasCooling = shade > 0f;
            var pop = Mathf.Clamp01((Time.unscaledTime - _popStart) / PopSeconds);
            transform.localScale = Vector3.one * (1f + PopSize * Mathf.Sin(pop * Mathf.PI));
            _cooldownShade.enabled = shade > 0f;
            _cooldownShade.fillAmount = shade;
            if (_levelBadge != null) _levelBadge.gameObject.SetActive(icon != null && level >= 2);

            _seconds.gameObject.SetActive(shade > 0f);
            if (shade > 0f) _seconds.text = CooldownLabel(cooldownSeconds);
            var hint = icon != null && !string.IsNullOrEmpty(keyHint);
            _key.gameObject.SetActive(hint);
            if (hint) _key.text = keyHint;
        }

        /// <summary>"7" for 6.2 s left, "0.8" under one second.</summary>
        public static string CooldownLabel(float seconds) =>
            seconds < 1f ? Mathf.Max(0.1f, seconds).ToString("0.0") : Mathf.CeilToInt(seconds).ToString();

        private void EnsureLevelBadge()
        {
            if (_levelBadge != null) return;
            _levelBadge = MakeText("LevelBadge", 16, new Vector2(1f, 1f), new Vector2(24f, 20f), new Vector2(2f, 2f));
            _levelBadge.text = "II";
            _seconds = MakeText("Seconds", 18, new Vector2(0.5f, 0.5f), new Vector2(60f, 30f), Vector2.zero);
            _key = MakeText("KeyHint", 20, new Vector2(0.5f, 0f), new Vector2(34f, 24f), new Vector2(0f, -1f), UiPalette.Ink, false);
            _key.rectTransform.pivot = new Vector2(0.5f, 1f);
        }

        private Text MakeText(string name, int size, Vector2 anchor, Vector2 sizeDelta, Vector2 position,
            Color? color = null, bool outline = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = position;

            var text = go.GetComponent<Text>();
            UiKit.StyleText(text, true, size, color ?? UiPalette.Cream);
            if (outline) UiKit.InkOutline(text, 1.5f);
            else Destroy(go.GetComponent<Outline>());
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            go.SetActive(false);
            return text;
        }
    }
}
