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
        private const string NicknameKey = "settings.nickname";
        private const string SchemeKey = "settings.controlScheme";
        private const string CustomAttackKey = "settings.custom.attack";
        private const string CustomAbilityKey = "settings.custom.ability";
        private const string CustomSliceKey = "settings.custom.slice";
        private const string CustomAimKey = "settings.custom.aimKeys";

        private static readonly List<Vector2Int> ResolutionList = new();
        private static SettingsData _defaults;
        private static ControlLayout _layout;

        /// <summary>Raised after any value changes.</summary>
        public static event Action Changed;

        public static float MasterVolume { get; private set; } = 1f;
        public static float SfxVolume { get; private set; } = 1f;
        public static float ShakeMultiplier { get; private set; } = 1f;
        public static bool Fullscreen { get; private set; } = true;
        public static bool VSync { get; private set; } = true;

        /// <summary>Chosen resolution; (0, 0) means the current screen size.</summary>
        public static Vector2Int Resolution { get; private set; }

        /// <summary>The name shown for this player online; empty to be shown as P1-P4. Never longer than the nickname limit.</summary>
        public static string Nickname { get; private set; } = string.Empty;

        public static ControlScheme Scheme { get; private set; } = ControlScheme.Mixed;

        /// <summary>The player's own picks, used while <see cref="Scheme"/> is Custom (kept for later when it is not).</summary>
        public static CustomControls Custom { get; private set; } = CustomControls.Default;

        /// <summary>What attacks and abilities are bound to right now.</summary>
        public static ControlLayout Layout => _layout ??= ControlSchemeRules.Build(Scheme, Custom);

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
            Nickname = GraceRun.Net.NicknameRules.Sanitize(PlayerPrefs.GetString(NicknameKey, string.Empty));
            LoadControls();
            Apply(true);
        }

        public static void SetMasterVolume(float value) => Change(() => MasterVolume = Mathf.Clamp01(value));
        public static void SetSfxVolume(float value) => Change(() => SfxVolume = Mathf.Clamp01(value));
        public static void SetShake(float value) => Change(() => ShakeMultiplier = Mathf.Max(0f, value));
        public static void SetVSync(bool on) => Change(() => VSync = on);
        public static void SetFullscreen(bool on) => Change(() => Fullscreen = on, true);
        public static void SetResolution(Vector2Int size) => Change(() => Resolution = size, true);

        /// <summary>Sets the nickname (cleaned up; empty means "shown as P1-P4"). Takes effect the next time you join.</summary>
        public static void SetNickname(string value) =>
            Change(() => Nickname = GraceRun.Net.NicknameRules.Sanitize(value));

        public static void SetScheme(ControlScheme scheme) => Change(() => Scheme = scheme);

        /// <summary>
        /// Edits your custom controls with <paramref name="edit"/> and switches to Custom. When another
        /// scheme was active, the edit starts from what that scheme bound, so one change keeps the rest.
        /// </summary>
        public static void EditCustom(Action<CustomControls> edit)
        {
            Change(() =>
            {
                if (Scheme != ControlScheme.Custom) Custom = ControlSchemeRules.CustomFrom(Layout);
                edit(Custom);
                Scheme = ControlScheme.Custom;
            });
        }

        /// <summary>Back to the SettingsData defaults (and the current screen size). Keeps your nickname.</summary>
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
                Scheme = ControlScheme.Mixed;
                Custom = CustomControls.Default;
            }, true);
        }

        private static void Change(Action set, bool screen = false)
        {
            set();
            _layout = null;
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
            PlayerPrefs.SetString(NicknameKey, Nickname);
            PlayerPrefs.SetInt(SchemeKey, (int)Scheme);
            PlayerPrefs.SetInt(CustomAttackKey, Custom.Attack);
            for (var i = 0; i < Custom.Abilities.Length; i++) PlayerPrefs.SetInt(CustomAbilityKey + i, Custom.Abilities[i]);
            PlayerPrefs.SetInt(CustomSliceKey, Custom.Slice);
            PlayerPrefs.SetInt(CustomAimKey, Custom.CursorAimKeys ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static void LoadControls()
        {
            var scheme = PlayerPrefs.GetInt(SchemeKey, (int)ControlScheme.Mixed);
            Scheme = Enum.IsDefined(typeof(ControlScheme), scheme) ? (ControlScheme)scheme : ControlScheme.Mixed;

            var custom = CustomControls.Default;
            custom.Attack = ClampChoice(PlayerPrefs.GetInt(CustomAttackKey, custom.Attack));
            for (var i = 0; i < custom.Abilities.Length; i++)
                custom.Abilities[i] = ClampChoice(PlayerPrefs.GetInt(CustomAbilityKey + i, custom.Abilities[i]));
            custom.Slice = Mathf.Clamp(PlayerPrefs.GetInt(CustomSliceKey, custom.Slice), -1, ControlLayout.AbilitySlots - 1);
            custom.CursorAimKeys = PlayerPrefs.GetInt(CustomAimKey, custom.CursorAimKeys ? 1 : 0) == 1;
            Custom = custom;
            _layout = null;
        }

        private static int ClampChoice(int index) => index >= 0 && index < ControlChoices.Count ? index : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _defaults = null;
            ResolutionList.Clear();
            MasterVolume = SfxVolume = ShakeMultiplier = 1f;
            Nickname = string.Empty;
            Scheme = ControlScheme.Mixed;
            Custom = CustomControls.Default;
            _layout = null;
            Changed = null;
        }
    }
}
