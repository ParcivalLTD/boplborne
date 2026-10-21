using GraceRun.Core;
using GraceRun.Player;
using GraceRun.Run;
using GraceRun.UI.Skin;
using GraceRun.UI.Toon;
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
        // Cinema look: cream cards with ink text on a dimmed sepia backdrop; text drawn straight on the backdrop is cream / paper.
        private static readonly Color NightColor = new(0.071f, 0.059f, 0.094f, 0.93f);
        private static readonly Color CardColor = Color.white;
        private static readonly Color OnDark = UiPalette.Cream;
        private static readonly Color OnDarkSoft = UiPalette.Paper;
        private static readonly Color CardInk = UiPalette.Ink;
        private static readonly Color CardSoft = UiPalette.Sepia;
        private static readonly Color CardAccent = UiPalette.SepiaDark;

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
                _titleText.color = result.IsVictory ? OnDark : OnDarkSoft;
            }

            if (_subtitleText != null)
            {
                _subtitleText.text = RunResultRules.FormatFloors(result.IsVictory, result.FloorReached, result.TotalFloors);
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
                        _playerBadges[i].text = "* TOP DAMAGE *";
                    else if (i == topCulpritSlot && knockbackList[i] > 0)
                        _playerBadges[i].text = "! TEAM MENACE !";
                    else
                        _playerBadges[i].text = string.Empty;
                }
            }

            // Funniest Stat Card
            if (_funStatTitleText != null)
                _funStatTitleText.text = "FUNNIEST STAT: FRIENDLY-FIRE KNOCKBACKS";

            if (_funStatHighlightText != null)
            {
                if (topCulpritSlot >= 0)
                {
                    _funStatHighlightText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(CardAccent)}>Most Dangerous Teammate:</color>  {PlayerIdentity.NameOfSlot(topCulpritSlot)} with {knockbackList[topCulpritSlot]} friendly pushes!";
                }
                else
                {
                    _funStatHighlightText.text = "Peaceful Party: 0 friendly pushes (Are you even using combos?)";
                }
            }

            if (_funStatDetailText != null)
            {
                _funStatDetailText.text = $"Total: {totalKnockbacks}  |  {PlayerIdentity.NameOfSlot(0)}: {result.KnockbacksP1}  {PlayerIdentity.NameOfSlot(1)}: {result.KnockbacksP2}  {PlayerIdentity.NameOfSlot(2)}: {result.KnockbacksP3}  {PlayerIdentity.NameOfSlot(3)}: {result.KnockbacksP4}";
            }

            // Return to Hub Button
            var isLeader = loop != null && loop.IsLocalLeader;
            if (_returnButton != null) _returnButton.gameObject.SetActive(isLeader);
            if (_waitingLeaderText != null) _waitingLeaderText.gameObject.SetActive(!isLeader);
        }

        private void OnReturnToHub()
        {
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
            _titleText = CreateText("Title", _root.transform, "RUN VICTORY", 96, FontStyle.Bold, TextAnchor.MiddleCenter, OnDark);
            SetRect(_titleText.rectTransform, new Vector2(0.5f, 0.925f), new Vector2(0.5f, 0.925f), new Vector2(0.5f, 0.5f), new Vector2(1400, 120), Vector2.zero);

            _subtitleText = CreateText("Subtitle", _root.transform, "All Floors Cleared!", 32, FontStyle.Normal, TextAnchor.MiddleCenter, OnDark);
            SetRect(_subtitleText.rectTransform, new Vector2(0.5f, 0.855f), new Vector2(0.5f, 0.855f), new Vector2(0.5f, 0.5f), new Vector2(1200, 44), Vector2.zero);

            // 3 Summary Chips (Time, Floors, Damage)
            var chipY = 0.785f;
            _timeSummaryText = CreateText("TimeChip", _root.transform, "TIME: 00:00", 24, FontStyle.Bold, TextAnchor.MiddleCenter, OnDarkSoft);
            SetRect(_timeSummaryText.rectTransform, new Vector2(0.25f, chipY), new Vector2(0.25f, chipY), new Vector2(0.5f, 0.5f), new Vector2(420, 40), Vector2.zero);

            _floorSummaryText = CreateText("FloorChip", _root.transform, "FLOORS: 3/3", 24, FontStyle.Bold, TextAnchor.MiddleCenter, OnDarkSoft);
            SetRect(_floorSummaryText.rectTransform, new Vector2(0.50f, chipY), new Vector2(0.50f, chipY), new Vector2(0.5f, 0.5f), new Vector2(420, 40), Vector2.zero);

            _damageSummaryText = CreateText("DamageChip", _root.transform, "TOTAL DAMAGE: 0 DMG", 24, FontStyle.Bold, TextAnchor.MiddleCenter, OnDarkSoft);
            SetRect(_damageSummaryText.rectTransform, new Vector2(0.75f, chipY), new Vector2(0.75f, chipY), new Vector2(0.5f, 0.5f), new Vector2(520, 40), Vector2.zero);

            // 4 Player Damage Cards
            var cardWidth = 380f;
            var cardHeight = 250f;
            var startX = 225f;
            var gapX = 490f;
            var cardCenterY = 540f;

            for (var i = 0; i < 4; i++)
            {
                var card = CreateCard($"PlayerCard{i}", _root.transform, new Vector2(startX + i * gapX, cardCenterY), new Vector2(cardWidth, cardHeight));
                _playerCards[i] = card;

                // Color Stripe at top
                var playerColor = DefaultPlayerColors[i];
                var stripe = CreatePanel($"Stripe{i}", card.transform, new Vector2(0f, 0.9f), new Vector2(1f, 0.94f), Vector2.zero, Vector2.zero, playerColor);
                stripe.GetComponent<RectTransform>().offsetMin = new Vector2(24f, 0f);
                stripe.GetComponent<RectTransform>().offsetMax = new Vector2(-24f, 0f);

                // Player Header
                _playerHeaders[i] = CreateText($"P{i}Header", card.transform, $"PLAYER {i + 1}", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.78f), new Vector2(cardWidth - 40, 44));

                // Badge
                _playerBadges[i] = CreateText($"P{i}Badge", card.transform, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleCenter, CardAccent, new Vector2(0.5f, 0.62f), new Vector2(cardWidth - 40, 32));

                // Damage
                CreateText($"P{i}DmgLabel", card.transform, "Damage to Bosses", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardSoft, new Vector2(0.5f, 0.45f), new Vector2(cardWidth - 40, 32));
                _playerDamageTexts[i] = CreateText($"P{i}Damage", card.transform, "0 DMG", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.31f), new Vector2(cardWidth - 40, 44));

                // Knockbacks
                _playerKnockbackTexts[i] = CreateText($"P{i}Knockback", card.transform, "0 friendly pushes", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardSoft, new Vector2(0.5f, 0.14f), new Vector2(cardWidth - 40, 32));
            }

            // Funniest Stat Highlight Section
            var funBox = CreateCard("FunStatBox", _root.transform, new Vector2(960, 250), new Vector2(1300, 190), true);
            _funStatTitleText = CreateText("FunTitle", funBox.transform, "FUNNIEST STAT: FRIENDLY-FIRE KNOCKBACKS", 24, FontStyle.Bold, TextAnchor.MiddleCenter, CardAccent, new Vector2(0.5f, 0.80f), new Vector2(1200, 36));

            _funStatHighlightText = CreateText("FunHighlight", funBox.transform, "Most Dangerous Teammate: -", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CardInk, new Vector2(0.5f, 0.52f), new Vector2(1240, 44));
            _funStatHighlightText.supportRichText = true;

            _funStatDetailText = CreateText("FunDetail", funBox.transform, "Friendly-fire knockbacks count", 24, FontStyle.Normal, TextAnchor.MiddleCenter, CardSoft, new Vector2(0.5f, 0.2f), new Vector2(1240, 36));

            // Return to Hub Button
            _returnButton = CreateButton("ReturnBtn", _root.transform, "Return to Hub", new Vector2(0.5f, 0.09f), new Vector2(480, 80), OnReturnToHub, out _returnButtonText);
            if (_returnButtonText != null) UiKit.StyleText(_returnButtonText, false, 32, CardInk);

            _waitingLeaderText = CreateText("WaitingText", _root.transform, "Waiting for the host to return to the Hub...", 24, FontStyle.Normal, TextAnchor.MiddleCenter, OnDarkSoft);
            SetRect(_waitingLeaderText.rectTransform, new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.09f), new Vector2(0.5f, 0.5f), new Vector2(900, 44), Vector2.zero);
        }

        private GameObject CreateCard(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, bool paper = false)
        {
            var card = CreatePanel(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, CardColor);
            card.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchoredPosition.x - 960f, anchoredPosition.y - 540f);
            UiKit.Slice(card.GetComponent<Image>(), UiKit.CardPanel(paper));
            card.GetComponent<Image>().color = Color.white;
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
            // One pixel look: display font for the big banners, body font for the rest; no synthetic bold / italic.
            UiKit.StyleText(text, fontSize >= 64, fontSize, color);
            text.text = content;
            text.alignment = align;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

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

            var button = go.GetComponent<Button>();
            if (action != null) button.onClick.AddListener(action);

            labelText = CreateText("Label", go.transform, label, 32, FontStyle.Normal, TextAnchor.MiddleCenter, CardInk);
            UiKit.StyleButton(button, labelText, 32, ToonTint.Orange, -1f);
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
