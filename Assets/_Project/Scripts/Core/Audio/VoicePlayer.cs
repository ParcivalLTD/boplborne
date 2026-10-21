using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>
    /// Plays one voice over line at a time on its own 2D source (a new line cuts the old one). While a line
    /// plays, <see cref="MusicPlayer"/> ducks the music. Local cosmetic only; silent in batch mode.
    /// </summary>
    public static class VoicePlayer
    {
        private static AudioSource _source;

        /// <summary>True while a line is playing.</summary>
        public static bool IsPlaying => _source != null && _source.isPlaying;

        /// <summary>Plays <paramref name="clip"/> at <paramref name="volume"/> (before the SFX slider); null just stops the current line.</summary>
        public static void Play(AudioClip clip, float volume)
        {
            if (Application.isBatchMode) return;
            Stop();
            if (clip == null) return;
            if (_source == null) Create();
            if (AudioLevels.HasMixer) _source.outputAudioMixerGroup = AudioLevels.SfxGroup;
            _source.clip = clip;
            _source.volume = AudioLevels.HasMixer ? volume : volume * Settings.GameSettings.SfxVolume;
            _source.Play();
        }

        /// <summary>Cuts the current line.</summary>
        public static void Stop()
        {
            if (_source != null) _source.Stop();
        }

        private static void Create()
        {
            var holder = new GameObject("VoicePlayer") { hideFlags = HideFlags.HideInHierarchy };
            Object.DontDestroyOnLoad(holder);
            _source = holder.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _source = null;
    }
}
