using System;
using UnityEngine;

namespace GraceRun.UI.TitleCards
{
    /// <summary>
    /// The words and timing of the title cards shown before a fight (boss name and a tagline), after a
    /// win and after a defeat. Taglines are keyed by <c>BossData.Id</c>; a boss without an entry gets the
    /// fallback lines. All our own words. Lives in <c>Resources/TitleCards.asset</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Title Card Data", fileName = "TitleCards")]
    public class TitleCardData : ScriptableObject
    {
        public const string ResourceName = "TitleCards";

        [Serializable]
        public struct Entry
        {
            [Tooltip("BossData.Id, e.g. gatekeeper.")] public string BossId;
            [Tooltip("The line under the boss name before the fight.")] public string Tagline;
            [Tooltip("The line under the win card after this boss falls.")] public string VictoryTagline;
        }

        [Header("Before a fight")]
        [SerializeField] private string _fallbackTagline = "Somebody has to do it.";
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        [Header("After a win")]
        [SerializeField] private string _victoryTitle = "THAT'S A WRAP";
        [SerializeField] private string _fallbackVictoryTagline = "Scene cleared. Take a bow.";
        [SerializeField] private string _finalVictoryTitle = "CURTAIN CALL";
        [SerializeField] private string _finalVictoryTagline = "Every floor cleared. The house is on its feet.";

        [Header("After a defeat")]
        [SerializeField] private string _defeatTitle = "TAKE TWO";
        [SerializeField] private string _defeatTagline = "The reel runs out. Roll it back and go again.";

        [Header("Timing (seconds, real time)")]
        [SerializeField, Min(0.05f)] private float _slideIn = 0.35f;
        [SerializeField, Min(0f)] private float _hold = 1.3f;
        [SerializeField, Min(0.05f)] private float _slideOut = 0.3f;
        [SerializeField, Min(1f), Tooltip("The card moves in steps of 1 / this many seconds, like held frames on a film.")]
        private float _framesPerSecond = 12f;

        public string VictoryTitle => _victoryTitle;
        public string FinalVictoryTitle => _finalVictoryTitle;
        public string FinalVictoryTagline => _finalVictoryTagline;
        public string DefeatTitle => _defeatTitle;
        public string DefeatTagline => _defeatTagline;
        public float SlideIn => _slideIn;
        public float Hold => _hold;
        public float SlideOut => _slideOut;
        public float FramesPerSecond => _framesPerSecond;
        public float TotalSeconds => _slideIn + _hold + _slideOut;

        public string TaglineFor(string bossId)
        {
            var entry = Find(bossId);
            return entry.HasValue && !string.IsNullOrEmpty(entry.Value.Tagline) ? entry.Value.Tagline : _fallbackTagline;
        }

        public string VictoryTaglineFor(string bossId)
        {
            var entry = Find(bossId);
            return entry.HasValue && !string.IsNullOrEmpty(entry.Value.VictoryTagline) ? entry.Value.VictoryTagline : _fallbackVictoryTagline;
        }

        private Entry? Find(string bossId)
        {
            if (string.IsNullOrEmpty(bossId) || _entries == null) return null;
            foreach (var entry in _entries)
                if (entry.BossId == bossId) return entry;
            return null;
        }
    }
}
