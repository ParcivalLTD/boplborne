using UnityEngine;

namespace GraceRun.UI.Toon
{
    /// <summary>The colours a cartoon button comes in.</summary>
    public enum ToonTint : byte
    {
        Cream,
        Orange,
        Mint,
        Sky,
        Pink,
        Stone,
        Plum,
    }

    /// <summary>
    /// The cartoon UI sprites in one asset (<c>Resources/ToonTheme.asset</c>, filled by Grace Run → Build Toon UI):
    /// chunky candy buttons, posters, tickets, ribbons, badges, marquee bulbs, curtains, stars. Missing asset
    /// or sprite = plain UI (nothing breaks).
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Toon Theme", fileName = "ToonTheme")]
    public class ToonTheme : ScriptableObject
    {
        public const string ResourceName = "ToonTheme";

        [Header("Buttons (9-slice), indexed by ToonTint")]
        [SerializeField] private Sprite[] _buttons = new Sprite[7];
        [SerializeField] private Sprite _roundButton;
        [SerializeField] private Sprite _roundButtonOrange;

        [Header("Panels (9-slice)")]
        [SerializeField] private Sprite _poster;
        [SerializeField] private Sprite _posterPaper;
        [SerializeField] private Sprite _posterPlum;
        [SerializeField] private Sprite _ticket;

        [Header("Ribbons (3-slice), orange / mint / sky / pink / stone / cream")]
        [SerializeField] private Sprite[] _ribbons = new Sprite[6];

        [Header("Decor")]
        [SerializeField] private Sprite _badge;
        [SerializeField] private Sprite _badgeOrange;
        [SerializeField] private Sprite _bulbOn;
        [SerializeField] private Sprite _bulbOff;
        [SerializeField] private Sprite _star;
        [SerializeField] private Sprite _starOrange;
        [SerializeField] private Sprite _sparkle;
        [SerializeField] private Sprite _curtain;
        [SerializeField] private Sprite _valance;
        [SerializeField] private Sprite _spotlight;
        [SerializeField] private Sprite _stripes;
        [SerializeField] private Sprite _barFrame;
        [SerializeField, Tooltip("The Fists icon on the weapon rack (the other weapons use their own sprite).")]
        private Sprite _fistIcon;

        [Header("Mascot (the blue blob knight, idle frames)")]
        [SerializeField] private Sprite[] _mascotFrames;
        [SerializeField] private Sprite _mascotHat;
        [SerializeField, Tooltip("Where the hat sits on each mascot frame, in sprite pixels from the frame's bottom-left (one per frame).")]
        private Vector2[] _mascotHatPixels;

        private static ToonTheme _current;

        public static ToonTheme Current
        {
            get
            {
                if (_current == null) _current = Resources.Load<ToonTheme>(ResourceName);
                return _current;
            }
        }

        public Sprite Button(ToonTint tint) => _buttons != null && (int)tint < _buttons.Length ? _buttons[(int)tint] : null;
        public Sprite RoundButton => _roundButton;
        public Sprite RoundButtonOrange => _roundButtonOrange;
        public Sprite Poster => _poster;
        public Sprite PosterPaper => _posterPaper;
        public Sprite PosterPlum => _posterPlum;
        public Sprite Ticket => _ticket;
        public Sprite Ribbon(ToonTint tint) => tint switch
        {
            ToonTint.Orange => At(_ribbons, 0),
            ToonTint.Mint => At(_ribbons, 1),
            ToonTint.Sky => At(_ribbons, 2),
            ToonTint.Pink => At(_ribbons, 3),
            ToonTint.Cream => At(_ribbons, 5),
            _ => At(_ribbons, 4),
        };
        public Sprite Badge => _badge;
        public Sprite BadgeOrange => _badgeOrange;
        public Sprite BulbOn => _bulbOn;
        public Sprite BulbOff => _bulbOff;
        public Sprite Star => _star;
        public Sprite StarOrange => _starOrange;
        public Sprite Sparkle => _sparkle;
        public Sprite Curtain => _curtain;
        public Sprite Valance => _valance;
        public Sprite Spotlight => _spotlight;
        public Sprite Stripes => _stripes;
        public Sprite BarFrame => _barFrame;
        public Sprite FistIcon => _fistIcon;
        public Sprite[] MascotFrames => _mascotFrames;
        public Sprite MascotHat => _mascotHat;
        public Vector2[] MascotHatPixels => _mascotHatPixels;

        private static Sprite At(Sprite[] list, int index) => list != null && index < list.Length ? list[index] : null;
    }
}
