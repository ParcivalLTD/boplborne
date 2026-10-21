using GraceRun.UI.Skin;
using UnityEngine;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// Colours and sizes shared by every menu screen: ink text on cream panels (docs/art step 6).
    /// The old names stay (Bone = main text, Fog = secondary text, Gold = headings and values) so the pages keep working.
    /// </summary>
    public static class MenuStyle
    {
        public static readonly Color Backdrop = UiPalette.Dim;
        public static readonly Color Panel = Color.white;
        public static readonly Color Bone = UiPalette.Ink;
        public static readonly Color Fog = UiPalette.Sepia;
        public static readonly Color Gold = UiPalette.SepiaDark;
        public static readonly Color Danger = UiPalette.Brick;

        /// <summary>Text drawn straight on the dimmed game behind the panels.</summary>
        public static readonly Color OnBackdrop = UiPalette.Cream;

        public const int TitleSize = UiKit.DisplayBig;
        public const int HeadingSize = UiKit.DisplayMedium;
        public const int BodySize = UiKit.BodyNormal;
        public const int SmallSize = UiKit.BodySmall;
        public const float RowHeight = 64f;
        public const float PanelWidth = 760f;
        public const float Spacing = 10f;
        public const int Padding = 40;
    }
}
