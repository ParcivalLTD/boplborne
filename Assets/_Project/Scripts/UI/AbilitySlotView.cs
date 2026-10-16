using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// One ability slot in a <see cref="PlayerPanel"/>: an icon (hidden while the slot is empty)
    /// and a dark cooldown shade that drains from the top as the cooldown runs out.
    /// The ability framework (M5) feeds it; until then every slot stays empty.
    /// </summary>
    public class AbilitySlotView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField, Tooltip("Filled image (vertical, origin bottom) over the icon.")]
        private Image _cooldownShade;

        private void Awake() => Show(null, 0f);

        /// <summary>Shows <paramref name="icon"/> (null = empty slot) with 0..1 cooldown left.</summary>
        public void Show(Sprite icon, float cooldownRemaining01)
        {
            _icon.enabled = icon != null;
            _icon.sprite = icon;
            var shade = icon != null ? Mathf.Clamp01(cooldownRemaining01) : 0f;
            _cooldownShade.enabled = shade > 0f;
            _cooldownShade.fillAmount = shade;
        }
    }
}
