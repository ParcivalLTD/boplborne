using GraceRun.Core.Settings;
using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>
    /// Plays one looping music track at a time and crossfades to the next. Two sources swap roles.
    /// Local cosmetic only; stays silent in batch mode (server builds). Driven by AudioDirector.
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private float _crossfadeSeconds = 1.6f;

        private AudioSource[] _sources;
        private float[] _targets;
        private float[] _levels;
        private int _current;

        /// <summary>The clip that is (or is fading) in, or null.</summary>
        public AudioClip CurrentClip { get; private set; }

        /// <summary>The loudness the current track fades to (before the Music slider).</summary>
        public float CurrentVolume { get; private set; }

        private void Awake()
        {
            if (Application.isBatchMode)
            {
                enabled = false;
                return;
            }
            _sources = new AudioSource[2];
            _targets = new float[2];
            _levels = new float[2];
            for (var i = 0; i < 2; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;
                source.spatialBlend = 0f;
                source.volume = 0f;
                _sources[i] = source;
            }
        }

        private void Update()
        {
            var step = Time.unscaledDeltaTime / Mathf.Max(0.05f, _crossfadeSeconds);
            var gain = AudioLevels.HasMixer ? 1f : GameSettings.MusicVolume;
            for (var i = 0; i < 2; i++)
            {
                var source = _sources[i];
                if (source.clip == null) continue;
                if (AudioLevels.HasMixer && source.outputAudioMixerGroup == null) source.outputAudioMixerGroup = AudioLevels.MusicGroup;
                _levels[i] = Mathf.MoveTowards(_levels[i], _targets[i], step);
                source.volume = _levels[i] * gain;
                if (_targets[i] <= 0f && _levels[i] <= 0f && source.isPlaying) source.Stop();
            }
        }

        /// <summary>Sets how long crossfades take.</summary>
        public void SetCrossfade(float seconds) => _crossfadeSeconds = seconds;

        /// <summary>Crossfades to <paramref name="track"/>; an empty clip fades the music out. The same clip again does nothing.</summary>
        public void Play(MusicTrack track)
        {
            if (!enabled || _sources == null || track.Clip == CurrentClip) return;
            CurrentClip = track.Clip;
            CurrentVolume = track.Volume;
            _targets[_current] = 0f;
            if (track.Clip == null) return;

            _current = 1 - _current;
            var next = _sources[_current];
            next.clip = track.Clip;
            next.volume = 0f;
            _levels[_current] = 0f;
            next.outputAudioMixerGroup = AudioLevels.MusicGroup;
            _targets[_current] = track.Volume;
            next.Play();
        }
    }
}
