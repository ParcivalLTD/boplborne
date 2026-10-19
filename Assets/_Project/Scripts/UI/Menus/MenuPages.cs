using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// Shows one menu page at a time and remembers the way back: <see cref="Open"/> pushes a page,
    /// Cancel (Esc, gamepad B) or <see cref="Back"/> pops it. Keeps a button selected so keyboard and
    /// gamepad navigation always has somewhere to start, even after the mouse clicked empty space.
    /// </summary>
    public class MenuPages : MonoBehaviour
    {
        private readonly Stack<GameObject> _stack = new();
        private readonly Dictionary<GameObject, Selectable> _first = new();
        private InputSystemUIInputModule _module;
        private int _openedFrame = -1;

        /// <summary>Raised when Back is used on the bottom page (the pause menu closes itself then).</summary>
        public event Action BackedOutOfRoot;

        /// <summary>The page on screen, or null.</summary>
        public GameObject Current => _stack.Count > 0 ? _stack.Peek() : null;

        /// <summary>Frame of the last Back (Cancel), so the same key press is not also read as Pause.</summary>
        public int LastBackFrame { get; private set; } = -1;

        /// <summary>Registers a page and the control selected when it opens. Pages start hidden.</summary>
        public void Add(GameObject page, Selectable first)
        {
            _first[page] = first;
            page.SetActive(false);
        }

        /// <summary>Hides every page and starts over with <paramref name="page"/>.</summary>
        public void Reset(GameObject page)
        {
            while (_stack.Count > 0) _stack.Pop().SetActive(false);
            Open(page);
        }

        public void Open(GameObject page)
        {
            _openedFrame = Time.frameCount;
            if (Current != null) Current.SetActive(false);
            _stack.Push(page);
            page.SetActive(true);
            Select(page);
        }

        public void Back()
        {
            LastBackFrame = Time.frameCount;
            if (_stack.Count <= 1)
            {
                BackedOutOfRoot?.Invoke();
                return;
            }
            _stack.Pop().SetActive(false);
            Current.SetActive(true);
            Select(Current);
        }

        /// <summary>Hides everything (the pause menu closing).</summary>
        public void Close()
        {
            while (_stack.Count > 0) _stack.Pop().SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnDisable() => Unhook();

        private void Update()
        {
            var system = EventSystem.current;
            if (system == null || Current == null) return;
            Hook(system);
            var selected = system.currentSelectedGameObject;
            if (selected == null || !selected.activeInHierarchy) Select(Current);
        }

        private void Hook(EventSystem system)
        {
            if (_module != null) return;
            _module = system.GetComponent<InputSystemUIInputModule>();
            if (_module != null && _module.cancel != null) _module.cancel.action.performed += OnCancel;
        }

        private void Unhook()
        {
            if (_module != null && _module.cancel != null) _module.cancel.action.performed -= OnCancel;
            _module = null;
        }

        private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            if (Current != null && isActiveAndEnabled && Time.frameCount != _openedFrame) Back();
        }

        private void Select(GameObject page)
        {
            if (EventSystem.current == null || !_first.TryGetValue(page, out var first) || first == null) return;
            EventSystem.current.SetSelectedGameObject(first.gameObject);
        }
    }
}
