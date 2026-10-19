using GraceRun.Abilities;
using GraceRun.Core;
using GraceRun.Run;
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
        private static readonly Color NightColor = new(0.071f, 0.063f, 0.102f, 0.92f);
        private static readonly Color StoneColor = new(0.165f, 0.145f, 0.200f);
        private static readonly Color StoneLightColor = new(0.290f, 0.259f, 0.337f);
        private static readonly Color FogColor = new(0.549f, 0.518f, 0.627f);
        private static readonly Color BoneColor = new(0.929f, 0.906f, 0.965f);
        private static readonly Color GoldColor = new(1.000f, 0.788f, 0.290f);

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

        private Button _advanceButton;
        private Text _advanceButtonText;
        private Text _waitingLeaderText;

        private bool _shownOpen;
        private Font _font;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildUi();
            if (_root != null) _root.SetActive(false);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name != "Game") return;
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
                _currencyText.text = $"Shared Currency:  <color=#{ColorUtility.ToHtmlStringRGB(GoldColor)}>✦ {currency}</color>";

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
                        _upgradeSlotNames[i].text = $"Slot {i + 1}: {ability.DisplayName} <color=#{ColorUtility.ToHtmlStringRGB(GoldColor)}>[LEVEL 2 MAX]</color>";
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
            if (_buffStatusText != null)
            {
                if (buff == PartyBuff.None)
                    _buffStatusText.text = "Active: None (buy one below for the next fight)";
                else
                    _buffStatusText.text = $"Active: <color=#{ColorUtility.ToHtmlStringRGB(GoldColor)}>{BuffName(buff)}</color>";
            }

            if (_buff1Button != null)
                _buff1Button.interactable = RestStopRules.CanBuyBuff(PartyBuff.EmpoweredAttacks, buff, currency, buffCost);
            if (_buff2Button != null)
                _buff2Button.interactable = RestStopRules.CanBuyBuff(PartyBuff.IronSkin, buff, currency, buffCost);
            if (_buff3Button != null)
                _buff3Button.interactable = RestStopRules.CanBuyBuff(PartyBuff.Swiftstride, buff, currency, buffCost);

            // 4. Advance / Next Floor Button
            var isLeader = loop != null && loop.IsLocalLeader;
            if (_advanceButton != null) _advanceButton.gameObject.SetActive(isLeader);
            if (_waitingLeaderText != null) _waitingLeaderText.gameObject.SetActive(!isLeader);
        }

        private static string BuffName(PartyBuff buff) => buff switch
        {
            PartyBuff.EmpoweredAttacks => "Empowered Attacks (+20% Boss Damage)",
            PartyBuff.IronSkin => "Iron Skin (+25 Max HP)",
            PartyBuff.Swiftstride => "Swiftstride (+15% Move Speed, +25% Stamina Regen)",
            _ => "None",
        };

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
            var title = CreateText("Title", _root.transform, "REST STOP", 40, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor);
            SetRect(title.rectTransform, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(600, 50), Vector2.zero);

            var flaskStatus = CreateText("FlaskStatus", _root.transform, "Flasks refilled to full (2 charges)", 20, FontStyle.Normal, TextAnchor.MiddleCenter, FogColor);
            SetRect(flaskStatus.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f), new Vector2(600, 30), Vector2.zero);

            _currencyText = CreateText("CurrencyText", _root.transform, "Shared Currency: ✦ 0", 28, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor);
            _currencyText.supportRichText = true;
            SetRect(_currencyText.rectTransform, new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.5f), new Vector2(600, 40), Vector2.zero);

            // 3 Column Cards
            var cardWidth = 520f;
            var cardHeight = 440f;
            var centerY = 500f;

            // Card 1: Team Reroll
            var card1 = CreateCard("CardReroll", _root.transform, new Vector2(360, centerY), new Vector2(cardWidth, cardHeight));
            CreateText("Card1Title", card1.transform, "TEAM REROLL", 24, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor, new Vector2(0.5f, 0.88f), new Vector2(cardWidth - 40, 40));
            CreateText("Card1Desc", card1.transform, "Gives every party member +1 reroll\nfor upcoming ability drafts.", 18, FontStyle.Normal, TextAnchor.MiddleCenter, FogColor, new Vector2(0.5f, 0.68f), new Vector2(cardWidth - 40, 60));
            _rerollStatusText = CreateText("Card1Status", card1.transform, "Bonus Rerolls Purchased: 0", 18, FontStyle.Italic, TextAnchor.MiddleCenter, BoneColor, new Vector2(0.5f, 0.45f), new Vector2(cardWidth - 40, 30));
            _rerollButton = CreateButton("BuyRerollBtn", card1.transform, "+1 Team Reroll (50 ✦)", new Vector2(0.5f, 0.22f), new Vector2(cardWidth - 80, 56), OnBuyReroll, out _rerollButtonText);

            // Card 2: Ability Upgrade (Level 2)
            var card2 = CreateCard("CardUpgrade", _root.transform, new Vector2(960, centerY), new Vector2(cardWidth, cardHeight));
            CreateText("Card2Title", card2.transform, "UPGRADE ABILITY", 24, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor, new Vector2(0.5f, 0.90f), new Vector2(cardWidth - 40, 40));
            CreateText("Card2Desc", card2.transform, "Upgrade an ability to Level 2:\n<color=#FFC94A>Cooldown -20% | Power +25%</color>", 16, FontStyle.Normal, TextAnchor.MiddleCenter, FogColor, new Vector2(0.5f, 0.77f), new Vector2(cardWidth - 40, 45));

            var slotY = new[] { 0.58f, 0.38f, 0.18f };
            for (var i = 0; i < AbilityLoadout.SlotCount; i++)
            {
                var slot = i;
                _upgradeSlotNames[i] = CreateText($"Slot{i}Name", card2.transform, $"Slot {i + 1}: Empty", 16, FontStyle.Bold, TextAnchor.MiddleLeft, BoneColor, new Vector2(0.32f, slotY[i]), new Vector2(250, 30));
                _upgradeSlotNames[i].supportRichText = true;
                _upgradeButtons[i] = CreateButton($"Slot{i}Btn", card2.transform, "Upgrade (100 ✦)", new Vector2(0.76f, slotY[i]), new Vector2(180, 42), () => OnBuyUpgrade(slot), out _upgradeButtonTexts[i]);
            }

            // Card 3: One-off Party Buff
            var card3 = CreateCard("CardBuff", _root.transform, new Vector2(1560, centerY), new Vector2(cardWidth, cardHeight));
            CreateText("Card3Title", card3.transform, "ONE-OFF PARTY BUFF", 24, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor, new Vector2(0.5f, 0.90f), new Vector2(cardWidth - 40, 40));
            _buffStatusText = CreateText("Card3Status", card3.transform, "Active: None (buy one below)", 16, FontStyle.Italic, TextAnchor.MiddleCenter, FogColor, new Vector2(0.5f, 0.78f), new Vector2(cardWidth - 40, 36));
            _buffStatusText.supportRichText = true;

            _buff1Button = CreateButton("Buff1Btn", card3.transform, "Empowered Strikes (+20% Dmg) — 75 ✦", new Vector2(0.5f, 0.58f), new Vector2(cardWidth - 60, 48), () => OnBuyBuff(PartyBuff.EmpoweredAttacks), out _);
            _buff2Button = CreateButton("Buff2Btn", card3.transform, "Iron Skin (+25 Max HP) — 75 ✦", new Vector2(0.5f, 0.38f), new Vector2(cardWidth - 60, 48), () => OnBuyBuff(PartyBuff.IronSkin), out _);
            _buff3Button = CreateButton("Buff3Btn", card3.transform, "Swiftstride (+15% Spd) — 75 ✦", new Vector2(0.5f, 0.18f), new Vector2(cardWidth - 60, 48), () => OnBuyBuff(PartyBuff.Swiftstride), out _);

            // Bottom Continue Section
            _advanceButton = CreateButton("AdvanceBtn", _root.transform, "Continue to Next Floor", new Vector2(0.5f, 0.10f), new Vector2(360, 60), OnAdvance, out _advanceButtonText);
            _advanceButton.image.color = new Color(0.30f, 0.25f, 0.40f);
            if (_advanceButtonText != null)
            {
                _advanceButtonText.color = BoneColor;
                _advanceButtonText.fontSize = 22;
                _advanceButtonText.fontStyle = FontStyle.Bold;
            }

            _waitingLeaderText = CreateText("WaitingLeaderText", _root.transform, "Waiting for the host to continue to next floor...", 20, FontStyle.Italic, TextAnchor.MiddleCenter, FogColor);
            SetRect(_waitingLeaderText.rectTransform, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.5f), new Vector2(600, 40), Vector2.zero);
        }

        private GameObject CreateCard(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var panel = CreatePanel(name, parent, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), size, StoneColor);
            panel.GetComponent<RectTransform>().anchoredPosition = pos;
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
            txt.font = _font;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.alignment = align;
            txt.color = color;
            txt.text = content;
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

            var img = go.GetComponent<Image>();
            img.color = StoneLightColor;

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = StoneLightColor;
            colors.highlightedColor = new Color(0.42f, 0.38f, 0.48f);
            colors.pressedColor = new Color(0.24f, 0.20f, 0.28f);
            colors.disabledColor = new Color(0.18f, 0.16f, 0.22f, 0.6f);
            btn.colors = colors;
            btn.onClick.AddListener(action);

            labelText = CreateText("Label", go.transform, label, 16, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor);
            SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
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
