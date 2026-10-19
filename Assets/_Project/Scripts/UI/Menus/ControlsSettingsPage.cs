using System;
using System.Collections.Generic;
using GraceRun.Core.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Menus
{
    /// <summary>
    /// Settings → Controls setup: pick a control scheme (keyboard + mouse, keyboard only, mouse only,
    /// custom) and, below it, the keys and buttons each action uses. The rows always show what the
    /// current scheme binds; changing any row switches to Custom and keeps everything else as it was.
    /// </summary>
    public static class ControlsSettingsPage
    {
        public static GameObject Build(Transform parent, MenuPages pages, out Selectable first)
        {
            var column = MenuKit.Column(parent, "ControlsSetup", MenuStyle.PanelWidth + 80f);
            MenuKit.Label(column, "CONTROLS SETUP", MenuStyle.HeadingSize, MenuStyle.Gold);
            var hint = MenuKit.Label(column, string.Empty, MenuStyle.SmallSize, MenuStyle.Fog);
            var rows = new List<MenuOptionRow>();

            var scheme = MenuKit.Option(column, "Control scheme", () => ControlSchemeRules.SchemeLabel(GameSettings.Scheme),
                delta => GameSettings.SetScheme((ControlScheme)SettingsRules.Cycle((int)GameSettings.Scheme, delta, Enum.GetValues(typeof(ControlScheme)).Length)));
            rows.Add(scheme);

            rows.Add(MenuKit.Option(column, "Attack", () => ControlSchemeRules.AttackLabel(GameSettings.Layout),
                delta => GameSettings.EditCustom(c => c.Attack = ControlSchemeRules.CycleChoice(c.Attack, delta))));
            for (var i = 0; i < ControlLayout.AbilitySlots; i++)
            {
                var slot = i;
                rows.Add(MenuKit.Option(column, "Ability " + (slot + 1), () => ControlSchemeRules.AbilityLabel(GameSettings.Layout, slot),
                    delta => GameSettings.EditCustom(c => c.Abilities[slot] = ControlSchemeRules.CycleChoice(c.Abilities[slot], delta))));
            }
            rows.Add(MenuKit.Option(column, "Slice (drag)", SliceText,
                delta => GameSettings.EditCustom(c => c.Slice = ControlSchemeRules.CycleSlice(c.Slice, delta))));
            rows.Add(MenuKit.Option(column, "Keys aim at cursor", () => SettingsRules.OnOff(GameSettings.Layout.CursorAimKeys),
                _ => GameSettings.EditCustom(c => c.CursorAimKeys = !c.CursorAimKeys)));

            MenuKit.Button(column, "Back", pages.Back);
            MenuKit.Label(column, "Left / right to change. Mouse buttons fire on release and aim at the cursor.", MenuStyle.SmallSize, MenuStyle.Fog);
            MenuKit.ChainVertical(column);

            column.gameObject.AddComponent<ControlsSettingsView>().Setup(hint, rows);
            first = scheme;
            return column.gameObject;
        }

        private static string SliceText()
        {
            var layout = GameSettings.Layout;
            if (GameSettings.Scheme == ControlScheme.Custom && GameSettings.Custom.Slice >= 0 && layout.SliceSlot < 0)
                return ControlSchemeRules.SliceLabel(GameSettings.Custom.Slice) + " (needs a mouse button)";
            return ControlSchemeRules.SliceLabel(layout.SliceSlot);
        }
    }
}
