using GraceRun.Core.Audio;
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
        private const float VoiceVolume = 1f;

        private readonly AscentTowerPanel _panel = new();
        private StoryData _data;
        private Canvas _canvas;
        private float _time;
        private float _hold;
        private float _voiceStarted;
        private AudioClip _twistVoice;
        private bool _showing;

        /// <summary>Raised when the ascent finished or was skipped (not when <see cref="Stop"/> cuts it off).</summary>
        public event System.Action Completed;

        /// <summary>Raised when an ascent screen starts (for the lantern chime).</summary>
        public static event System.Action ClimbStarted;

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
            if (_twistVoice != null && !VoicePlayer.IsPlaying && _time > _voiceStarted + 0.2f)
            {
                VoicePlayer.Play(_twistVoice, VoiceVolume);
                _twistVoice = null;
            }
            if (!SkipPressed() && !_panel.Pose(_time, _hold)) return;
            Stop();
            Completed?.Invoke();
        }

        /// <summary>Shows the climb from floor <paramref name="from"/> to <paramref name="to"/> of <paramref name="total"/>.</summary>
        public void Play(int from, int to, int total)
        {
            if (_data == null) return;
            var caption = StoryRules.AscentCaption(to, total, _data.AscentCaptions, _data.FinalAscentCaption);
            var final = total > 0 && to >= total;
            var twist = final ? _data.TwistLine : null;
            _panel.Show(_canvas.transform, _data, from, to, total, caption, twist);

            var voice = _data.AscentVoice(to, total);
            _twistVoice = final ? _data.TwistVoice : null;
            var voiceLength = (voice != null ? voice.length : 0f) + (_twistVoice != null ? _twistVoice.length : 0f);
            _hold = Mathf.Max(_data.AscentHold, StoryRules.HoldSeconds(caption + " " + twist, voiceLength, _data.AscentHold, _data.WordsPerSecond, _data.ReadPadding));
            _time = 0f;
            _voiceStarted = _data.AscentSlideIn;
            VoicePlayer.Play(voice, VoiceVolume);
            _showing = true;
            ClimbStarted?.Invoke();
        }

        public void Stop()
        {
            if (_showing) VoicePlayer.Stop();
            _twistVoice = null;
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
