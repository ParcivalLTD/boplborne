using GraceRun.Core.Audio;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Tunable values of The Storm Wyrm's lightning columns (timing, size, damage and look).
    /// Lives in Data/Bosses/StormWyrm/LightningColumnData.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Bosses/Lightning Column Data", fileName = "LightningColumnData")]
    public class LightningColumnData : ScriptableObject
    {
        [Header("Timing (seconds)")]
        [SerializeField, Min(0.2f), Tooltip("Crackle on the spot before the bolt lands. Keep it a little under the attack's wind-up.")]
        private float _delay = 1.1f;
        [SerializeField, Min(0.05f)] private float _strikeTime = 0.3f;
        [SerializeField, Min(0f)] private float _fadeTime = 0.3f;

        [Header("Hit")]
        [SerializeField, Min(0.2f)] private float _width = 1.8f;
        [SerializeField, Min(1f)] private float _height = 30f;
        [SerializeField, Min(1)] private int _damage = 32;
        [SerializeField] private Vector2 _knockback = new(0f, 10f);

        [Header("Look")]
        [SerializeField, Min(0.02f)] private float _telegraphWidth = 0.14f;
        [SerializeField, Range(0f, 1f)] private float _telegraphAlpha = 0.55f;
        [SerializeField] private Color _telegraphColor = new(0.7f, 0.95f, 0.95f, 1f);
        [SerializeField] private Color _strikeColor = new(0.95f, 1f, 1f, 1f);

        [Header("Sound")]
        [SerializeField, Tooltip("Played on every PC when this column's bolt lands (local only). Empty = silent.")]
        private AudioCue _strikeCue;

        public AudioCue StrikeCue => _strikeCue;
        public float Delay => _delay;
        public float StrikeTime => _strikeTime;
        public float FadeTime => _fadeTime;
        public float Width => _width;
        public float Height => _height;
        public int Damage => _damage;
        public Vector2 Knockback => _knockback;
        public float TelegraphWidth => _telegraphWidth;
        public float TelegraphAlpha => _telegraphAlpha;
        public Color TelegraphColor => _telegraphColor;
        public Color StrikeColor => _strikeColor;
    }
}
