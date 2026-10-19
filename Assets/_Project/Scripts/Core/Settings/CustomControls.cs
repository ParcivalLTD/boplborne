namespace GraceRun.Core.Settings
{
    /// <summary>
    /// The player's own picks for <see cref="ControlScheme.Custom"/>: an index into
    /// <see cref="ControlChoices"/> per action, which ability slot slices, and whether keys aim at the cursor.
    /// </summary>
    public sealed class CustomControls
    {
        /// <summary>Choice index for the light attack.</summary>
        public int Attack;

        /// <summary>Choice index per ability slot.</summary>
        public readonly int[] Abilities = new int[ControlLayout.AbilitySlots];

        /// <summary>Ability slot (0-2) whose mouse button slices, or -1 for none. Only works when that ability is on a mouse button.</summary>
        public int Slice = -1;

        public bool CursorAimKeys = true;

        /// <summary>J to attack, abilities on the three mouse buttons, slice on Ability 1.</summary>
        public static CustomControls Default
        {
            get
            {
                var custom = new CustomControls { Attack = ControlChoices.IndexOf("<Keyboard>/j"), Slice = 0 };
                custom.Abilities[0] = ControlChoices.IndexOf("<Mouse>/leftButton");
                custom.Abilities[1] = ControlChoices.IndexOf("<Mouse>/middleButton");
                custom.Abilities[2] = ControlChoices.IndexOf("<Mouse>/rightButton");
                return custom;
            }
        }

        public CustomControls Clone()
        {
            var copy = new CustomControls { Attack = Attack, Slice = Slice, CursorAimKeys = CursorAimKeys };
            for (var i = 0; i < Abilities.Length; i++) copy.Abilities[i] = Abilities[i];
            return copy;
        }
    }
}
