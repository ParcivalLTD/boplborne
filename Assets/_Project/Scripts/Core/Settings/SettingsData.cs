using UnityEngine;

namespace GraceRun.Core.Settings
{
    /// <summary>
    /// Defaults and limits for the player-facing settings screen. What each player picks is saved
    /// on their PC (see <see cref="GameSettings"/>); this asset is only the starting point.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Settings Data", fileName = "SettingsData")]
    public class SettingsData : ScriptableObject
    {
        [Header("Defaults")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 2f)] private float _screenShake = 1f;
        [SerializeField, Tooltip("Film grain, vignette, warm grade and dust (FilmLook).")]
        private bool _filmEffects = true;
        [SerializeField] private bool _fullscreen = true;
        [SerializeField] private bool _vSync = true;

        [Header("Audio")]
        [SerializeField, Tooltip("Master / Music / SFX groups with exposed MasterVolume, MusicVolume and SfxVolume.")]
        private UnityEngine.Audio.AudioMixer _mixer;

        [Header("Limits")]
        [SerializeField, Min(0.01f), Tooltip("How much one left / right press changes a volume or shake value.")]
        private float _step = 0.1f;
        [SerializeField, Min(0f), Tooltip("Highest screen shake the slider allows (1 = as designed).")]
        private float _maxScreenShake = 2f;

        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        public UnityEngine.Audio.AudioMixer Mixer => _mixer;
        public float ScreenShake => _screenShake;
        public bool FilmEffects => _filmEffects;
        public bool Fullscreen => _fullscreen;
        public bool VSync => _vSync;
        public float Step => _step;
        public float MaxScreenShake => _maxScreenShake;
    }
}
