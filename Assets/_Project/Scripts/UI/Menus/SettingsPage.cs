using System.Collections.Generic;
using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// The settings page, shared by the main menu and the pause menu, laid out in titled sections on one
    /// cream panel: Sound (left), Look (left), Screen (right) and the controls setup button. Every change is
    /// applied and saved at once (<see cref="GameSettings"/>).
    /// To add the Master / Music / SFX sliders, replace the rows made in <see cref="AddSoundRows"/>; the
    /// Sound section grows downward and the panel resizes itself.
    /// </summary>
    public static class SettingsPage
    {
        private const float PanelWidth = 1560f;

        /// <summary>Builds the page under <paramref name="parent"/>; <paramref name="first"/> is the row selected on open.</summary>
        public static GameObject Build(Transform parent, MenuPages pages, out Selectable first)
        {
            var defaults = GameSettings.Defaults;
            var step = defaults != null ? defaults.Step : 0.1f;
            var maxShake = defaults != null ? defaults.MaxScreenShake : 2f;
            var rows = new List<MenuOptionRow>();

            var controls = ControlsSettingsPage.Build(parent, pages, out var controlsFirst);
            pages.Add(controls, controlsFirst);

            var column = MenuKit.Column(parent, "Settings", PanelWidth);
            MenuKit.Label(column, "SETTINGS", MenuStyle.HeadingSize, MenuStyle.Gold);
            var sections = MenuKit.Columns(column, "Sections", 64f);

            var left = MenuKit.Section(sections, "Sound");
            AddSoundRows(left, rows, step);
            var look = MenuKit.Section(left, "Look");
            rows.Add(MenuKit.Option(look, "Screen shake", () => SettingsRules.Percent(GameSettings.ShakeMultiplier),
                d => GameSettings.SetShake(SettingsRules.Step(GameSettings.ShakeMultiplier, d, step, 0f, maxShake))));
            rows.Add(MenuKit.Option(look, "Film effects", () => SettingsRules.OnOff(GameSettings.FilmEffects),
                _ => GameSettings.SetFilmEffects(!GameSettings.FilmEffects)));

            var right = MenuKit.Section(sections, "Screen");
            rows.Add(MenuKit.Option(right, "Display", () => GameSettings.Fullscreen ? "Fullscreen" : "Windowed",
                _ => GameSettings.SetFullscreen(!GameSettings.Fullscreen)));
            rows.Add(MenuKit.Option(right, "Resolution", ResolutionText, CycleResolution));
            rows.Add(MenuKit.Option(right, "VSync", () => SettingsRules.OnOff(GameSettings.VSync),
                _ => GameSettings.SetVSync(!GameSettings.VSync)));
            var controlsSection = MenuKit.Section(right, "Controls");
            MenuKit.Button(controlsSection, "Controls setup", () => pages.Open(controls));

            var bottom = MenuKit.Columns(column, "Buttons", 24f);
            MenuKit.Button(bottom, "Reset to defaults", () =>
            {
                GameSettings.ResetToDefaults();
                foreach (var row in rows) row.Refresh();
            });
            MenuKit.Button(bottom, "Back", pages.Back);
            MenuKit.Label(column, "Left / right to change", MenuStyle.SmallSize, MenuStyle.Fog);
            MenuKit.ChainVertical(column);

            first = rows[0];
            return column.gameObject;
        }

        /// <summary>The Sound section: Master / Music / Sound effects volume, each a bar stepped with left / right (saved at once, routed to the mixer).</summary>
        private static void AddSoundRows(Transform section, List<MenuOptionRow> rows, float step)
        {
            rows.Add(MenuKit.Option(section, "Master volume", () => VolumeBar(GameSettings.MasterVolume),
                d => GameSettings.SetMasterVolume(SettingsRules.Step(GameSettings.MasterVolume, d, step, 0f, 1f))));
            rows.Add(MenuKit.Option(section, "Music volume", () => VolumeBar(GameSettings.MusicVolume),
                d => GameSettings.SetMusicVolume(SettingsRules.Step(GameSettings.MusicVolume, d, step, 0f, 1f))));
            rows.Add(MenuKit.Option(section, "Sound effects", () => VolumeBar(GameSettings.SfxVolume),
                d => GameSettings.SetSfxVolume(SettingsRules.Step(GameSettings.SfxVolume, d, step, 0f, 1f))));
        }

        private static string VolumeBar(float volume)
        {
            const int cells = 10;
            var filled = Mathf.RoundToInt(Mathf.Clamp01(volume) * cells);
            return "[" + new string('#', filled) + new string('.', cells - filled) + "] " + SettingsRules.Percent(volume);
        }

        private static string ResolutionText()
        {
            var size = Current();
            return SettingsRules.ResolutionLabel(size.x, size.y);
        }

        private static void CycleResolution(int delta)
        {
            var list = GameSettings.Resolutions;
            var index = IndexOf(list, Current());
            GameSettings.SetResolution(list[SettingsRules.Cycle(index, delta, list.Count)]);
        }

        private static Vector2Int Current() =>
            GameSettings.Resolution == Vector2Int.zero ? new Vector2Int(Screen.width, Screen.height) : GameSettings.Resolution;

        private static int IndexOf(IReadOnlyList<Vector2Int> list, Vector2Int size)
        {
            for (var i = 0; i < list.Count; i++)
                if (list[i] == size) return i;
            return list.Count - 1;
        }
    }
}
