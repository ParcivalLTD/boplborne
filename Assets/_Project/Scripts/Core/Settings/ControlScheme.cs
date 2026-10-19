namespace GraceRun.Core.Settings
{
    /// <summary>How attacks and abilities are controlled (Settings → Controls).</summary>
    public enum ControlScheme
    {
        /// <summary>Keyboard and mouse at once: J / K / L / ; and the mouse with the slice. The default.</summary>
        Mixed,
        /// <summary>Keys only: J attacks, K / L / ; use abilities, aimed by facing. The mouse does nothing in combat.</summary>
        KeyboardOnly,
        /// <summary>Mouse only: left-drag slice for melee, left / middle / right mouse for abilities, aimed at the cursor.</summary>
        MouseOnly,
        /// <summary>Your own choice per action.</summary>
        Custom,
    }
}
