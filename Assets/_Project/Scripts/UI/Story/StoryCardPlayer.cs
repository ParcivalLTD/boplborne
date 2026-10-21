using System.Collections.Generic;
using GraceRun.UI.Menus;
using GraceRun.UI.Skin;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// Plays a list of story cards one after another with held-frame slide-in / hold / slide-out. Enter, a click or
    /// the gamepad's top button skips the current card; Backspace or the gamepad's east button skips them all.
    /// Cosmetic: the canvas never takes clicks and nothing here touches the game or the network.
    /// </summary>
    public class StoryCardPlayer : MonoBehaviour
    {
        private const int SortingOrder = 40;

        private readonly StoryCardPanel _panel = new();
        private StoryData _data;
        private IReadOnlyList<StoryCard> _cards;
        private Image _dim;
        private float _time;
        private int _shown = -1;

        /// <summary>True while cards are on screen.</summary>
        public static bool IsPlaying { get; private set; }

        /// <summary>True once <see cref="Play"/> has cards to show on this component.</summary>
        public bool Playing => _cards != null;

        private void Awake()
        {
            var canvas = MenuKit.CreateCanvas("StoryCardCanvas", SortingOrder);
            canvas.transform.SetParent(transform, false);
            Destroy(canvas.GetComponent<GraphicRaycaster>());
            _dim = MenuKit.Fill(canvas.transform, "Dim", UiPalette.Dim).GetComponent<Image>();
            _dim.raycastTarget = false;
            _panel.Build(canvas.transform);
        }

        private void OnDisable() => Stop();

        private void LateUpdate()
        {
            if (_cards == null) return;
            _time += Time.unscaledDeltaTime;
            if (SkipAll()) _time = float.MaxValue;
            else if (SkipOne()) _time = StoryRules.SkipTime(_time, _cards.Count, _data.CardSlideIn, _data.CardHold, _data.CardSlideOut);
            var p = StoryRules.OpeningProgress(_time, _cards.Count, _data.CardSlideIn, _data.CardHold, _data.CardSlideOut, _data.FramesPerSecond);
            if (p.done)
            {
                Stop();
                return;
            }
            if (p.card != _shown)
            {
                _shown = p.card;
                _panel.Set(_cards[p.card]);
                _panel.Root.SetActive(true);
            }
            _panel.Pose(p.enter, p.exit);
            var dim = _dim.color;
            dim.a = UiPalette.Dim.a * Mathf.Clamp01(_time / 0.25f);
            _dim.color = dim;
        }

        /// <summary>Starts the cards; ignored when there is nothing to show or the data is missing.</summary>
        public void Play(StoryData data, IReadOnlyList<StoryCard> cards)
        {
            if (data == null || cards == null || cards.Count == 0) return;
            _data = data;
            _cards = cards;
            _time = 0f;
            _shown = -1;
            IsPlaying = true;
            _dim.gameObject.SetActive(true);
        }

        /// <summary>Hides everything and ends the sequence.</summary>
        public void Stop()
        {
            _cards = null;
            IsPlaying = false;
            if (_panel.Root != null) _panel.Root.SetActive(false);
            if (_dim != null) _dim.gameObject.SetActive(false);
        }

        private static bool SkipOne()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pad = Gamepad.current;
            return (keyboard != null && keyboard.enterKey.wasPressedThisFrame)
                || (mouse != null && mouse.leftButton.wasPressedThisFrame)
                || (pad != null && pad.buttonNorth.wasPressedThisFrame);
        }

        private static bool SkipAll()
        {
            var keyboard = Keyboard.current;
            var pad = Gamepad.current;
            return (keyboard != null && keyboard.backspaceKey.wasPressedThisFrame)
                || (pad != null && pad.buttonEast.wasPressedThisFrame);
        }
    }
}
