using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>
    /// One sound event (swing, hit, dodge...): a few clip variations with random pitch, played
    /// with <see cref="Play"/>. Local only; every PC plays its own sounds from synced state.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Audio Cue", fileName = "AudioCue")]
    public class AudioCue : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.7f;
        [SerializeField, Tooltip("Random pitch between x and y.")] private Vector2 _pitchRange = new(0.95f, 1.05f);
        [SerializeField, Min(0f), Tooltip("Plays closer together than this are skipped, so stacked hits don't get loud.")]
        private float _minInterval = 0.03f;

        [System.NonSerialized] private float _lastPlayed = float.NegativeInfinity;

        /// <summary>Plays a random clip of this cue. Safe to call with no clips (silent).</summary>
        public void Play()
        {
            if (_clips == null || _clips.Length == 0) return;

            var now = Time.unscaledTime;
            if (now < _lastPlayed) _lastPlayed = float.NegativeInfinity; // new play session in the Editor
            if (now - _lastPlayed < _minInterval) return;
            _lastPlayed = now;

            var clip = _clips[Random.Range(0, _clips.Length)];
            AudioCuePlayer.Play(clip, _volume, Random.Range(_pitchRange.x, _pitchRange.y));
        }
    }
}
