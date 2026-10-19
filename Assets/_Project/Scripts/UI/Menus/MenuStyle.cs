using UnityEngine;

namespace GraceRun.UI.Menus
{
    /// <summary>Colours and sizes shared by every menu screen (the night/stone/bone/gold palette of the result screen).</summary>
    public static class MenuStyle
    {
        public static readonly Color Backdrop = new(0.071f, 0.063f, 0.102f, 0.92f);
        public static readonly Color Panel = new(0.118f, 0.106f, 0.149f, 0.97f);
        public static readonly Color Stone = new(0.165f, 0.145f, 0.200f, 1f);
        public static readonly Color StoneLight = new(0.290f, 0.259f, 0.337f, 1f);
        public static readonly Color Fog = new(0.549f, 0.518f, 0.627f, 1f);
        public static readonly Color Bone = new(0.929f, 0.906f, 0.965f, 1f);
        public static readonly Color Gold = new(1.000f, 0.788f, 0.290f, 1f);
        public static readonly Color Danger = new(0.878f, 0.294f, 0.294f, 1f);

        public const int TitleSize = 72;
        public const int HeadingSize = 40;
        public const int BodySize = 26;
        public const int SmallSize = 20;
        public const float RowHeight = 56f;
        public const float PanelWidth = 640f;
        public const float Spacing = 10f;
        public const int Padding = 28;
    }
}
