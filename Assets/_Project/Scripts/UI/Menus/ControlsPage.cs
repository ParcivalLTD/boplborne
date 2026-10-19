using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>A read-only list of the controls for the current control scheme, opened from the menus.</summary>
    public static class ControlsPage
    {
        public static GameObject Build(Transform parent, MenuPages pages, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Controls", MenuStyle.PanelWidth + 260f);
            MenuKit.Label(column, "CONTROLS", MenuStyle.HeadingSize, MenuStyle.Gold);
            var list = MenuKit.Label(column, string.Empty, MenuStyle.SmallSize, MenuStyle.Bone, TextAnchor.UpperLeft);
            list.lineSpacing = 1.25f;
            list.GetComponent<LayoutElement>().minHeight = 14 * MenuStyle.SmallSize * 1.25f;
            list.gameObject.AddComponent<ControlsTextView>();
            first = MenuKit.Button(column, "Back", pages.Back);
            return column.gameObject;
        }
    }
}
