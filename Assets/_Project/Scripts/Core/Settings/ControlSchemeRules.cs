using System.Text;

namespace GraceRun.Core.Settings
{
    /// <summary>Pure logic of the control schemes: which paths each scheme binds, and the texts the settings screen shows.</summary>
    public static class ControlSchemeRules
    {
        private const string AttackKey = "<Keyboard>/j";
        private const string HeavyKey = "<Keyboard>/u";
        private const string HeavyButton = "<Mouse>/forwardButton";
        private static readonly string[] AbilityKeys = { "<Keyboard>/k", "<Keyboard>/l", "<Keyboard>/semicolon" };
        private static readonly string[] AbilityButtons = { "<Mouse>/leftButton", "<Mouse>/middleButton", "<Mouse>/rightButton" };
        private static readonly string[] AbilityGamepad = { "North", "LB", "RB" };

        /// <summary>The bindings <paramref name="scheme"/> asks for (<paramref name="custom"/> only matters for Custom).</summary>
        public static ControlLayout Build(ControlScheme scheme, CustomControls custom)
        {
            switch (scheme)
            {
                case ControlScheme.KeyboardOnly:
                    return new ControlLayout(AttackKey, HeavyKey, string.Empty, (string[])AbilityKeys.Clone(), Empty(), -1, false);
                case ControlScheme.MouseOnly:
                    return new ControlLayout(string.Empty, string.Empty, HeavyButton, Empty(), (string[])AbilityButtons.Clone(), 0, true);
                case ControlScheme.Custom:
                    return BuildCustom(custom ?? CustomControls.Default);
                default:
                    return new ControlLayout(AttackKey, HeavyKey, string.Empty, (string[])AbilityKeys.Clone(), Empty(), -1, false);
            }
        }

        /// <summary>
        /// A custom setup that starts from <paramref name="layout"/>, so editing a preset keeps what you had.
        /// The device is the mouse when the layout has no keys at all.
        /// </summary>
        public static CustomControls CustomFrom(ControlLayout layout)
        {
            var mouse = layout.AttackPath.Length == 0 && layout.HeavyKeyPath.Length == 0 && layout.KeyPaths[0].Length == 0;
            var custom = new CustomControls
            {
                Mouse = mouse,
                Attack = ControlChoices.IndexOf(layout.AttackPath),
                Heavy = ControlChoices.IndexOf(mouse ? layout.HeavyPointerPath : layout.HeavyKeyPath),
                Slice = mouse ? layout.SliceSlot : -1,
            };
            for (var i = 0; i < ControlLayout.AbilitySlots; i++)
                custom.Abilities[i] = ControlChoices.IndexOf(mouse ? layout.PointerPaths[i] : layout.KeyPaths[i]);
            return custom;
        }

        /// <summary>Next choice on the same device (None always fits) after <paramref name="delta"/> steps, wrapping.</summary>
        public static int CycleChoice(int index, int delta, bool mouse)
        {
            var next = index;
            for (var i = 0; i < ControlChoices.Count; i++)
            {
                next = SettingsRules.Cycle(next, delta, ControlChoices.Count);
                if (ControlChoices.FitsDevice(next, mouse)) return next;
            }
            return index;
        }

        /// <summary>Next slice slot after <paramref name="delta"/> steps through Off, Ability 1, 2, 3.</summary>
        public static int CycleSlice(int slice, int delta) =>
            SettingsRules.Cycle(slice + 1, delta, ControlLayout.AbilitySlots + 1) - 1;

        public static string SchemeLabel(ControlScheme scheme) => scheme switch
        {
            ControlScheme.MouseOnly => "Mouse only",
            ControlScheme.Custom => "Custom",
            _ => "Keyboard only",
        };

        public static string SchemeHint(ControlScheme scheme) => scheme switch
        {
            ControlScheme.KeyboardOnly => "J attacks, U heavy, K / L / ; use abilities, aimed the way you face.",
            ControlScheme.MouseOnly => "Drag with left mouse to slice, side button for heavy. Mouse buttons use abilities, aimed at the cursor.",
            ControlScheme.Custom => "Your own keys or mouse buttons, never both. Change any row below.",
            _ => "Keys only. The mouse does nothing in combat.",
        };

        public static string AttackLabel(ControlLayout layout) => Label(layout.AttackPath);

        /// <summary>"U / Mouse Forward" for the heavy attack bound to both, or "None".</summary>
        public static string HeavyLabel(ControlLayout layout) => BothLabel(layout.HeavyKeyPath, layout.HeavyPointerPath);

        /// <summary>"K / Left Mouse" for a slot bound to both, or "None".</summary>
        public static string AbilityLabel(ControlLayout layout, int slot)
        {
            return BothLabel(layout.KeyPaths[slot], layout.PointerPaths[slot]);
        }

        private static string BothLabel(string key, string pointer)
        {
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
            Line(text, "Light attack · aerial · up / down", AttackLine(layout) + "   ·   West");
            Line(text, "Heavy attack", HeavyLabel(layout) + "   ·   Right trigger");
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
            var keys = Empty();
            var pointers = Empty();
            for (var i = 0; i < keys.Length; i++)
            {
                var path = DevicePath(custom.Abilities[i], custom.Mouse);
                if (custom.Mouse) pointers[i] = path;
                else keys[i] = path;
            }
            var slice = custom.Mouse && custom.Slice >= 0 && custom.Slice < pointers.Length && pointers[custom.Slice].Length > 0 ? custom.Slice : -1;
            var heavy = DevicePath(custom.Heavy, custom.Mouse);
            return new ControlLayout(DevicePath(custom.Attack, custom.Mouse), custom.Mouse ? string.Empty : heavy,
                custom.Mouse ? heavy : string.Empty, keys, pointers, slice, custom.Mouse);
        }

        /// <summary>The path of a choice, or empty when it belongs to the other device (an old save cannot mix them).</summary>
        private static string DevicePath(int choice, bool mouse) => ControlChoices.FitsDevice(choice, mouse) ? ControlChoices.PathOf(choice) : string.Empty;

        /// <summary>The device label shown for a custom scheme.</summary>
        public static string DeviceLabel(bool mouse) => mouse ? "Mouse" : "Keyboard";


        private static string[] Empty() => new[] { string.Empty, string.Empty, string.Empty };

        private static string Label(string path) => ControlChoices.LabelFor(path);

        private static void Line(StringBuilder text, string action, string keys) =>
            text.Append("<color=#FFC94A>").Append(action).Append("</color>    ").Append(keys).Append('\n');
    }
}
