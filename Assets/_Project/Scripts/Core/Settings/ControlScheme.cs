namespace GraceRun.Core.Settings
{
    /// <summary>How attacks and abilities are controlled (Settings → Controls).</summary>
    public enum ControlScheme
    {
        // Values are saved in PlayerPrefs; 0 was the removed keyboard + mouse scheme and loads as KeyboardOnly.
        /// <summary>Keys only: J attacks, U heavy, K / L / ; use abilities, aimed by facing. The mouse does nothing in combat. The default.</summary>
        KeyboardOnly = 1,
        /// <summary>Mouse only: left-drag slice for melee, left / middle / right mouse for abilities, aimed at the cursor.</summary>
        MouseOnly = 2,
        /// <summary>Your own choice per action, all on one device (keyboard or mouse).</summary>
        Custom = 3,
    }
}
