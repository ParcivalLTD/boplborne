namespace GraceRun.Core.Settings
{
    /// <summary>
    /// The player's own picks for <see cref="ControlScheme.Custom"/>: an index into
    /// <see cref="ControlChoices"/> per action (all keys or all mouse buttons), and which ability slot slices.
    /// </summary>
    public sealed class CustomControls
    {
        /// <summary>True when every action is on the mouse, false when every action is on the keyboard. Never both.</summary>
        public bool Mouse;

        /// <summary>Choice index for the light attack.</summary>
        public int Attack;

        /// <summary>Choice index for the heavy attack (a key or a mouse button).</summary>
        public int Heavy;

        /// <summary>Choice index per ability slot.</summary>
        public readonly int[] Abilities = new int[ControlLayout.AbilitySlots];

        /// <summary>Ability slot (0-2) whose mouse button slices, or -1 for none. Only works when that ability is on a mouse button.</summary>
        public int Slice = -1;

        /// <summary>Keyboard: J to attack, U for heavy, K / L / ; for abilities.</summary>
        public static CustomControls Default => DefaultFor(false);

        /// <summary>The starting choices for one device: keys (J, U, K, L, ;) or mouse (no attack key, Mouse Forward, the three buttons, slice on Ability 1).</summary>
        public static CustomControls DefaultFor(bool mouse)
        {
            var custom = new CustomControls { Mouse = mouse };
            custom.Attack = mouse ? 0 : ControlChoices.IndexOf("<Keyboard>/j");
            custom.Heavy = ControlChoices.IndexOf(mouse ? "<Mouse>/forwardButton" : "<Keyboard>/u");
            custom.Slice = mouse ? 0 : -1;
            string[] paths = mouse
                ? new[] { "<Mouse>/leftButton", "<Mouse>/middleButton", "<Mouse>/rightButton" }
                : new[] { "<Keyboard>/k", "<Keyboard>/l", "<Keyboard>/semicolon" };
            for (var i = 0; i < custom.Abilities.Length; i++) custom.Abilities[i] = ControlChoices.IndexOf(paths[i]);
            return custom;
        }

        /// <summary>Switches device and starts that device's choices over.</summary>
        public void SetMouse(bool mouse)
        {
            if (mouse == Mouse) return;
            var fresh = DefaultFor(mouse);
            Mouse = mouse;
            Attack = fresh.Attack;
            Heavy = fresh.Heavy;
            Slice = fresh.Slice;
            for (var i = 0; i < Abilities.Length; i++) Abilities[i] = fresh.Abilities[i];
        }

        public CustomControls Clone()
        {
            var copy = new CustomControls { Mouse = Mouse, Attack = Attack, Heavy = Heavy, Slice = Slice };
            for (var i = 0; i < Abilities.Length; i++) copy.Abilities[i] = Abilities[i];
            return copy;
        }
    }
}
