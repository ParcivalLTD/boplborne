using GraceRun.Abilities;
using GraceRun.Combat;
using GraceRun.Player;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// One player's panel in the bottom HUD: colour strip with the hat, name, HP and stamina bars,
    /// 3 ability slots, flask pips, and a DOWN state while the player is a ghost. Reads the synced
    /// values of the bound player every frame, so it looks the same on every PC.
    /// </summary>
    public class PlayerPanel : MonoBehaviour
    {
        private const string NamePrefix = "P";

        [SerializeField] private Image _colorStrip;
        [SerializeField] private Image _hat;
        [SerializeField] private Text _name;
        [SerializeField, Tooltip("Pivot on the left; scaled on x by the HP fraction.")]
        private RectTransform _hpFill;
        [SerializeField, Tooltip("Pivot on the left; scaled on x by the stamina fraction.")]
        private RectTransform _staminaFill;
        [SerializeField] private AbilitySlotView[] _abilitySlots;
        [SerializeField] private Transform _flaskPipRoot;
        [SerializeField, Tooltip("Inactive template; one copy per flask charge.")]
        private Image _flaskPipTemplate;
        [SerializeField] private Color _flaskFull = Color.white;
        [SerializeField] private Color _flaskEmpty = Color.gray;
        [SerializeField, Tooltip("Dimmed while the player is down.")]
        private CanvasGroup _content;
        [SerializeField, Range(0f, 1f)] private float _downAlpha = 0.4f;
        [SerializeField] private GameObject _downLabel;

        private PlayerIdentity _player;
        private Health _health;
        private PlayerStamina _stamina;
        private PlayerFlask _flask;
        private Ghost _ghost;
        private AbilityLoadout _loadout;
        private AbilityRunner _abilities;
        private int _shownIndex = PlayerIdentity.Unassigned;
        private Image[] _pips = new Image[0];
        private bool _bound;

        public PlayerIdentity Player => _player;

        private void Awake()
        {
            _flaskPipTemplate.gameObject.SetActive(false);
        }

        /// <summary>Shows <paramref name="player"/> in this panel, or hides the panel for null.</summary>
        public void Bind(PlayerIdentity player)
        {
            // Reference check: a destroyed player compares equal to null and would keep its panel up.
            if (_bound && ReferenceEquals(player, _player)) return;
            _bound = true;
            _player = player;
            _shownIndex = PlayerIdentity.Unassigned;
            gameObject.SetActive(player != null);
            if (player == null) return;

            _health = player.GetComponent<Health>();
            _stamina = player.GetComponent<PlayerStamina>();
            _flask = player.GetComponent<PlayerFlask>();
            _ghost = player.GetComponent<Ghost>();
            _loadout = player.GetComponent<AbilityLoadout>();
            _abilities = player.GetComponent<AbilityRunner>();
            foreach (var slot in _abilitySlots) slot.Show(null, 0f);
            BuildPips(_flask != null ? _flask.MaxCharges : 0);
        }

        /// <summary>Copies the bound player's synced values into the panel.</summary>
        public void Refresh()
        {
            if (_player == null || !_player.IsSpawned) return;
            if (_player.PlayerIndex != _shownIndex) ShowIdentity(_player.PlayerIndex);

            var down = _ghost != null && _ghost.IsGhost;
            _content.alpha = down ? _downAlpha : 1f;
            _downLabel.SetActive(down);

            SetFill(_hpFill, _health != null && _health.IsSpawned ? _health.Normalized : 0f);
            SetFill(_staminaFill, _stamina != null && !down ? _stamina.Normalized : 0f);

            RefreshAbilities();

            var charges = _flask != null ? _flask.Charges : 0;
            for (var i = 0; i < _pips.Length; i++) _pips[i].color = i < charges ? _flaskFull : _flaskEmpty;
        }

        private void ShowIdentity(int index)
        {
            _shownIndex = index;
            if (index == PlayerIdentity.Unassigned) return;
            var config = _player.Config;
            _colorStrip.color = config.GetPlayerColor(index);
            _hat.sprite = config.GetHatSprite(index);
            _name.text = NamePrefix + (index + 1);
        }

        private void RefreshAbilities()
        {
            if (_loadout == null || !_loadout.IsSpawned) return;
            for (var i = 0; i < _abilitySlots.Length; i++)
            {
                var ability = _loadout.Get(i);
                var cooldown = _abilities != null ? _abilities.CooldownRemaining01(i) : 0f;
                _abilitySlots[i].Show(ability != null ? ability.Icon : null, cooldown);
            }
        }

        private void BuildPips(int count)
        {
            if (_pips.Length == count) return;
            foreach (var pip in _pips) Destroy(pip.gameObject);
            _pips = new Image[count];
            for (var i = 0; i < count; i++)
            {
                _pips[i] = Instantiate(_flaskPipTemplate, _flaskPipRoot);
                _pips[i].gameObject.SetActive(true);
            }
        }

        private static void SetFill(RectTransform fill, float value) =>
            fill.localScale = new Vector3(Mathf.Clamp01(value), 1f, 1f);
    }
}
