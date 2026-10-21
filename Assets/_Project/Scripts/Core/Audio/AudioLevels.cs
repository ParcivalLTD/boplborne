using UnityEngine;
using UnityEngine.Audio;

namespace GraceRun.Core.Audio
{
    /// <summary>
    /// The AudioMixer behind Master / Music / SFX: finds the groups the sound players route to and
    /// writes the three volume sliders to the mixer's exposed parameters. Local only; without a
    /// mixer (tests, server builds) everything falls back to AudioListener and per-source volume.
    /// </summary>
    public static class AudioLevels
    {
        public const string MasterParameter = "MasterVolume";
        public const string MusicParameter = "MusicVolume";
        public const string SfxParameter = "SfxVolume";

        private static AudioMixer _mixer;

        public static AudioMixerGroup MusicGroup { get; private set; }
        public static AudioMixerGroup SfxGroup { get; private set; }

        /// <summary>True once a mixer with the Music and SFX groups is bound.</summary>
        public static bool HasMixer => _mixer != null && MusicGroup != null && SfxGroup != null;

        /// <summary>Remembers the mixer and its groups (called when the settings load).</summary>
        public static void Bind(AudioMixer mixer)
        {
            if (mixer == null || _mixer == mixer) return;
            _mixer = mixer;
            var music = mixer.FindMatchingGroups("Music");
            var sfx = mixer.FindMatchingGroups("SFX");
            MusicGroup = music.Length > 0 ? music[0] : null;
            SfxGroup = sfx.Length > 0 ? sfx[0] : null;
        }

        /// <summary>Sets the three mixer volumes from sliders (0..1).</summary>
        public static void Apply(float master, float music, float sfx)
        {
            if (_mixer == null) return;
            _mixer.SetFloat(MasterParameter, VolumeMath.SliderToDb(master));
            _mixer.SetFloat(MusicParameter, VolumeMath.SliderToDb(music));
            _mixer.SetFloat(SfxParameter, VolumeMath.SliderToDb(sfx));
        }

        /// <summary>Reads a mixer parameter back as a slider value (0..1), or -1 when there is no mixer.</summary>
        public static float ReadSlider(string parameter) =>
            _mixer != null && _mixer.GetFloat(parameter, out var db) ? VolumeMath.DbToSlider(db) : -1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _mixer = null;
            MusicGroup = null;
            SfxGroup = null;
        }
    }
}
