using System;

namespace GraceRun.Player
{
    /// <summary>
    /// Pure bookkeeping for mouse ability buttons: a press starts a hold, the release fires the
    /// ability, unless something claimed the hold first (the slice) or the press began over UI.
    /// Presses and releases are queued from input callbacks and handled in <see cref="Flush"/>.
    /// </summary>
    public class PointerPressTracker
    {
        private readonly bool[] _queuedDown;
        private readonly bool[] _queuedUp;
        private readonly bool[] _held;
        private readonly bool[] _claimed;

        public PointerPressTracker(int slots)
        {
            _queuedDown = new bool[slots];
            _queuedUp = new bool[slots];
            _held = new bool[slots];
            _claimed = new bool[slots];
        }

        public bool IsHeld(int slot) => _held[slot];

        public void QueueDown(int slot) => _queuedDown[slot] = true;

        public void QueueUp(int slot) => _queuedUp[slot] = true;

        /// <summary>The current hold of <paramref name="slot"/> no longer fires its ability on release.</summary>
        public void Claim(int slot)
        {
            if (_held[slot]) _claimed[slot] = true;
        }

        /// <summary>
        /// Handles queued presses, then queued releases. A press over UI is ignored. On release,
        /// <paramref name="released"/> runs first (it may still <see cref="Claim"/>), then
        /// <paramref name="fire"/> runs unless the hold was claimed.
        /// </summary>
        public void Flush(bool pointerOverUi, Action<int> pressed, Action<int> released, Action<int> fire)
        {
            for (var slot = 0; slot < _held.Length; slot++)
            {
                if (_queuedDown[slot])
                {
                    _queuedDown[slot] = false;
                    if (!pointerOverUi && !_held[slot])
                    {
                        _held[slot] = true;
                        _claimed[slot] = false;
                        pressed?.Invoke(slot);
                    }
                }

                if (!_queuedUp[slot]) continue;
                _queuedUp[slot] = false;
                if (!_held[slot]) continue;
                released?.Invoke(slot);
                _held[slot] = false;
                if (!_claimed[slot]) fire?.Invoke(slot);
                _claimed[slot] = false;
            }
        }
    }
}
