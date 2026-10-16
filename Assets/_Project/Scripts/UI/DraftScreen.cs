using GraceRun.Abilities;
using GraceRun.Core;
using GraceRun.Run;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// The draft overlay on every playing PC: this player's cards, the reroll button, a "replace
    /// which slot?" choice when all 3 slots are full, and one status chip per player showing what
    /// they picked. Reads the synced hands of <see cref="DraftManager"/>; picks and rerolls go to
    /// the server through its RPCs. The HUD below stays visible, so a pick shows up in its slot at once.
    /// </summary>
    public class DraftScreen : MonoBehaviour
    {
        private const string ChooseText = "Pick one ability.";
        private const string WaitText = "Waiting for the others...";
        private const string DealingText = "Dealing your cards...";
        private const string ChoosingStatus = "choosing...";
        private const string NamePrefix = "P";

        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _hint;
        [SerializeField] private DraftCardView[] _cards;
        [SerializeField] private Button _reroll;
        [SerializeField] private Text _rerollLabel;
        [SerializeField] private GameObject _replacePanel;
        [SerializeField, Tooltip("One per ability slot, in slot order.")]
        private Button[] _replaceButtons;
        [SerializeField] private Text[] _replaceLabels;
        [SerializeField] private Button _replaceCancel;
        [SerializeField, Tooltip("One per player slot, in slot order (P1 first).")]
        private DraftStatusChip[] _chips;
        [SerializeField] private Color _commonFrame = new(0.55f, 0.52f, 0.63f);
        [SerializeField] private Color _rareFrame = new(0.6f, 0.42f, 1f);
        [SerializeField] private Color _legendaryFrame = new(1f, 0.79f, 0.29f);

        private DraftManager _manager;
        private bool _shownOpen;
        private bool _dirty;
        private int _pendingCard = -1;

        private void Awake()
        {
            for (var i = 0; i < _cards.Length; i++)
            {
                var card = i;
                _cards[i].Clicked += () => OnCard(card);
            }
            for (var i = 0; i < _replaceButtons.Length; i++)
            {
                var slot = i;
                _replaceButtons[i].onClick.AddListener(() => OnReplace(slot));
            }
            _reroll.onClick.AddListener(OnReroll);
            _replaceCancel.onClick.AddListener(CloseReplace);
            _root.SetActive(false);
        }

        private void OnDestroy() => Watch(null);

        private void LateUpdate()
        {
            var manager = DraftManager.Instance;
            if (manager != _manager) Watch(manager);

            var net = NetworkManager.Singleton;
            var open = manager != null && manager.IsOpen && net != null && net.IsClient;
            if (open != _shownOpen)
            {
                _shownOpen = open;
                _root.SetActive(open);
                CloseReplace();
                _dirty = true;
                if (open) Select(_cards.Length > 0 ? _cards[0].Button : null);
            }
            if (open && _dirty) Refresh();
        }

        private void Watch(DraftManager manager)
        {
            if (_manager != null) _manager.HandsChanged -= MarkDirty;
            _manager = manager;
            if (_manager != null) _manager.HandsChanged += MarkDirty;
            _dirty = true;
        }

        private void MarkDirty() => _dirty = true;

        private void Refresh()
        {
            _dirty = false;
            var hasHand = _manager.TryGetHand(NetworkManager.Singleton.LocalClientId, out var hand);
            for (var i = 0; i < _cards.Length; i++)
            {
                var ability = hasHand && i < hand.Cards.Length ? _manager.Catalog.Get(hand.Cards[i]) : null;
                _cards[i].Show(ability, ability != null ? FrameFor(ability.Rarity) : Color.clear,
                    hand.Picked == i, hand.HasPicked);
            }

            _hint.text = !hasHand ? DealingText : hand.HasPicked ? WaitText : ChooseText;
            _reroll.gameObject.SetActive(hasHand && !hand.HasPicked && hand.RerollsLeft > 0);
            _rerollLabel.text = $"Reroll ({hand.RerollsLeft})";
            if (hand.HasPicked) CloseReplace();
            RefreshChips();
        }

        private void RefreshChips()
        {
            foreach (var chip in _chips) chip.Hide();
            for (var i = 0; i < _manager.HandCount; i++)
            {
                var hand = _manager.GetHand(i);
                if (hand.PlayerIndex < 0 || hand.PlayerIndex >= _chips.Length) continue;
                var picked = _manager.Catalog.Get(hand.PickedAbility);
                _chips[hand.PlayerIndex].Show(_gameConfig.GetPlayerColor(hand.PlayerIndex),
                    NamePrefix + (hand.PlayerIndex + 1), picked != null ? picked.DisplayName : ChoosingStatus, picked != null);
            }
        }

        private void OnCard(int card)
        {
            var loadout = LocalLoadout();
            if (loadout == null) return;
            if (HasEmptySlot(loadout))
            {
                _manager.PickRpc((byte)card, 0);
                return;
            }

            _pendingCard = card;
            for (var i = 0; i < _replaceLabels.Length; i++)
            {
                var ability = loadout.Get(i);
                _replaceLabels[i].text = $"{i + 1}: {(ability != null ? ability.DisplayName : "-")}";
            }
            _replacePanel.SetActive(true);
            Select(_replaceButtons.Length > 0 ? _replaceButtons[0] : null);
        }

        private void OnReplace(int slot)
        {
            if (_pendingCard >= 0) _manager.PickRpc((byte)_pendingCard, (byte)slot);
            CloseReplace();
        }

        private void OnReroll()
        {
            CloseReplace();
            _manager.RerollRpc();
        }

        private void CloseReplace()
        {
            _pendingCard = -1;
            _replacePanel.SetActive(false);
        }

        private Color FrameFor(AbilityRarity rarity) => rarity switch
        {
            AbilityRarity.Rare => _rareFrame,
            AbilityRarity.Legendary => _legendaryFrame,
            _ => _commonFrame,
        };

        private static AbilityLoadout LocalLoadout()
        {
            var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
            return player != null ? player.GetComponent<AbilityLoadout>() : null;
        }

        private static bool HasEmptySlot(AbilityLoadout loadout)
        {
            for (var i = 0; i < AbilityLoadout.SlotCount; i++)
                if (loadout.Get(i) == null) return true;
            return false;
        }

        private static void Select(Selectable selectable)
        {
            if (EventSystem.current != null && selectable != null) EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }
}
