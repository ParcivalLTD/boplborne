using GraceRun.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GraceRun.UI.Story
{
    /// <summary>
    /// The ascent screen: a full-screen tower cross-section with a lantern climbing to the new floor's window and a
    /// caption. Started by <see cref="StorySequenceView"/> as part of the story before a floor; raises
    /// <see cref="Completed"/> when it ends or is skipped (Enter, a click or the gamepad's top button).
    /// Cosmetic: never writes network state.
    /// </summary>
    public class AscentView : MonoBehaviour
    {
        private const int SortingOrder = 40;

        private readonly AscentTowerPanel _panel = new();
        private StoryData _data;
        private Canvas _canvas;
        private float _time;
        private bool _showing;

        /// <summary>Raised when the ascent finished or was skipped (not when <see cref="Stop"/> cuts it off).</summary>
        public event System.Action Completed;

        public bool Showing => _showing;

        private void Awake()
        {
            _data = Resources.Load<StoryData>(StoryData.ResourceName);
            _canvas = MenuKit.CreateCanvas("AscentCanvas", SortingOrder);
            _canvas.transform.SetParent(transform, false);
            Destroy(_canvas.GetComponent<GraphicRaycaster>());
        }

        private void OnDisable() => Stop();

        private void LateUpdate()
        {
            if (!_showing) return;
            _time += Time.unscaledDeltaTime;
            if (!SkipPressed() && !_panel.Pose(_time)) return;
            Stop();
            Completed?.Invoke();
        }

        /// <summary>Shows the climb from floor <paramref name="from"/> to <paramref name="to"/> of <paramref name="total"/>.</summary>
        public void Play(int from, int to, int total)
        {
            if (_data == null) return;
            var caption = StoryRules.AscentCaption(to, total, _data.AscentCaptions, _data.FinalAscentCaption);
            var final = total > 0 && to >= total;
            _panel.Show(_canvas.transform, _data, from, to, total, caption, final ? _data.TwistLine : null);
            _time = 0f;
            _showing = true;
        }

        public void Stop()
        {
            _showing = false;
            _panel.Hide();
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
    }
}
