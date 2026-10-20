using System.Collections.Generic;
using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// The settings page, shared by the main menu and the pause menu: master volume, sound effects,
    /// screen shake, film effects, display mode, resolution, vsync, reset to defaults and back. Every change is
    /// applied and saved at once (<see cref="GameSettings"/>).
    /// </summary>
    public static class SettingsPage
    {
        /// <summary>Builds the page under <paramref name="parent"/>; <paramref name="first"/> is the row selected on open.</summary>
        public static GameObject Build(Transform parent, MenuPages pages, out Selectable first)
        {
            var defaults = GameSettings.Defaults;
            var step = defaults != null ? defaults.Step : 0.1f;
            var maxShake = defaults != null ? defaults.MaxScreenShake : 2f;
            var rows = new List<MenuOptionRow>();

            var controls = ControlsSettingsPage.Build(parent, pages, out var controlsFirst);
            pages.Add(controls, controlsFirst);

            var column = MenuKit.Column(parent, "Settings", MenuStyle.PanelWidth);
            MenuKit.Label(column, "SETTINGS", MenuStyle.HeadingSize, MenuStyle.Gold);
            rows.Add(MenuKit.Option(column, "Master volume", () => SettingsRules.Percent(GameSettings.MasterVolume),
                d => GameSettings.SetMasterVolume(SettingsRules.Step(GameSettings.MasterVolume, d, step, 0f, 1f))));
            rows.Add(MenuKit.Option(column, "Sound effects", () => SettingsRules.Percent(GameSettings.SfxVolume),
                d => GameSettings.SetSfxVolume(SettingsRules.Step(GameSettings.SfxVolume, d, step, 0f, 1f))));
            rows.Add(MenuKit.Option(column, "Screen shake", () => SettingsRules.Percent(GameSettings.ShakeMultiplier),
                d => GameSettings.SetShake(SettingsRules.Step(GameSettings.ShakeMultiplier, d, step, 0f, maxShake))));
            rows.Add(MenuKit.Option(column, "Film effects", () => SettingsRules.OnOff(GameSettings.FilmEffects),
                _ => GameSettings.SetFilmEffects(!GameSettings.FilmEffects)));
            rows.Add(MenuKit.Option(column, "Display", () => GameSettings.Fullscreen ? "Fullscreen" : "Windowed",
                _ => GameSettings.SetFullscreen(!GameSettings.Fullscreen)));
            rows.Add(MenuKit.Option(column, "Resolution", ResolutionText, CycleResolution));
            rows.Add(MenuKit.Option(column, "VSync", () => SettingsRules.OnOff(GameSettings.VSync),
                _ => GameSettings.SetVSync(!GameSettings.VSync)));

            MenuKit.Button(column, "Controls setup", () => pages.Open(controls));
            MenuKit.Button(column, "Reset to defaults", () =>
            {
                GameSettings.ResetToDefaults();
                foreach (var row in rows) row.Refresh();
            });
            MenuKit.Button(column, "Back", pages.Back);
            MenuKit.Label(column, "Left / right to change", MenuStyle.SmallSize, MenuStyle.Fog);
            MenuKit.ChainVertical(column);

            first = rows[0];
            return column.gameObject;
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
