using System;
using GraceRun.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// One card on the <see cref="DraftScreen"/>: frame coloured by rarity, icon, name, rarity,
    /// tags, cooldown, effect text and the combo line. Clicking it raises <see cref="Clicked"/>.
    /// </summary>
    public class DraftCardView : MonoBehaviour
    {
        private const string ComboPrefix = "Combo: ";
        private const float RarityTextDarken = 0.4f;

        [SerializeField] private Button _button;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _name;
        [SerializeField] private Text _rarity;
        [SerializeField] private Text _tags;
        [SerializeField] private Text _cooldown;
        [SerializeField] private Text _description;
        [SerializeField] private Text _combo;
        [SerializeField, Tooltip("Shown on the card the player picked.")]
        private GameObject _pickedMark;
        [SerializeField] private CanvasGroup _group;
        [SerializeField, Range(0f, 1f), Tooltip("Alpha of the cards not picked, once a card is picked.")]
        private float _passedAlpha = 0.35f;

        public event Action Clicked;

        public Button Button => _button;

        private void OnEnable() => _button.onClick.AddListener(OnClick);

        private void OnDisable() => _button.onClick.RemoveListener(OnClick);

        /// <summary>Shows <paramref name="ability"/>; null hides the card.</summary>
        public void Show(AbilityData ability, Color frame, bool picked, bool anyPicked)
        {
            gameObject.SetActive(ability != null);
            if (ability == null) return;

            _frame.color = frame;
            _icon.enabled = ability.Icon != null;
            _icon.sprite = ability.Icon;
            _name.text = ability.DisplayName;
            _rarity.text = ability.Rarity.ToString().ToUpperInvariant();
            _rarity.color = new Color(frame.r * RarityTextDarken, frame.g * RarityTextDarken, frame.b * RarityTextDarken, 1f);
            _tags.text = ability.Tags.ToString().Replace(", ", " / ");
            _cooldown.text = $"{ability.Cooldown:0.#} s cooldown";
            _description.text = ability.Description;
            _combo.text = ComboPrefix + ability.ComboHook;
            _pickedMark.SetActive(picked);
            _group.alpha = anyPicked && !picked ? _passedAlpha : 1f;
            _button.interactable = !anyPicked;
        }

        private void OnClick() => Clicked?.Invoke();
    }
}
