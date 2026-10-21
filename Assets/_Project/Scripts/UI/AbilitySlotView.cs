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

        private void Awake() => Show(null, 0f, 1);

        /// <summary>Shows <paramref name="icon"/> (null = empty slot) with 0..1 cooldown left and level badge.</summary>
        public void Show(Sprite icon, float cooldownRemaining01, int level = 1)
        {
            EnsureLevelBadge();
            _icon.enabled = icon != null;
            _icon.sprite = icon;
            var shade = icon != null ? Mathf.Clamp01(cooldownRemaining01) : 0f;
            _cooldownShade.enabled = shade > 0f;
            _cooldownShade.fillAmount = shade;
            if (_levelBadge != null) _levelBadge.gameObject.SetActive(icon != null && level >= 2);
        }

        private void EnsureLevelBadge()
        {
            if (_levelBadge != null) return;
            var go = new GameObject("LevelBadge", typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(24f, 20f);
            rect.anchoredPosition = new Vector2(2f, 2f);

            _levelBadge = go.GetComponent<Text>();
            UiKit.StyleText(_levelBadge, true, 16, UiPalette.Cream);
            UiKit.InkOutline(_levelBadge, 1f);
            _levelBadge.alignment = TextAnchor.MiddleCenter;
            _levelBadge.text = "II";
            _levelBadge.raycastTarget = false;
            go.SetActive(false);
        }
    }
}
