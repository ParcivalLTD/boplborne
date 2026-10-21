using UnityEngine;

namespace GraceRun.UI.Skin
{
    /// <summary>
    /// The UI look in one asset (<c>Resources/UiTheme.asset</c>, filled by Grace Run → Build UI): the
    /// display and body pixel fonts and the 9-slice panel, button, bar and slot sprites. Screens build
    /// their widgets in code from it through <see cref="UiKit"/>; a missing asset falls back to plain
    /// built-in UI so nothing breaks.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/UI Theme", fileName = "UiTheme")]
    public class UiTheme : ScriptableObject
    {
        public const string ResourceName = "UiTheme";

        [Header("Fonts")]
        [SerializeField, Tooltip("Slab-serif pixel capitals for titles; 16 px glyphs, use sizes 32, 48, 64, 96.")]
        private Font _display;
        [SerializeField, Tooltip("Typewriter pixel text for everything else; 16 px glyphs, use 32 (or 24 for small print).")]
        private Font _body;

        [Header("9-slice sprites (PPU 100, drawn at 2x)")]
        [SerializeField] private Sprite _panelCream;
        [SerializeField] private Sprite _panelPaper;
        [SerializeField] private Sprite _panelInk;
        [SerializeField] private Sprite _button;
        [SerializeField] private Sprite _barFrame;
        [SerializeField] private Sprite _slot;
        [SerializeField] private Sprite _field;
        [SerializeField] private Sprite _cardFrame;
        [SerializeField] private Sprite _titleCard;

        [Header("Plain sprites")]
        [SerializeField] private Sprite _arrow;
        [SerializeField] private Sprite _flaskPip;
        [SerializeField] private Sprite _divider;

        private static UiTheme _current;

        /// <summary>The theme in Resources, or null when none exists.</summary>
        public static UiTheme Current
        {
            get
            {
                if (_current == null) _current = Resources.Load<UiTheme>(ResourceName);
                return _current;
            }
        }

        public Font Display => _display;
        public Font Body => _body;
        public Sprite PanelCream => _panelCream;
        public Sprite PanelPaper => _panelPaper;
        public Sprite PanelInk => _panelInk;
        public Sprite Button => _button;
        public Sprite BarFrame => _barFrame;
        public Sprite Slot => _slot;
        public Sprite Field => _field;
        public Sprite CardFrame => _cardFrame;
        public Sprite TitleCard => _titleCard;
        public Sprite Arrow => _arrow;
        public Sprite FlaskPip => _flaskPip;
        public Sprite Divider => _divider;
    }
}
