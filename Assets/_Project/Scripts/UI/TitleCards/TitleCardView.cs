using GraceRun.Run;
using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraceRun.UI.TitleCards
{
    /// <summary>
    /// The vintage title cards, shown locally on every PC from the fight-phase changes: before a fight
    /// (floor and boss name, tagline), after a win and after a defeat. Cosmetic only: it reads
    /// <see cref="FightLoop.PhaseChanged"/> and never blocks or changes the game. The card slides in with
    /// held frames, stays a moment and leaves; Enter, a click or the gamepad's top button skips it. It
    /// covers the upper middle of the screen for about two seconds. Builds itself when the Game scene loads.
    /// </summary>
    public class TitleCardView : MonoBehaviour
    {
        private const int SortingOrder = 15;
        private const float CardWidth = 1120f;
        private const float CardHeight = 260f;
        private const float AnchorY = 0.74f;

        private TitleCardData _data;
        private FightLoop _loop;
        private FightPhase _pending = (FightPhase)byte.MaxValue;
        private RectTransform _card;
        private CanvasGroup _group;
        private Text _title;
        private Text _tagline;
        private float _time;
        private bool _showing;

        /// <summary>True while a card is on screen.</summary>
        public static bool IsShowing { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == "Game" && !Application.isBatchMode && FindAnyObjectByType<TitleCardView>() == null)
                new GameObject("TitleCardCanvas").AddComponent<TitleCardView>();
        };

        private void Awake()
        {
            _data = Resources.Load<TitleCardData>(TitleCardData.ResourceName);
            Build();
            Hide();
        }

        private void OnDisable()
        {
            Unbind();
            IsShowing = false;
        }

        private void LateUpdate()
        {
            if (_data == null) return;
            if (FightLoop.Instance != _loop) Bind(FightLoop.Instance);
            if (_pending != (FightPhase)byte.MaxValue)
            {
                var phase = _pending;
                _pending = (FightPhase)byte.MaxValue;
                Begin(phase);
            }
            if (_showing) Animate();
        }

        private void Bind(FightLoop loop)
        {
            Unbind();
            _loop = loop;
            if (_loop != null) _loop.PhaseChanged += OnPhase;
        }

        private void Unbind()
        {
            if (_loop != null) _loop.PhaseChanged -= OnPhase;
            _loop = null;
        }

        private void OnPhase(FightPhase phase)
        {
            var net = NetworkManager.Singleton;
            if (net == null || !net.IsClient) return;
            if (phase == FightPhase.Countdown || phase == FightPhase.Victory || phase == FightPhase.Defeat) _pending = phase;
            else Hide();
        }

        private void Begin(FightPhase phase)
        {
            var run = RunManager.Instance;
            var boss = run != null && run.IsSpawned ? run.CurrentBoss : null;
            var id = boss != null ? boss.Id : null;
            var floor = run != null && run.IsSpawned ? run.Floor : 0;
            var final = run != null && run.IsSpawned && run.IsFinalFloor;
            switch (phase)
            {
                case FightPhase.Countdown:
                    _title.text = TitleCardRules.IntroTitle(floor, final, boss != null ? boss.DisplayName : null);
                    _tagline.text = _data.TaglineFor(id);
                    break;
                case FightPhase.Victory:
                    _title.text = final ? _data.FinalVictoryTitle : _data.VictoryTitle;
                    _tagline.text = final ? _data.FinalVictoryTagline : _data.VictoryTaglineFor(id);
                    break;
                default:
                    _title.text = _data.DefeatTitle;
                    _tagline.text = _data.DefeatTagline;
                    break;
            }
            _time = 0f;
            _showing = true;
            IsShowing = true;
            _group.gameObject.SetActive(true);
        }

        private void Animate()
        {
            _time += Time.unscaledDeltaTime;
            if (SkipPressed()) _time = TitleCardRules.SkipTime(_time, _data.SlideIn, _data.Hold);
            var (enter, exit, done) = TitleCardRules.Progress(_time, _data.SlideIn, _data.Hold, _data.SlideOut, _data.FramesPerSecond);
            if (done)
            {
                Hide();
                return;
            }
            _group.alpha = Mathf.Clamp01(enter * 2f) * (1f - exit);
            var zoom = Mathf.Lerp(1.5f, 1f, enter);
            _card.localScale = new Vector3(zoom, zoom, 1f) * (1f - 0.1f * exit);
            _card.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-4f, 0f, enter) + 3f * exit);
            _card.anchoredPosition = new Vector2(Mathf.Lerp(-260f, 0f, enter) + 420f * exit, 0f);
        }

        private static bool SkipPressed()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;
            return (keyboard != null && keyboard.enterKey.wasPressedThisFrame)
                || (mouse != null && mouse.leftButton.wasPressedThisFrame)
                || (pad != null && pad.buttonNorth.wasPressedThisFrame);
        }

        private void Hide()
        {
            _showing = false;
            IsShowing = false;
            _pending = (FightPhase)byte.MaxValue;
            if (_group != null) _group.gameObject.SetActive(false);
        }

        private void Build()
        {
            var canvas = MenuKit.CreateCanvas("TitleCardCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            Destroy(canvas.GetComponent<GraphicRaycaster>());
            var theme = UiTheme.Current;

            var holder = new GameObject("Holder", typeof(RectTransform), typeof(CanvasGroup));
            holder.transform.SetParent(canvas.transform, false);
            var holderRect = (RectTransform)holder.transform;
            holderRect.anchorMin = new Vector2(0f, AnchorY);
            holderRect.anchorMax = new Vector2(1f, AnchorY);
            holderRect.sizeDelta = new Vector2(0f, CardHeight);
            holderRect.anchoredPosition = Vector2.zero;
            _group = holder.GetComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            var card = UiKit.NewImage(holder.transform, "Card", theme != null ? theme.TitleCard : null, Color.white);
            _card = card.rectTransform;
            _card.anchorMin = _card.anchorMax = _card.pivot = new Vector2(0.5f, 0.5f);
            _card.sizeDelta = new Vector2(CardWidth, CardHeight);
            card.raycastTarget = false;

            _title = UiKit.NewText(_card, "Title", "", UiKit.DisplayMedium, UiPalette.Ink, TextAnchor.MiddleCenter, true);
            Place(_title.rectTransform, 0.66f, 80f);
            var divider = UiKit.NewImage(_card, "Divider", theme != null ? theme.Divider : null, Color.white);
            divider.type = Image.Type.Simple;
            divider.preserveAspect = true;
            divider.raycastTarget = false;
            Place(divider.rectTransform, 0.45f, 28f);
            divider.rectTransform.anchorMin = new Vector2(0.5f, 0.45f);
            divider.rectTransform.anchorMax = new Vector2(0.5f, 0.45f);
            divider.rectTransform.sizeDelta = new Vector2(192f, 28f);
            _tagline = UiKit.NewText(_card, "Tagline", "", UiKit.BodyNormal, UiPalette.Sepia);
            Place(_tagline.rectTransform, 0.25f, 48f);
        }

        private static void Place(RectTransform rect, float anchorY, float height)
        {
            rect.anchorMin = new Vector2(0.04f, anchorY);
            rect.anchorMax = new Vector2(0.96f, anchorY);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
