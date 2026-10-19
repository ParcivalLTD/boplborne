using System.Text;

namespace GraceRun.Core.Settings
{
    /// <summary>Pure logic of the control schemes: which paths each scheme binds, and the texts the settings screen shows.</summary>
    public static class ControlSchemeRules
    {
        private const string AttackKey = "<Keyboard>/j";
        private static readonly string[] AbilityKeys = { "<Keyboard>/k", "<Keyboard>/l", "<Keyboard>/semicolon" };
        private static readonly string[] AbilityButtons = { "<Mouse>/leftButton", "<Mouse>/middleButton", "<Mouse>/rightButton" };
        private static readonly string[] AbilityGamepad = { "North", "LB", "RB" };

        /// <summary>The bindings <paramref name="scheme"/> asks for (<paramref name="custom"/> only matters for Custom).</summary>
        public static ControlLayout Build(ControlScheme scheme, CustomControls custom)
        {
            switch (scheme)
            {
                case ControlScheme.KeyboardOnly:
                    return new ControlLayout(AttackKey, (string[])AbilityKeys.Clone(), Empty(), -1, false);
                case ControlScheme.MouseOnly:
                    return new ControlLayout(string.Empty, Empty(), (string[])AbilityButtons.Clone(), 0, true);
                case ControlScheme.Custom:
                    return BuildCustom(custom ?? CustomControls.Default);
                default:
                    return new ControlLayout(AttackKey, (string[])AbilityKeys.Clone(), (string[])AbilityButtons.Clone(), 0, true);
            }
        }

        /// <summary>
        /// A custom setup that behaves like <paramref name="layout"/> as closely as one choice per
        /// action allows (a key wins over a mouse button), so editing a preset starts from what you had.
        /// </summary>
        public static CustomControls CustomFrom(ControlLayout layout)
        {
            var custom = new CustomControls
            {
                Attack = ControlChoices.IndexOf(layout.AttackPath),
                Slice = layout.SliceSlot,
                CursorAimKeys = layout.CursorAimKeys,
            };
            for (var i = 0; i < ControlLayout.AbilitySlots; i++)
            {
                var path = layout.KeyPaths[i].Length > 0 ? layout.KeyPaths[i] : layout.PointerPaths[i];
                custom.Abilities[i] = ControlChoices.IndexOf(path);
            }
            return custom;
        }

        /// <summary>Next choice index after <paramref name="delta"/> steps, wrapping.</summary>
        public static int CycleChoice(int index, int delta) => SettingsRules.Cycle(index, delta, ControlChoices.Count);

        /// <summary>Next slice slot after <paramref name="delta"/> steps through Off, Ability 1, 2, 3.</summary>
        public static int CycleSlice(int slice, int delta) =>
            SettingsRules.Cycle(slice + 1, delta, ControlLayout.AbilitySlots + 1) - 1;

        public static string SchemeLabel(ControlScheme scheme) => scheme switch
        {
            ControlScheme.KeyboardOnly => "Keyboard only",
            ControlScheme.MouseOnly => "Mouse only",
            ControlScheme.Custom => "Custom",
            _ => "Keyboard + mouse",
        };

        public static string SchemeHint(ControlScheme scheme) => scheme switch
        {
            ControlScheme.KeyboardOnly => "J attacks, K / L / ; use abilities, aimed the way you face.",
            ControlScheme.MouseOnly => "Drag with left mouse to slice. Mouse buttons use abilities, aimed at the cursor.",
            ControlScheme.Custom => "Your own keys and buttons. Change any row below.",
            _ => "Both at once: keys, and the mouse with the slice.",
        };

        public static string AttackLabel(ControlLayout layout) => Label(layout.AttackPath);

        /// <summary>"K / Left Mouse" for a slot bound to both, or "None".</summary>
        public static string AbilityLabel(ControlLayout layout, int slot)
        {
            var key = layout.KeyPaths[slot];
            var pointer = layout.PointerPaths[slot];
            if (key.Length > 0 && pointer.Length > 0) return Label(key) + " / " + Label(pointer);
            return key.Length > 0 ? Label(key) : Label(pointer);
        }

        public static string SliceLabel(int slice) => slice < 0 ? "Off" : "Ability " + (slice + 1) + " button";

        /// <summary>The controls list shown on the Controls page for <paramref name="layout"/>.</summary>
        public static string Describe(ControlLayout layout)
        {
            var text = new StringBuilder();
            Line(text, "Move", "A / D   ·   left stick");
            Line(text, "Jump · air jump", "Space   ·   South");
            Line(text, "Wall slide · wall jump", "push into a wall in the air, then Jump");
            Line(text, "Fast fall", "hold S while falling");
            Line(text, "Drop through a ledge", "S + Space");
            Line(text, "Dodge roll (any direction)", "Shift   ·   East");
            Line(text, "Light attack", AttackLine(layout) + "   ·   West");
            Line(text, "Slice", layout.SliceSlot >= 0
                ? "hold " + Label(layout.PointerPaths[layout.SliceSlot]) + " and drag across a target"
                : "off");
            for (var i = 0; i < ControlLayout.AbilitySlots; i++)
                Line(text, "Ability " + (i + 1), AbilityLabel(layout, i) + "   ·   " + AbilityGamepad[i]);
            Line(text, "Flask", "R");
            Line(text, "Revive · interact", "hold E");
            Line(text, "Menu", "Esc   ·   Start");
            return text.ToString().TrimEnd('\n');
        }

        private static string AttackLine(ControlLayout layout)
        {
            if (layout.AttackPath.Length == 0) return "None";
            var aim = layout.CursorAimKeys || ControlChoices.IsPointer(layout.AttackPath) ? "aims at the cursor" : "aims the way you face";
            return Label(layout.AttackPath) + " (" + aim + ")";
        }

        private static ControlLayout BuildCustom(CustomControls custom)
        {
            var keys = new string[ControlLayout.AbilitySlots];
            var pointers = new string[ControlLayout.AbilitySlots];
            for (var i = 0; i < keys.Length; i++)
            {
                var path = ControlChoices.PathOf(custom.Abilities[i]);
                var pointer = ControlChoices.IsPointer(path);
                keys[i] = pointer ? string.Empty : path;
                pointers[i] = pointer ? path : string.Empty;
            }
            var slice = custom.Slice >= 0 && custom.Slice < pointers.Length && pointers[custom.Slice].Length > 0 ? custom.Slice : -1;
            return new ControlLayout(ControlChoices.PathOf(custom.Attack), keys, pointers, slice, custom.CursorAimKeys);
        }

        private static string[] Empty() => new[] { string.Empty, string.Empty, string.Empty };

        private static string Label(string path) => ControlChoices.LabelFor(path);

        private static void Line(StringBuilder text, string action, string keys) =>
            text.Append("<color=#FFC94A>").Append(action).Append("</color>    ").Append(keys).Append('\n');
    }
}
