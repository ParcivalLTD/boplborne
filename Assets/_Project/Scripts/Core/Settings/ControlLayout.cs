using System.Collections.Generic;

namespace GraceRun.Core.Settings
{
    /// <summary>
    /// What the attack and ability actions are bound to under the current control scheme: one
    /// attack path, a heavy-attack key and mouse button, and for each of the 3 ability slots a key path and a mouse-button path (either
    /// may be empty). Also which slot the mouse slice rides on and whether keyboard attacks aim at the cursor.
    /// </summary>
    public sealed class ControlLayout
    {
        public const int AbilitySlots = 3;

        public ControlLayout(string attackPath, string heavyKeyPath, string heavyPointerPath, string[] keyPaths,
            string[] pointerPaths, int sliceSlot, bool cursorAimKeys)
        {
            AttackPath = attackPath ?? string.Empty;
            HeavyKeyPath = heavyKeyPath ?? string.Empty;
            HeavyPointerPath = heavyPointerPath ?? string.Empty;
            KeyPaths = keyPaths;
            PointerPaths = pointerPaths;
            SliceSlot = sliceSlot;
            CursorAimKeys = cursorAimKeys;
        }

        /// <summary>Input System path of the light attack (a key or a mouse button), empty for none.</summary>
        public string AttackPath { get; }

        /// <summary>Input System path of the heavy attack's key, empty for none.</summary>
        public string HeavyKeyPath { get; }

        /// <summary>Input System path of the heavy attack's mouse button, empty for none.</summary>
        public string HeavyPointerPath { get; }

        /// <summary>Keyboard path per ability slot, empty for none.</summary>
        public IReadOnlyList<string> KeyPaths { get; }

        /// <summary>Mouse-button path per ability slot, empty for none. Mouse abilities fire on release and aim at the cursor.</summary>
        public IReadOnlyList<string> PointerPaths { get; }

        /// <summary>Ability slot (0-2) whose mouse button also slices when dragged, or -1 for no slice.</summary>
        public int SliceSlot { get; }

        /// <summary>Whether a keyboard attack aims at the mouse cursor (otherwise the way you face).</summary>
        public bool CursorAimKeys { get; }

        /// <summary>The layout the game shipped with (same as <see cref="ControlScheme.Mixed"/>).</summary>
        public static ControlLayout Default => ControlSchemeRules.Build(ControlScheme.Mixed, null);
    }
}
