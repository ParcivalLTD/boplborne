using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Core.Settings
{
    /// <summary>
    /// The player's own settings on this PC: volumes, screen shake, film effects, display mode,
    /// resolution and vsync. Saved in PlayerPrefs and applied at once. Local only, never networked; a server-only
    /// build leaves the screen alone. Until <see cref="EnsureLoaded"/> runs every value is neutral (1).
    /// </summary>
    public static class GameSettings
    {
        private const string MasterKey = "settings.masterVolume";
        private const string MusicKey = "settings.musicVolume";
        private const string SfxKey = "settings.sfxVolume";
        private const string ShakeKey = "settings.screenShake";
        private const string FilmKey = "settings.filmEffects";
        private const string FullscreenKey = "settings.fullscreen";
        private const string VSyncKey = "settings.vsync";
        private const string WidthKey = "settings.width";
        private const string HeightKey = "settings.height";
        private const string NicknameKey = "settings.nickname";
        private const string SchemeKey = "settings.controlScheme";
        private const string CustomAttackKey = "settings.custom.attack";
        private const string CustomHeavyKey = "settings.custom.heavy";
        private const string CustomAbilityKey = "settings.custom.ability";
        private const string CustomSliceKey = "settings.custom.slice";
        private const string CustomMouseKey = "settings.custom.mouse";
        private const string AimAssistKey = "settings.aimAssist";
        private const string HoldToAimKey = "settings.holdToAim";

        private static readonly List<Vector2Int> ResolutionList = new();
        private static SettingsData _defaults;
        private static ControlLayout _layout;

        /// <summary>Raised after any value changes.</summary>
        public static event Action Changed;

        public static float MasterVolume { get; private set; } = 1f;
        public static float MusicVolume { get; private set; } = 1f;
        public static float SfxVolume { get; private set; } = 1f;
        public static float ShakeMultiplier { get; private set; } = 1f;
        public static bool FilmEffects { get; private set; } = true;
        public static bool Fullscreen { get; private set; } = true;
        public static bool VSync { get; private set; } = true;

        /// <summary>Chosen resolution; (0, 0) means the current screen size.</summary>
        public static Vector2Int Resolution { get; private set; }

        /// <summary>The name shown for this player online; empty to be shown as P1-P4. Never longer than the nickname limit.</summary>
        public static string Nickname { get; private set; } = string.Empty;

        public static ControlScheme Scheme { get; private set; } = ControlScheme.KeyboardOnly;

        /// <summary>Keyboard and gamepad abilities snap onto the enemy nearest to your aim (see AimAssistData).</summary>
        public static bool AimAssist { get; private set; } = true;

        /// <summary>Keyboard and gamepad abilities fire when the button is released, with an aim line while held.</summary>
        public static bool HoldToAim { get; private set; }

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
            Audio.AudioLevels.Bind(defaults.Mixer);
            MasterVolume = PlayerPrefs.GetFloat(MasterKey, defaults.MasterVolume);
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, defaults.MusicVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, defaults.SfxVolume);
            ShakeMultiplier = PlayerPrefs.GetFloat(ShakeKey, defaults.ScreenShake);
            FilmEffects = PlayerPrefs.GetInt(FilmKey, defaults.FilmEffects ? 1 : 0) == 1;
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, defaults.Fullscreen ? 1 : 0) == 1;
            VSync = PlayerPrefs.GetInt(VSyncKey, defaults.VSync ? 1 : 0) == 1;
            Resolution = new Vector2Int(PlayerPrefs.GetInt(WidthKey, 0), PlayerPrefs.GetInt(HeightKey, 0));
            Nickname = GraceRun.Net.NicknameRules.Sanitize(PlayerPrefs.GetString(NicknameKey, string.Empty));
            AimAssist = PlayerPrefs.GetInt(AimAssistKey, 1) == 1;
            HoldToAim = PlayerPrefs.GetInt(HoldToAimKey, 0) == 1;
            LoadControls();
            Apply(true);
        }

        public static void SetMasterVolume(float value) => Change(() => MasterVolume = Mathf.Clamp01(value));
        public static void SetMusicVolume(float value) => Change(() => MusicVolume = Mathf.Clamp01(value));
        public static void SetSfxVolume(float value) => Change(() => SfxVolume = Mathf.Clamp01(value));
        public static void SetShake(float value) => Change(() => ShakeMultiplier = Mathf.Max(0f, value));
        public static void SetFilmEffects(bool on) => Change(() => FilmEffects = on);
        public static void SetVSync(bool on) => Change(() => VSync = on);
        public static void SetFullscreen(bool on) => Change(() => Fullscreen = on, true);
        public static void SetResolution(Vector2Int size) => Change(() => Resolution = size, true);

        /// <summary>Sets the nickname (cleaned up; empty means "shown as P1-P4"). Takes effect the next time you join.</summary>
        public static void SetNickname(string value) =>
            Change(() => Nickname = GraceRun.Net.NicknameRules.Sanitize(value));

        public static void SetAimAssist(bool on) => Change(() => AimAssist = on);
        public static void SetHoldToAim(bool on) => Change(() => HoldToAim = on);

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
                MusicVolume = _defaults.MusicVolume;
                SfxVolume = _defaults.SfxVolume;
                ShakeMultiplier = _defaults.ScreenShake;
                FilmEffects = _defaults.FilmEffects;
                Fullscreen = _defaults.Fullscreen;
                VSync = _defaults.VSync;
                Resolution = Vector2Int.zero;
                Scheme = ControlScheme.KeyboardOnly;
                Custom = CustomControls.Default;
                AimAssist = true;
                HoldToAim = false;
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
            var mixed = Audio.AudioLevels.HasMixer;
            AudioListener.volume = mixed ? 1f : MasterVolume;
            if (mixed) Audio.AudioLevels.Apply(MasterVolume, MusicVolume, SfxVolume);
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
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
            PlayerPrefs.SetFloat(ShakeKey, ShakeMultiplier);
            PlayerPrefs.SetInt(FilmKey, FilmEffects ? 1 : 0);
            PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(VSyncKey, VSync ? 1 : 0);
            PlayerPrefs.SetInt(WidthKey, Resolution.x);
            PlayerPrefs.SetInt(HeightKey, Resolution.y);
            PlayerPrefs.SetString(NicknameKey, Nickname);
            PlayerPrefs.SetInt(SchemeKey, (int)Scheme);
            PlayerPrefs.SetInt(CustomAttackKey, Custom.Attack);
            PlayerPrefs.SetInt(CustomHeavyKey, Custom.Heavy);
            for (var i = 0; i < Custom.Abilities.Length; i++) PlayerPrefs.SetInt(CustomAbilityKey + i, Custom.Abilities[i]);
            PlayerPrefs.SetInt(CustomSliceKey, Custom.Slice);
            PlayerPrefs.SetInt(CustomMouseKey, Custom.Mouse ? 1 : 0);
            PlayerPrefs.SetInt(AimAssistKey, AimAssist ? 1 : 0);
            PlayerPrefs.SetInt(HoldToAimKey, HoldToAim ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static void LoadControls()
        {
            var scheme = PlayerPrefs.GetInt(SchemeKey, (int)ControlScheme.KeyboardOnly);
            Scheme = Enum.IsDefined(typeof(ControlScheme), scheme) ? (ControlScheme)scheme : ControlScheme.KeyboardOnly;

            var custom = CustomControls.DefaultFor(PlayerPrefs.GetInt(CustomMouseKey, 0) == 1);
            custom.Attack = ClampChoice(PlayerPrefs.GetInt(CustomAttackKey, custom.Attack));
            custom.Heavy = ClampChoice(PlayerPrefs.GetInt(CustomHeavyKey, custom.Heavy));
            for (var i = 0; i < custom.Abilities.Length; i++)
                custom.Abilities[i] = ClampChoice(PlayerPrefs.GetInt(CustomAbilityKey + i, custom.Abilities[i]));
            custom.Slice = Mathf.Clamp(PlayerPrefs.GetInt(CustomSliceKey, custom.Slice), -1, ControlLayout.AbilitySlots - 1);
            Custom = custom;
            _layout = null;
        }

        private static int ClampChoice(int index) => index >= 0 && index < ControlChoices.Count ? index : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _defaults = null;
            ResolutionList.Clear();
            MasterVolume = MusicVolume = SfxVolume = ShakeMultiplier = 1f;
            FilmEffects = true;
            Nickname = string.Empty;
            Scheme = ControlScheme.KeyboardOnly;
            Custom = CustomControls.Default;
            _layout = null;
            Changed = null;
        }
    }
}
