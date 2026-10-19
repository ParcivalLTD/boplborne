using UnityEngine;

namespace GraceRun.Core.Audio
{
    /// <summary>
    /// The play helper behind <see cref="AudioCue"/>: a few 2D audio sources on a hidden,
    /// persistent object, created on first use. Does nothing in batch mode (server-only builds).
    /// </summary>
    public static class AudioCuePlayer
    {
        private const int VoiceCount = 16;

        private static AudioSource[] _voices;
        private static int _next;

        /// <summary>Plays a clip on a free voice, or on the oldest one when all are busy.</summary>
        public static void Play(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || Application.isBatchMode) return;
            if (_voices == null || _voices[0] == null) CreateVoices();

            var voice = _voices[_next];
            for (var i = 0; i < VoiceCount; i++)
            {
                var candidate = _voices[(_next + i) % VoiceCount];
                if (candidate.isPlaying) continue;
                voice = candidate;
                _next = (_next + i) % VoiceCount;
                break;
            }
            _next = (_next + 1) % VoiceCount;

            voice.clip = clip;
            voice.volume = volume * Settings.GameSettings.SfxVolume;
            voice.pitch = pitch;
            voice.Play();
        }

        private static void CreateVoices()
        {
            var holder = new GameObject("AudioCuePlayer") { hideFlags = HideFlags.HideInHierarchy };
            Object.DontDestroyOnLoad(holder);
            _voices = new AudioSource[VoiceCount];
            for (var i = 0; i < VoiceCount; i++)
            {
                var source = holder.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                _voices[i] = source;
            }
            _next = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _voices = null;
            _next = 0;
        }
    }
}
