using GraceRun.Abilities;
using GraceRun.Core;
using GraceRun.Run;
using GraceRun.UI.Skin;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Rest stop shop screen (docs/GDD.md, docs/PROMPTS.md 6.4): displays refilled flasks,
    /// shared group currency, and options to buy team rerolls, upgrade abilities to Level 2,
    /// or purchase one-off party buffs for the next boss encounter.
    /// </summary>
    public class RestStopScreen : MonoBehaviour
    {
        // Cinema look: cream cards with ink text on a dimmed sepia backdrop; text drawn straight on the backdrop is cream / paper / brass.
        private static readonly Color NightColor = new(0.071f, 0.059f, 0.094f, 0.93f);
        private static readonly Color CardColor = Color.white;
        private static readonly Color OnDark = UiPalette.Cream;
        private static readonly Color OnDarkSoft = UiPalette.Paper;
        private static readonly Color Brass = UiPalette.Brass;
        private static readonly Color CardInk = UiPalette.Ink;
        private static readonly Color CardSoft = UiPalette.Sepia;
        private static readonly Color CardAccent = UiPalette.SepiaDark;

        private GameObject _root;
        private Text _currencyText;
        private Text _rerollStatusText;
        private Button _rerollButton;
        private Text _rerollButtonText;

        private readonly Text[] _upgradeSlotNames = new Text[AbilityLoadout.SlotCount];
        private readonly Button[] _upgradeButtons = new Button[AbilityLoadout.SlotCount];
        private readonly Text[] _upgradeButtonTexts = new Text[AbilityLoadout.SlotCount];

        private Text _buffStatusText;
        private Button _buff1Button;
        private Button _buff2Button;
        private Button _buff3Button;
        private Text _buff1Text;
        private Text _buff2Text;
        private Text _buff3Text;

        private Button _advanceButton;
        private Text _advanceButtonText;
        private Text _waitingLeaderText;

        private bool _shownOpen;

        private void Awake()
        {
            BuildUi();
            if (_root != null) _root.SetActive(false);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            EnsureCanvas();
        }

        /// <summary>Ensures the RestStopScreen host canvas exists in the Game scene.</summary>
        public static void EnsureCanvas()
        {
            if (FindAnyObjectByType<RestStopScreen>() == null)
            {
                var host = new GameObject("RestStopScreenCanvas");
                host.AddComponent<RestStopScreen>();
            }
        }

        private void LateUpdate()
        {
            var loop = FightLoop.Instance;
            var net = NetworkManager.Singleton;
            var isRestStop = loop != null && loop.IsSpawned && loop.Phase == FightPhase.RestStop && net != null && net.IsClient;

            if (isRestStop != _shownOpen)
            {
                _shownOpen = isRestStop;
                if (_root != null) _root.SetActive(isRestStop);
            }

            if (_shownOpen) Refresh();
        }

        private void Refresh()
        {
            var run = RunManager.Instance;
            var draft = DraftManager.Instance;
            var loop = FightLoop.Instance;
            if (run == null || !run.IsSpawned || draft == null) return;

            var config = run.Config;
            var currency = run.SharedCurrency;
            var buff = run.ActiveBuff;

            if (_currencyText != null)
                _currencyText.text = $"Shared Currency:  <color=#{ColorUtility.ToHtmlStringRGB(Brass)}>✦ {currency}</color>";

            // 1. Team Reroll Card
            var rerollCost = config != null ? config.RerollCost : 50;
            var extraRerolls = draft.ExtraRerolls;
            if (_rerollStatusText != null)
                _rerollStatusText.text = $"Bonus Rerolls Purchased: {extraRerolls}";
            if (_rerollButton != null)
            {
                _rerollButton.interactable = RestStopRules.CanBuyReroll(currency, rerollCost);
                if (_rerollButtonText != null)
                    _rerollButtonText.text = $"+1 Team Reroll  ({rerollCost} ✦)";
            }

            // 2. Ability Upgrades (Level 2)
            var upgradeCost = config != null ? config.UpgradeCost : 100;
            var loadout = LocalLoadout();
            for (var i = 0; i < AbilityLoadout.SlotCount; i++)
            {
                var ability = loadout != null ? loadout.Get(i) : null;
                var level = loadout != null ? loadout.Level(i) : 1;
                var hasAbility = ability != null;
                var canUpgrade = loadout != null && loadout.CanUpgrade(i) && RestStopRules.CanUpgradeAbility(hasAbility, level, currency, upgradeCost);

                if (_upgradeSlotNames[i] != null)
                {
                    if (!hasAbility)
                        _upgradeSlotNames[i].text = $"Slot {i + 1}: Empty";
                    else if (level >= 2)
                        _upgradeSlotNames[i].text = $"Slot {i + 1}: {ability.DisplayName} <color=#{ColorUtility.ToHtmlStringRGB(CardAccent)}>[LEVEL 2 MAX]</color>";
                    else
                        _upgradeSlotNames[i].text = $"Slot {i + 1}: {ability.DisplayName} (Level 1)";
                }

                if (_upgradeButtons[i] != null)
                {
                    _upgradeButtons[i].interactable = canUpgrade;
                    if (_upgradeButtonTexts[i] != null)
                    {
                        if (level >= 2)
                            _upgradeButtonTexts[i].text = "MAX LEVEL";
                        else if (!hasAbility)
                            _upgradeButtonTexts[i].text = "No Ability";
                        else
                            _upgradeButtonTexts[i].text = $"Upgrade to Lvl 2  ({upgradeCost} ✦)";
                    }
                }
            }

            // 3. One-off Party Buff Card
            var buffCost = config != null ? config.BuffCost : 75;
            var tuning = config != null ? config.PartyBuffs : new PartyBuffTuning();
            if (_buffStatusText != null)
            {
                if (buff == PartyBuff.None)
                    _buffStatusText.text = "Active: None (buy one below for the next fight)";
                else
                    _buffStatusText.text = $"Active: <color=#{ColorUtility.ToHtmlStringRGB(CardAccent)}>{BuffName(buff, tuning)}</color>";
            }

            RefreshBuffButton(_buff1Button, _buff1Text, PartyBuff.EmpoweredAttacks, buff, currency, buffCost, tuning);
            RefreshBuffButton(_buff2Button, _buff2Text, PartyBuff.IronSkin, buff, currency, buffCost, tuning);
            RefreshBuffButton(_buff3Button, _buff3Text, PartyBuff.Swiftstride, buff, currency, buffCost, tuning);

            // 4. Advance / Next Floor Button
            var isLeader = loop != null && loop.IsLocalLeader;
            if (_advanceButton != null) _advanceButton.gameObject.SetActive(isLeader);
            if (_waitingLeaderText != null) _waitingLeaderText.gameObject.SetActive(!isLeader);
        }

        private static void RefreshBuffButton(Button button, Text label, PartyBuff offered, PartyBuff active,
            int currency, int cost, PartyBuffTuning tuning)
        {
            if (button == null) return;
            button.interactable = RestStopRules.CanBuyBuff(offered, active, currency, cost);
            if (label != null) label.text = $"{BuffShortName(offered, tuning)} — {cost} ✦";
        }

        private static string BuffName(PartyBuff buff, PartyBuffTuning t) => buff switch
        {
            PartyBuff.EmpoweredAttacks => $"Empowered Attacks (+{Percent(t.EmpoweredDamageMultiplier)}% Boss Damage)",
            PartyBuff.IronSkin => $"Iron Skin (+{t.IronSkinBonusMaxHp} Max HP)",
            PartyBuff.Swiftstride => $"Swiftstride (+{Percent(t.SwiftstrideMoveMultiplier)}% Move Speed, +{Percent(t.SwiftstrideStaminaRegenMultiplier)}% Stamina Regen)",
            _ => "None",
        };

        private static string BuffShortName(PartyBuff buff, PartyBuffTuning t) => buff switch
        {
            PartyBuff.EmpoweredAttacks => $"Empowered Strikes (+{Percent(t.EmpoweredDamageMultiplier)}% Dmg)",
            PartyBuff.IronSkin => $"Iron Skin (+{t.IronSkinBonusMaxHp} Max HP)",
            PartyBuff.Swiftstride => $"Swiftstride (+{Percent(t.SwiftstrideMoveMultiplier)}% Spd)",
            _ => "None",
        };

        private static int Percent(float multiplier) => Mathf.RoundToInt((multiplier - 1f) * 100f);

        private static AbilityLoadout LocalLoadout()
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsClient) return null;
            var player = net.LocalClient != null ? net.LocalClient.PlayerObject : null;
            return player != null ? player.GetComponent<AbilityLoadout>() : null;
        }

        private void OnBuyReroll()
        {
            var run = RunManager.Instance;
            if (run != null) run.BuyRerollRpc();
        }

        private void OnBuyUpgrade(int slot)
        {
            var run = RunManager.Instance;
            if (run != null) run.BuyUpgradeRpc((byte)slot);
        }

        private void OnBuyBuff(PartyBuff buff)
        {
            var run = RunManager.Instance;
            if (run != null) run.BuyBuffRpc(buff);
        }

        private void OnAdvance()
        {
            var run = RunManager.Instance;
            if (run != null) run.ServerAdvance();
        }

        private void BuildUi()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 12;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            _root = CreatePanel("RestStopRoot", transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero, NightColor);

            // Title & Subtitle Banner
            var title = CreateText("Title", _root.transform, "REST STOP", 80, FontStyle.Bold, TextAnchor.MiddleCenter, OnDark);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(1000, 100), Vector2.zero);

            var flaskStatus = CreateText("FlaskStatus", _root.transform, "Flasks refilled to full (2 charges)", 24, FontStyle.Normal, TextAnchor.MiddleCenter, OnDarkSoft);
            SetRect(flaskStatus.rectTransform, new Vector2(0.5f, 0.865f), new Vector2(0.5f, 0.865f), new Vector2(0.5f, 0.5f), new Vector2(1000, 36), Vector2.zero);

            _currencyText = CreateText("CurrencyText", _root.transform, "Shared Currency: ✦ 0", 32, FontStyle.Normal, TextAnchor.MiddleCenter, OnDark);
            _currencyText.supportRichText = true;
            SetRect(_currencyText.rectTransform, new Vector2(0.5f, 0.815f), new Vector2(0.5f, 0.815f), new Vector2(0.5f, 0.5f), new Vector2(1000, 44), Vector2.zero);

            // 3 Column Cards
            var cardWidth = 520f;
            var cardHeight = 600f;
            var centerY = 450f;

            // Card 1: Team Reroll
            var card1 = CreateCard("CardReroll", _root.transform, new Vector2(360, centerY), new Vector2(cardWidth, cardHeight));
            CreateText("Card1Title", card1.transform, "TEAM REROLL", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.90f), new Vector2(cardWidth - 60, 48));
            CreateText("Card1Desc", card1.transform, "Gives every party member +1 reroll\nfor upcoming ability drafts.", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardSoft, new Vector2(0.5f, 0.70f), new Vector2(cardWidth - 60, 76));
            _rerollStatusText = CreateText("Card1Status", card1.transform, "Bonus Rerolls Purchased: 0", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.47f), new Vector2(cardWidth - 60, 40));
            _rerollButton = CreateButton("BuyRerollBtn", card1.transform, "+1 Team Reroll (50 ✦)", new Vector2(0.5f, 0.22f), new Vector2(cardWidth - 80, 72), OnBuyReroll, out _rerollButtonText);

            // Card 2: Ability Upgrade (Level 2)
            var card2 = CreateCard("CardUpgrade", _root.transform, new Vector2(960, centerY), new Vector2(cardWidth, cardHeight));
            CreateText("Card2Title", card2.transform, "UPGRADE ABILITY", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.92f), new Vector2(cardWidth - 60, 48));
            CreateText("Card2Desc", card2.transform, "Upgrade an ability to Level 2:\n<color=#3B2A1E>Cooldown -20% | Power +25%</color>", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardSoft, new Vector2(0.5f, 0.80f), new Vector2(cardWidth - 60, 76));

            var slotY = new[] { 0.60f, 0.39f, 0.18f };
            for (var i = 0; i < AbilityLoadout.SlotCount; i++)
            {
                var slot = i;
                _upgradeSlotNames[i] = CreateText($"Slot{i}Name", card2.transform, $"Slot {i + 1}: Empty", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, slotY[i] + 0.055f), new Vector2(cardWidth - 40, 40));
                _upgradeSlotNames[i].supportRichText = true;
                _upgradeButtons[i] = CreateButton($"Slot{i}Btn", card2.transform, "Upgrade (100 ✦)", new Vector2(0.5f, slotY[i] - 0.03f), new Vector2(cardWidth - 80, 56), () => OnBuyUpgrade(slot), out _upgradeButtonTexts[i]);
            }

            // Card 3: One-off Party Buff
            var card3 = CreateCard("CardBuff", _root.transform, new Vector2(1560, centerY), new Vector2(cardWidth, cardHeight));
            CreateText("Card3Title", card3.transform, "PARTY BUFF", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.92f), new Vector2(cardWidth - 60, 48));
            _buffStatusText = CreateText("Card3Status", card3.transform, "Active: None (buy one below)", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardSoft, new Vector2(0.5f, 0.78f), new Vector2(cardWidth - 60, 76));
            _buffStatusText.supportRichText = true;

            _buff1Button = CreateButton("Buff1Btn", card3.transform, "Empowered Strikes (+20% Dmg) — 75 ✦", new Vector2(0.5f, 0.55f), new Vector2(cardWidth - 60, 84), () => OnBuyBuff(PartyBuff.EmpoweredAttacks), out _buff1Text);
            _buff2Button = CreateButton("Buff2Btn", card3.transform, "Iron Skin (+25 Max HP) — 75 ✦", new Vector2(0.5f, 0.35f), new Vector2(cardWidth - 60, 84), () => OnBuyBuff(PartyBuff.IronSkin), out _buff2Text);
            _buff3Button = CreateButton("Buff3Btn", card3.transform, "Swiftstride (+15% Spd) — 75 ✦", new Vector2(0.5f, 0.15f), new Vector2(cardWidth - 60, 84), () => OnBuyBuff(PartyBuff.Swiftstride), out _buff3Text);

            // Bottom Continue Section
            _advanceButton = CreateButton("AdvanceBtn", _root.transform, "Continue to Next Floor", new Vector2(0.5f, 0.09f), new Vector2(560, 80), OnAdvance, out _advanceButtonText);
            if (_advanceButtonText != null) UiKit.StyleText(_advanceButtonText, false, 32, CardInk);

            _waitingLeaderText = CreateText("WaitingLeaderText", _root.transform, "Waiting for the host to continue to next floor...", 24, FontStyle.Normal, TextAnchor.MiddleCenter, OnDarkSoft);
            SetRect(_waitingLeaderText.rectTransform, new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.5f), new Vector2(1000, 44), Vector2.zero);
        }

        private GameObject CreateCard(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var panel = CreatePanel(name, parent, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), size, CardColor);
            panel.GetComponent<RectTransform>().anchoredPosition = pos;
            var theme = UiTheme.Current;
            UiKit.Slice(panel.GetComponent<Image>(), theme != null ? theme.PanelCream : null);
            return panel;
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetRect(rect, anchorMin, anchorMax, pivot, size, Vector2.zero);
            var img = go.GetComponent<Image>();
            img.color = color;
            return go;
        }

        private Text CreateText(string name, Transform parent, string content, int size, FontStyle style, TextAnchor align, Color color, Vector2 anchorNorm = default, Vector2 sizeDelta = default)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var txt = go.GetComponent<Text>();
            // One pixel look: display font for titles (bold 32+ or 64+), body font for the rest.
            UiKit.StyleText(txt, size >= 64 || (style == FontStyle.Bold && size >= 32), size, color);
            txt.alignment = align;
            txt.text = content;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            if (anchorNorm != default)
            {
                var rect = txt.rectTransform;
                SetRect(rect, anchorNorm, anchorNorm, new Vector2(0.5f, 0.5f), sizeDelta, Vector2.zero);
            }
            return txt;
        }

        private Button CreateButton(string name, Transform parent, string label, Vector2 anchorNorm, Vector2 sizeDelta, UnityEngine.Events.UnityAction action, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetRect(rect, anchorNorm, anchorNorm, new Vector2(0.5f, 0.5f), sizeDelta, Vector2.zero);

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(action);

            labelText = CreateText("Label", go.transform, label, 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardInk);
            SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            labelText.rectTransform.offsetMin = new Vector2(12f, 4f);
            labelText.rectTransform.offsetMax = new Vector2(-12f, -4f);
            // Long labels (buff names) shrink a little instead of overflowing the button.
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 16;
            labelText.resizeTextMaxSize = 24;
            UiKit.StyleButton(btn, labelText, 24);
            return btn;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPos;
        }
    }
}
