using GraceRun.Core;
using GraceRun.Player;
using GraceRun.Run;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Full run results overlay on every PC (docs/GDD.md, docs/PROMPTS.md 6.5):
    /// victory vs defeat banner, floors reached, total run time, damage dealt per player,
    /// the funniest stat (friendly-fire knockbacks), and the "Return to Hub" flow.
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        private static readonly Color NightColor = new(0.071f, 0.063f, 0.102f, 0.95f);
        private static readonly Color StoneColor = new(0.165f, 0.145f, 0.200f);
        private static readonly Color StoneLightColor = new(0.290f, 0.259f, 0.337f);
        private static readonly Color FogColor = new(0.549f, 0.518f, 0.627f);
        private static readonly Color BoneColor = new(0.929f, 0.906f, 0.965f);
        private static readonly Color GoldColor = new(1.000f, 0.788f, 0.290f);
        private static readonly Color DefeatColor = new(0.878f, 0.294f, 0.294f);

        private static readonly Color[] DefaultPlayerColors =
        {
            new(0.298f, 0.553f, 1.000f),
            new(1.000f, 0.604f, 0.235f),
            new(0.235f, 0.796f, 0.498f),
            new(1.000f, 0.373f, 0.659f)
        };

        private GameObject _root;
        private Text _titleText;
        private Text _subtitleText;

        private Text _timeSummaryText;
        private Text _floorSummaryText;
        private Text _damageSummaryText;

        private readonly GameObject[] _playerCards = new GameObject[4];
        private readonly Text[] _playerDamageTexts = new Text[4];
        private readonly Text[] _playerKnockbackTexts = new Text[4];
        private readonly Text[] _playerBadges = new Text[4];
        private readonly Text[] _playerHeaders = new Text[4];

        private Text _funStatTitleText;
        private Text _funStatHighlightText;
        private Text _funStatDetailText;

        private Button _returnButton;
        private Text _returnButtonText;
        private Text _waitingLeaderText;

        private bool _shownOpen;
        private Font _font;
        private string _unlocksBannerText = string.Empty;

        private void OnEnable()
        {
            MetaProgress.UnlocksEarned += OnUnlocksEarned;
        }

        private void OnDisable()
        {
            MetaProgress.UnlocksEarned -= OnUnlocksEarned;
        }

        private void OnUnlocksEarned(MetaProgressData data, System.Collections.Generic.IReadOnlyList<string> unlocks)
        {
            if (unlocks != null && unlocks.Count > 0)
            {
                _unlocksBannerText = "★ NEW UNLOCKS: " + string.Join(", ", unlocks) + " ★";
            }
        }

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

        /// <summary>Ensures the ResultScreen host canvas exists in the Game scene.</summary>
        public static void EnsureCanvas()
        {
            if (FindAnyObjectByType<ResultScreen>() == null)
            {
                var host = new GameObject("ResultScreenCanvas");
                host.AddComponent<ResultScreen>();
            }
        }

        private void LateUpdate()
        {
            var run = RunManager.Instance;
            var net = NetworkManager.Singleton;
            var isResult = run != null && run.IsSpawned && run.RunResult.IsValid && net != null && net.IsClient;

            if (isResult != _shownOpen)
            {
                _shownOpen = isResult;
                if (_root != null) _root.SetActive(isResult);
            }

            if (_shownOpen) Refresh();
        }

        private void Refresh()
        {
            var run = RunManager.Instance;
            var loop = FightLoop.Instance;
            if (run == null || !run.IsSpawned) return;

            var result = run.RunResult;
            if (!result.IsValid) return;

            // Title & Subtitle
            if (_titleText != null)
            {
                _titleText.text = result.IsVictory ? "RUN VICTORY" : "RUN DEFEAT";
                _titleText.color = result.IsVictory ? GoldColor : DefeatColor;
            }

            if (_subtitleText != null)
            {
                var floorSummary = RunResultRules.FormatFloors(result.IsVictory, result.FloorReached, result.TotalFloors);
                _subtitleText.text = !string.IsNullOrEmpty(_unlocksBannerText)
                    ? $"{floorSummary}\n<color=#{ColorUtility.ToHtmlStringRGB(GoldColor)}>{_unlocksBannerText}</color>"
                    : floorSummary;
            }

            // Overview Summary
            if (_timeSummaryText != null)
                _timeSummaryText.text = $"TIME: {RunResultRules.FormatTime(result.ElapsedSeconds)}";

            if (_floorSummaryText != null)
                _floorSummaryText.text = result.IsVictory
                    ? $"CLEARED: {result.TotalFloors}/{result.TotalFloors} FLOORS"
                    : $"FLOOR: {result.FloorReached}/{result.TotalFloors}";

            var damageList = new[] { result.DamageP1, result.DamageP2, result.DamageP3, result.DamageP4 };
            var knockbackList = new[] { result.KnockbacksP1, result.KnockbacksP2, result.KnockbacksP3, result.KnockbacksP4 };
            var totalDamage = RunResultRules.CalculateTotalDamage(damageList);
            var totalKnockbacks = RunResultRules.CalculateTotalKnockbacks(knockbackList);

            if (_damageSummaryText != null)
                _damageSummaryText.text = $"TOTAL DAMAGE: {totalDamage:N0} DMG";

            // Player Cards
            var topDamageSlot = RunResultRules.FindTopDamageDealer(damageList);
            var topCulpritSlot = RunResultRules.FindTopKnockbackCulprit(knockbackList);

            for (var i = 0; i < 4; i++)
            {
                if (_playerHeaders[i] != null)
                    _playerHeaders[i].text = PlayerIdentity.NameOfSlot(i).ToUpperInvariant();

                if (_playerDamageTexts[i] != null)
                    _playerDamageTexts[i].text = $"{damageList[i]:N0} DMG";

                if (_playerKnockbackTexts[i] != null)
                    _playerKnockbackTexts[i].text = $"{knockbackList[i]} friendly pushes";

                if (_playerBadges[i] != null)
                {
                    if (i == topDamageSlot && damageList[i] > 0)
                        _playerBadges[i].text = "★ TOP DAMAGE";
                    else if (i == topCulpritSlot && knockbackList[i] > 0)
                        _playerBadges[i].text = "⚠ TEAM MENACE";
                    else
                        _playerBadges[i].text = string.Empty;
                }
            }

            // Funniest Stat Card
            if (_funStatTitleText != null)
                _funStatTitleText.text = "★ FUNNIEST STAT: FRIENDLY-FIRE KNOCKBACKS ★";

            if (_funStatHighlightText != null)
            {
                if (topCulpritSlot >= 0)
                {
                    _funStatHighlightText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(GoldColor)}>👑 Most Dangerous Teammate:</color>  {PlayerIdentity.NameOfSlot(topCulpritSlot)} with {knockbackList[topCulpritSlot]} friendly pushes!";
                }
                else
                {
                    _funStatHighlightText.text = "Peaceful Party: 0 friendly pushes (Are you even using combos?)";
                }
            }

            if (_funStatDetailText != null)
            {
                _funStatDetailText.text = $"Total Friendly-Fire Pushes: {totalKnockbacks}  |  {PlayerIdentity.NameOfSlot(0)}: {result.KnockbacksP1}  {PlayerIdentity.NameOfSlot(1)}: {result.KnockbacksP2}  {PlayerIdentity.NameOfSlot(2)}: {result.KnockbacksP3}  {PlayerIdentity.NameOfSlot(3)}: {result.KnockbacksP4}";
            }

            // Return to Hub Button
            var isLeader = loop != null && loop.IsLocalLeader;
            if (_returnButton != null) _returnButton.gameObject.SetActive(isLeader);
            if (_waitingLeaderText != null) _waitingLeaderText.gameObject.SetActive(!isLeader);
        }

        private void OnReturnToHub()
        {
            _unlocksBannerText = string.Empty;
            var loop = FightLoop.Instance;
            if (loop != null && loop.IsLocalLeader) loop.StartFightRpc();
        }

        private void BuildUi()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 13; // Above FightScreen and RestStop

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            _root = CreatePanel("ResultRoot", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, NightColor);

            // Title & Subtitle Banner
            _titleText = CreateText("Title", _root.transform, "RUN VICTORY", 46, FontStyle.Bold, TextAnchor.MiddleCenter, GoldColor);
            SetRect(_titleText.rectTransform, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(800, 60), Vector2.zero);

            _subtitleText = CreateText("Subtitle", _root.transform, "All Floors Cleared!", 22, FontStyle.Normal, TextAnchor.MiddleCenter, BoneColor);
            SetRect(_subtitleText.rectTransform, new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.5f), new Vector2(800, 50), Vector2.zero);

            // 3 Summary Chips (Time, Floors, Damage)
            var chipY = 0.80f;
            _timeSummaryText = CreateText("TimeChip", _root.transform, "TIME: 00:00", 20, FontStyle.Bold, TextAnchor.MiddleCenter, FogColor);
            SetRect(_timeSummaryText.rectTransform, new Vector2(0.28f, chipY), new Vector2(0.28f, chipY), new Vector2(0.5f, 0.5f), new Vector2(280, 36), Vector2.zero);

            _floorSummaryText = CreateText("FloorChip", _root.transform, "FLOORS: 3/3", 20, FontStyle.Bold, TextAnchor.MiddleCenter, FogColor);
            SetRect(_floorSummaryText.rectTransform, new Vector2(0.50f, chipY), new Vector2(0.50f, chipY), new Vector2(0.5f, 0.5f), new Vector2(280, 36), Vector2.zero);

            _damageSummaryText = CreateText("DamageChip", _root.transform, "TOTAL DAMAGE: 0 DMG", 20, FontStyle.Bold, TextAnchor.MiddleCenter, FogColor);
            SetRect(_damageSummaryText.rectTransform, new Vector2(0.72f, chipY), new Vector2(0.72f, chipY), new Vector2(0.5f, 0.5f), new Vector2(340, 36), Vector2.zero);

            // 4 Player Damage Cards
            var cardWidth = 380f;
            var cardHeight = 240f;
            var startX = 225f;
            var gapX = 490f;
            var cardCenterY = 560f;

            for (var i = 0; i < 4; i++)
            {
                var card = CreateCard($"PlayerCard{i}", _root.transform, new Vector2(startX + i * gapX, cardCenterY), new Vector2(cardWidth, cardHeight));
                _playerCards[i] = card;

                // Color Stripe at top
                var playerColor = DefaultPlayerColors[i];
                var stripe = CreatePanel($"Stripe{i}", card.transform, new Vector2(0f, 0.94f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, playerColor);

                // Player Header
                _playerHeaders[i] = CreateText($"P{i}Header", card.transform, $"PLAYER {i + 1}", 24, FontStyle.Bold, TextAnchor.MiddleCenter, playerColor, new Vector2(0.5f, 0.80f), new Vector2(cardWidth - 40, 32));

                // Badge
                _playerBadges[i] = CreateText($"P{i}Badge", card.transform, string.Empty, 14, FontStyle.Bold, TextAnchor.MiddleCenter, GoldColor, new Vector2(0.5f, 0.65f), new Vector2(cardWidth - 40, 24));

                // Damage
                CreateText($"P{i}DmgLabel", card.transform, "Damage to Bosses", 14, FontStyle.Normal, TextAnchor.MiddleCenter, FogColor, new Vector2(0.5f, 0.48f), new Vector2(cardWidth - 40, 22));
                _playerDamageTexts[i] = CreateText($"P{i}Damage", card.transform, "0 DMG", 24, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor, new Vector2(0.5f, 0.35f), new Vector2(cardWidth - 40, 30));

                // Knockbacks
                _playerKnockbackTexts[i] = CreateText($"P{i}Knockback", card.transform, "0 friendly pushes", 15, FontStyle.Italic, TextAnchor.MiddleCenter, FogColor, new Vector2(0.5f, 0.16f), new Vector2(cardWidth - 40, 24));
            }

            // Funniest Stat Highlight Section
            var funBox = CreateCard("FunStatBox", _root.transform, new Vector2(960, 260), new Vector2(1200, 150));
            _funStatTitleText = CreateText("FunTitle", funBox.transform, "★ FUNNIEST STAT: FRIENDLY-FIRE KNOCKBACKS ★", 20, FontStyle.Bold, TextAnchor.MiddleCenter, GoldColor, new Vector2(0.5f, 0.80f), new Vector2(1100, 32));

            _funStatHighlightText = CreateText("FunHighlight", funBox.transform, "Most Dangerous Teammate: -", 20, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor, new Vector2(0.5f, 0.50f), new Vector2(1100, 32));
            _funStatHighlightText.supportRichText = true;

            _funStatDetailText = CreateText("FunDetail", funBox.transform, "Friendly-fire knockbacks count", 15, FontStyle.Normal, TextAnchor.MiddleCenter, FogColor, new Vector2(0.5f, 0.22f), new Vector2(1100, 26));

            // Return to Hub Button
            _returnButton = CreateButton("ReturnBtn", _root.transform, "Return to Hub", new Vector2(0.5f, 0.08f), new Vector2(360, 60), OnReturnToHub, out _returnButtonText);
            _returnButton.image.color = new Color(0.32f, 0.26f, 0.42f);
            if (_returnButtonText != null)
            {
                _returnButtonText.color = BoneColor;
                _returnButtonText.fontSize = 22;
                _returnButtonText.fontStyle = FontStyle.Bold;
            }

            _waitingLeaderText = CreateText("WaitingText", _root.transform, "Waiting for the host to return to the Hub...", 20, FontStyle.Italic, TextAnchor.MiddleCenter, FogColor);
            SetRect(_waitingLeaderText.rectTransform, new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.5f), new Vector2(600, 40), Vector2.zero);
        }

        private GameObject CreateCard(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var card = CreatePanel(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, StoneColor);
            card.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchoredPosition.x - 960f, anchoredPosition.y - 540f);
            var outline = card.AddComponent<Outline>();
            outline.effectColor = StoneLightColor;
            outline.effectDistance = new Vector2(2, -2);
            return card;
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            SetRect(rect, anchorMin, anchorMax, pivot, size, Vector2.zero);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle style, TextAnchor align, Color color, Vector2 anchorPos = default, Vector2 size = default)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = align;
            text.color = color;

            if (size != default)
            {
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = anchorPos;
                rect.anchorMax = anchorPos;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = Vector2.zero;
            }
            return text;
        }

        private Button CreateButton(string name, Transform parent, string label, Vector2 anchorPos, Vector2 size, UnityEngine.Events.UnityAction action, out Text labelText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = StoneLightColor;

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            button.colors = colors;
            if (action != null) button.onClick.AddListener(action);

            labelText = CreateText("Label", go.transform, label, 18, FontStyle.Bold, TextAnchor.MiddleCenter, BoneColor);
            SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return button;
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
