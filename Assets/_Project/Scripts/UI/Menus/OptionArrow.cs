using UnityEngine;
using UnityEngine.EventSystems;

namespace GraceRun.UI.Menus
{
    /// <summary>The "&lt;" or "&gt;" of a <see cref="MenuOptionRow"/>: a click steps the row's value one way.</summary>
    public class OptionArrow : MonoBehaviour, IPointerClickHandler
    {
        private MenuOptionRow _row;
        private int _delta;

        public void Setup(MenuOptionRow row, int delta)
        {
            _row = row;
            _delta = delta;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) _row.Adjust(_delta);
        }
    }
}
