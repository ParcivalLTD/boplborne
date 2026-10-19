using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// One settings row: a label, a value and two arrow buttons. Left / right (keys, stick, d-pad) or a
    /// click on an arrow change the value; up / down move between rows as usual; a click on the row
    /// or submit steps it forward. Built by <see cref="MenuKit.Option"/>.
    /// </summary>
    public class MenuOptionRow : Button
    {
        private Text _value;
        private Func<string> _read;
        private Action<int> _adjust;

        /// <summary>Hooks the row to its value: <paramref name="read"/> gives the text, <paramref name="adjust"/> moves it by ±1.</summary>
        public void Bind(Text value, Func<string> read, Action<int> adjust)
        {
            _value = value;
            _read = read;
            _adjust = adjust;
            onClick.AddListener(() => Adjust(1));
            Refresh();
        }

        /// <summary>Shows the current value again (after a reset or an outside change).</summary>
        public void Refresh()
        {
            if (_value != null && _read != null) _value.text = _read();
        }

        public override void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: Adjust(-1); eventData.Use(); return;
                case MoveDirection.Right: Adjust(1); eventData.Use(); return;
                default: base.OnMove(eventData); return;
            }
        }

        /// <summary>Moves the value by <paramref name="delta"/> (±1) and shows it.</summary>
        public void Adjust(int delta)
        {
            _adjust?.Invoke(delta);
            Refresh();
        }
    }
}
