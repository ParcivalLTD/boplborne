using System.Collections.Generic;
using GraceRun.Core.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GraceRun.UI
{
    /// <summary>
    /// Plays the menu sounds for any uGUI: a tick when the selection or the pointer moves onto a
    /// control, a click or confirm when it is pressed (mouse click or the submit key), a back sound
    /// for Back / Close / Cancel, and the draft pick sound on a draft card. Works on every screen
    /// without changing it; local cosmetic only. Lives in the Bootstrap scene next to AudioDirector.
    /// </summary>
    public class UiSounds : MonoBehaviour
    {
        private static readonly string[] BackWords = { "back", "cancel", "close", "resume", "quit", "leave" };
        private static readonly string[] ConfirmWords = { "start", "host", "join", "play", "next", "confirm", "buy", "return", "continue", "apply" };

        [SerializeField] private GameAudioData _sounds;

        private readonly List<RaycastResult> _hits = new();
        private PointerEventData _pointer;
        private GameObject _lastSelected;
        private Selectable _lastHover;
        private Vector2 _lastPointer;

        private void Awake()
        {
            if (Application.isBatchMode) enabled = false;
        }

        private void Update()
        {
            var system = EventSystem.current;
            if (_sounds == null || system == null) return;
            var module = system.currentInputModule as InputSystemUIInputModule;

            var selected = system.currentSelectedGameObject;
            if (selected != _lastSelected)
            {
                if (selected != null && _lastSelected != null) Play(_sounds.UiHover);
                _lastSelected = selected;
            }

            if (module == null) return;
            var position = module.point.action.ReadValue<Vector2>();
            if ((position - _lastPointer).sqrMagnitude > 1f)
            {
                _lastPointer = position;
                var hover = FindHovered(system, position);
                if (hover != _lastHover && hover != null) Play(_sounds.UiHover);
                _lastHover = hover;
            }

            if (module.leftClick.action.WasPressedThisFrame() && _lastHover != null && _lastHover.interactable)
                Play(CueFor(_lastHover));
            else if (module.submit.action.WasPressedThisFrame() && selected != null &&
                     selected.TryGetComponent(out Selectable pressed) && pressed.interactable && pressed is Button)
                Play(CueFor(pressed));
        }

        private Selectable FindHovered(EventSystem system, Vector2 position)
        {
            _pointer ??= new PointerEventData(system);
            _pointer.position = position;
            _hits.Clear();
            system.RaycastAll(_pointer, _hits);
            foreach (var hit in _hits)
            {
                var selectable = hit.gameObject.GetComponentInParent<Selectable>();
                if (selectable != null) return selectable;
            }
            return null;
        }

        private AudioCue CueFor(Selectable control)
        {
            if (control.GetComponentInParent<DraftCardView>() != null) return _sounds.DraftPick;
            if (control is not Button) return _sounds.UiClick;
            var text = control.GetComponentInChildren<Text>(true);
            var label = (text != null ? text.text : control.name).ToLowerInvariant();
            foreach (var word in BackWords)
                if (label.Contains(word)) return _sounds.UiBack;
            foreach (var word in ConfirmWords)
                if (label.Contains(word)) return _sounds.UiConfirm;
            return _sounds.UiClick;
        }

        private static void Play(AudioCue cue)
        {
            if (cue != null) cue.Play();
        }
    }
}
