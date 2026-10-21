using UnityEngine;

namespace GraceRun.UI.Skin
{
    /// <summary>
    /// The vintage cinema colours of every screen (docs/art/grace-run.gpl): ink for lines and text,
    /// cream and paper for panels, sepia for secondary text and accents. Telegraph red and weak-point gold
    /// are not here: they belong to gameplay.
    /// </summary>
    public static class UiPalette
    {
        public static readonly Color Ink = new Color32(18, 15, 24, 255);
        public static readonly Color Cream = new Color32(242, 232, 213, 255);
        public static readonly Color Paper = new Color32(233, 220, 192, 255);
        public static readonly Color Sepia = new Color32(122, 92, 62, 255);
        public static readonly Color SepiaDark = new Color32(59, 42, 30, 255);
        public static readonly Color Brass = new Color32(184, 146, 90, 255);
        public static readonly Color Stone = new Color32(74, 66, 86, 255);

        /// <summary>Defeat and warnings on cream: a dark brick, not the gameplay telegraph red.</summary>
        public static readonly Color Brick = new Color32(150, 52, 44, 255);

        /// <summary>A dimmed overlay behind a panel (ink at 78%).</summary>
        public static readonly Color Dim = new Color(0.071f, 0.059f, 0.094f, 0.78f);
    }
}
