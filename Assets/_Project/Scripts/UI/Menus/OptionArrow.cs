using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>The "&lt;" or "&gt;" of a <see cref="MenuOptionRow"/>: a click steps the row's value one way. Grows a little under the mouse.</summary>
    public class OptionArrow : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const float HoverScale = 1.15f;
        private const float PressedScale = 0.92f;

        private MenuOptionRow _row;
        private int _delta;
        private float _baseScaleX;

        public void Setup(MenuOptionRow row, int delta)
        {
            _row = row;
            _delta = delta;
            _baseScaleX = Mathf.Sign(transform.localScale.x);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) _row.Adjust(_delta);
        }

        public void OnPointerEnter(PointerEventData eventData) => Scale(HoverScale);

        public void OnPointerExit(PointerEventData eventData) => Scale(1f);

        public void OnPointerDown(PointerEventData eventData) => Scale(PressedScale);

        public void OnPointerUp(PointerEventData eventData) => Scale(HoverScale);

        private void OnDisable() => Scale(1f);

        private void Scale(float factor) => transform.localScale = new Vector3(_baseScaleX * factor, factor, 1f);
    }
}
