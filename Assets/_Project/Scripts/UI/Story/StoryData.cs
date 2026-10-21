using System;
using UnityEngine;

namespace GraceRun.UI.Story
{
    /// <summary>One story card: heading, caption and picture (the picture may be missing; a flat panel is drawn instead).</summary>
    [Serializable]
    public struct StoryCard
    {
        public string Title;
        public string Caption;
        public Sprite Picture;
        public AudioClip Voice;

        /// <summary>The words as read aloud and on the clock: heading, then caption.</summary>
        public readonly string Spoken => string.IsNullOrEmpty(Title) ? Caption : Title + ". " + Caption;
    }

    /// <summary>
    /// The words, timing and pictures of the story harness (docs/LORE.md): the opening cards, the ascent screen
    /// captions and tower art, the ending and defeat extras. All our own words. Lives in
    /// <c>Resources/StoryData.asset</c>, filled by Grace Run → Build Story Assets. Every sprite may be missing.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Story Data", fileName = "StoryData")]
    public class StoryData : ScriptableObject
    {
        public const string ResourceName = "StoryData";

        [Header("Opening (five cards in the hub)")]
        [SerializeField] private StoryCard[] _opening = Array.Empty<StoryCard>();

        [Header("Ascent captions: index 0 = floor 1 ... the last regular floor; the final floor has its own line")]
        [SerializeField, TextArea(1, 3)] private string[] _ascentCaptions = Array.Empty<string>();
        [SerializeField, TextArea(1, 3)] private string _finalAscentCaption = "The top. The crown is waiting.";
        [SerializeField, TextArea(1, 3), Tooltip("Shown with the Hollow's silhouette before the final fight.")]
        private string _twistLine = "";

        [Header("Voice over (tools/audio/make_voice.ps1, then Grace Run → Build Story Assets)")]
        [SerializeField] private AudioClip[] _ascentVoices = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip _finalAscentVoice;
        [SerializeField] private AudioClip _twistVoice;

        [Header("Ending and defeat extras")]
        [SerializeField] private StoryCard[] _ending = Array.Empty<StoryCard>();
        [SerializeField] private StoryCard _defeat;
        [SerializeField] private Sprite _hollowSilhouette;

        [Header("Tower art")]
        [SerializeField] private Sprite _tower;
        [SerializeField] private Sprite _windowLit;
        [SerializeField] private Sprite _windowDark;
        [SerializeField] private Sprite _lantern;
        [SerializeField, Tooltip("trophy_1 .. trophy_7: the trophy of the beaten floor with that number.")]
        private Sprite[] _trophies = Array.Empty<Sprite>();
        [SerializeField, Tooltip("Window centres in tower sprite pixels, x from the left and y from the TOP; index 0 = hub, 1.. = floors.")]
        private Vector2[] _windowCentres = Array.Empty<Vector2>();

        [Header("Card timing (seconds, real time)")]
        [SerializeField, Min(0.05f)] private float _cardSlideIn = 0.3f;
        [SerializeField, Min(0f)] private float _cardHold = 1.4f;
        [SerializeField, Min(0.05f)] private float _cardSlideOut = 0.3f;
        [SerializeField, Min(1f), Tooltip("Everything moves in steps of 1 / this many seconds, like held frames on a film.")]
        private float _framesPerSecond = 12f;

        [SerializeField, Min(0.5f), Tooltip("Reading speed that sets how long a card stays: words / this, plus the padding.")]
        private float _wordsPerSecond = 2f;
        [SerializeField, Min(0f), Tooltip("Seconds added after the words (or the voice over) end.")]
        private float _readPadding = 1.5f;

        [Header("Ascent timing (seconds, real time)")]
        [SerializeField, Min(0.05f)] private float _ascentSlideIn = 0.3f;
        [SerializeField, Min(0f)] private float _ascentHold = 4f;
        [SerializeField, Min(0.05f)] private float _ascentSlideOut = 0.3f;
        [SerializeField, Min(0f)] private float _lanternDelay = 0.5f;
        [SerializeField, Min(0.05f)] private float _lanternTravel = 1.5f;

        public StoryCard[] Opening => _opening;
        public string[] AscentCaptions => _ascentCaptions;
        public string FinalAscentCaption => _finalAscentCaption;
        public string TwistLine => _twistLine;
        public StoryCard[] Ending => _ending;
        public StoryCard Defeat => _defeat;
        public Sprite HollowSilhouette => _hollowSilhouette;
        public Sprite Tower => _tower;
        public Sprite WindowLit => _windowLit;
        public Sprite WindowDark => _windowDark;
        public Sprite Lantern => _lantern;
        public Vector2[] WindowCentres => _windowCentres;
        public float CardSlideIn => _cardSlideIn;
        public float CardHold => _cardHold;
        public float CardSlideOut => _cardSlideOut;
        public float WordsPerSecond => _wordsPerSecond;
        public float ReadPadding => _readPadding;
        public AudioClip FinalAscentVoice => _finalAscentVoice;
        public AudioClip TwistVoice => _twistVoice;
        public float CardSeconds => _cardSlideIn + _cardHold + _cardSlideOut;
        public float FramesPerSecond => _framesPerSecond;
        public float AscentSlideIn => _ascentSlideIn;
        public float AscentHold => _ascentHold;
        public float AscentSlideOut => _ascentSlideOut;
        public float LanternDelay => _lanternDelay;
        public float LanternTravel => _lanternTravel;

        /// <summary>The voice over of the ascent to <paramref name="floor"/> (same rule as <see cref="StoryRules.AscentCaption"/>), or null.</summary>
        public AudioClip AscentVoice(int floor, int totalFloors)
        {
            if (totalFloors > 0 && floor >= totalFloors) return _finalAscentVoice;
            if (_ascentVoices == null || _ascentVoices.Length == 0 || floor < 1) return null;
            return _ascentVoices[Mathf.Min(floor, _ascentVoices.Length) - 1];
        }

        /// <summary>The trophy sprite of beaten <paramref name="floor"/> (1-based), or null.</summary>
        public Sprite TrophyFor(int floor) =>
            _trophies != null && floor >= 1 && floor <= _trophies.Length ? _trophies[floor - 1] : null;

        /// <summary>Fills the data from the builder (editor use; keeps the fields private for everyone else).</summary>
        public void Configure(StoryCard[] opening, string[] ascent, string finalAscent, string twist, StoryCard[] ending, StoryCard defeat,
            Sprite silhouette, Sprite tower, Sprite lit, Sprite dark, Sprite lantern, Sprite[] trophies, Vector2[] windows)
        {
            _opening = opening;
            _ascentCaptions = ascent;
            _finalAscentCaption = finalAscent;
            _twistLine = twist;
            _ending = ending;
            _defeat = defeat;
            _hollowSilhouette = silhouette;
            _tower = tower;
            _windowLit = lit;
            _windowDark = dark;
            _lantern = lantern;
            _trophies = trophies;
            _windowCentres = windows;
        }
    }
}
