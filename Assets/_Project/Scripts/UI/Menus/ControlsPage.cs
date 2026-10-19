using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>A read-only list of the controls (keyboard and mouse, then gamepad), opened from the menus.</summary>
    public static class ControlsPage
    {
        private static readonly (string action, string keys)[] Lines =
        {
            ("Move", "A / D   ·   left stick"),
            ("Jump · air jump", "Space   ·   South"),
            ("Wall slide · wall jump", "push into a wall in the air, then Jump"),
            ("Fast fall", "hold S while falling"),
            ("Drop through a ledge", "S + Space"),
            ("Dodge roll (any direction)", "Shift   ·   East"),
            ("Light attack", "J (aims at the cursor)   ·   West"),
            ("Slice", "hold left mouse and drag across a target"),
            ("Abilities 1 / 2 / 3", "left / middle / right mouse   ·   K / L / ;"),
            ("Flask", "R"),
            ("Revive · interact", "hold E"),
            ("Menu", "Esc   ·   Start"),
        };

        public static GameObject Build(Transform parent, MenuPages pages, out Selectable first)
        {
            var column = MenuKit.Column(parent, "Controls", MenuStyle.PanelWidth + 260f);
            MenuKit.Label(column, "CONTROLS", MenuStyle.HeadingSize, MenuStyle.Gold);
            foreach (var (action, keys) in Lines)
                MenuKit.Label(column, $"<color=#FFC94A>{action}</color>    {keys}", MenuStyle.SmallSize, MenuStyle.Bone, TextAnchor.MiddleLeft);
            first = MenuKit.Button(column, "Back", pages.Back);
            return column.gameObject;
        }
    }
}
