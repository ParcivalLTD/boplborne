using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Core.Settings
{
    /// <summary>
    /// The player's own settings on this PC: volumes, screen shake, display mode, resolution and
    /// vsync. Saved in PlayerPrefs and applied at once. Local only, never networked; a server-only
    /// build leaves the screen alone. Until <see cref="EnsureLoaded"/> runs every value is neutral (1).
    /// </summary>
    public static class GameSettings
    {
        private const string MasterKey = "settings.masterVolume";
        private const string SfxKey = "settings.sfxVolume";
        private const string ShakeKey = "settings.screenShake";
        private const string FullscreenKey = "settings.fullscreen";
        private const string VSyncKey = "settings.vsync";
        private const string WidthKey = "settings.width";
        private const string HeightKey = "settings.height";

        private static readonly List<Vector2Int> ResolutionList = new();
        private static SettingsData _defaults;

        /// <summary>Raised after any value changes.</summary>
        public static event Action Changed;

        public static float MasterVolume { get; private set; } = 1f;
        public static float SfxVolume { get; private set; } = 1f;
        public static float ShakeMultiplier { get; private set; } = 1f;
        public static bool Fullscreen { get; private set; } = true;
        public static bool VSync { get; private set; } = true;

        /// <summary>Chosen resolution; (0, 0) means the current screen size.</summary>
        public static Vector2Int Resolution { get; private set; }

        public static SettingsData Defaults => _defaults;
        public static bool IsLoaded => _defaults != null;

        /// <summary>Distinct screen sizes this display supports, smallest first.</summary>
        public static IReadOnlyList<Vector2Int> Resolutions
        {
            get
            {
                if (ResolutionList.Count > 0) return ResolutionList;
                foreach (var r in Screen.resolutions)
                {
                    var size = new Vector2Int(r.width, r.height);
                    if (!ResolutionList.Contains(size)) ResolutionList.Add(size);
                }
                if (ResolutionList.Count == 0) ResolutionList.Add(new Vector2Int(Screen.width, Screen.height));
                return ResolutionList;
            }
        }

        /// <summary>Loads the saved values (or <paramref name="defaults"/>) once and applies them.</summary>
        public static void EnsureLoaded(SettingsData defaults)
        {
            if (IsLoaded || defaults == null) return;
            _defaults = defaults;
            MasterVolume = PlayerPrefs.GetFloat(MasterKey, defaults.MasterVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, defaults.SfxVolume);
            ShakeMultiplier = PlayerPrefs.GetFloat(ShakeKey, defaults.ScreenShake);
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, defaults.Fullscreen ? 1 : 0) == 1;
            VSync = PlayerPrefs.GetInt(VSyncKey, defaults.VSync ? 1 : 0) == 1;
            Resolution = new Vector2Int(PlayerPrefs.GetInt(WidthKey, 0), PlayerPrefs.GetInt(HeightKey, 0));
            Apply(true);
        }

        public static void SetMasterVolume(float value) => Change(() => MasterVolume = Mathf.Clamp01(value));
        public static void SetSfxVolume(float value) => Change(() => SfxVolume = Mathf.Clamp01(value));
        public static void SetShake(float value) => Change(() => ShakeMultiplier = Mathf.Max(0f, value));
        public static void SetVSync(bool on) => Change(() => VSync = on);
        public static void SetFullscreen(bool on) => Change(() => Fullscreen = on, true);
        public static void SetResolution(Vector2Int size) => Change(() => Resolution = size, true);

        /// <summary>Back to the SettingsData defaults (and the current screen size).</summary>
        public static void ResetToDefaults()
        {
            if (!IsLoaded) return;
            Change(() =>
            {
                MasterVolume = _defaults.MasterVolume;
                SfxVolume = _defaults.SfxVolume;
                ShakeMultiplier = _defaults.ScreenShake;
                Fullscreen = _defaults.Fullscreen;
                VSync = _defaults.VSync;
                Resolution = Vector2Int.zero;
            }, true);
        }

        private static void Change(Action set, bool screen = false)
        {
            set();
            Apply(screen);
            Save();
            Changed?.Invoke();
        }

        private static void Apply(bool screen)
        {
            AudioListener.volume = MasterVolume;
            if (Application.isBatchMode) return;
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            if (!screen || Application.isEditor) return;
            var mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            var size = Resolution == Vector2Int.zero ? new Vector2Int(Screen.width, Screen.height) : Resolution;
            Screen.SetResolution(size.x, size.y, mode);
        }

        private static void Save()
        {
            PlayerPrefs.SetFloat(MasterKey, MasterVolume);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
            PlayerPrefs.SetFloat(ShakeKey, ShakeMultiplier);
            PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(VSyncKey, VSync ? 1 : 0);
            PlayerPrefs.SetInt(WidthKey, Resolution.x);
            PlayerPrefs.SetInt(HeightKey, Resolution.y);
            PlayerPrefs.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _defaults = null;
            ResolutionList.Clear();
            MasterVolume = SfxVolume = ShakeMultiplier = 1f;
            Changed = null;
        }
    }
}
