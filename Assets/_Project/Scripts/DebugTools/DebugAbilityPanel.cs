using GraceRun.Abilities;
using GraceRun.Player;
using UnityEngine;

namespace GraceRun.DebugTools
{
    /// <summary>
    /// The "give any ability to any player" section of the F1 menu: pick a player and a slot,
    /// then an ability from the catalog (or Clear). The change goes to the server through an RPC.
    /// </summary>
    public class DebugAbilityPanel
    {
        private const string PlayerPrefix = "P";
        private const int Columns = 2;

        private int _playerSlot;
        private int _abilitySlot;
        private PlayerIdentity _cachedPlayer;
        private AbilityLoadout _cachedLoadout;

        /// <summary>Draws the section with GUILayout; call from OnGUI.</summary>
        public void Draw()
        {
            GUILayout.Label("Give ability");
            DrawPlayerChoice();
            var loadout = SelectedLoadout();
            if (loadout == null || !loadout.IsSpawned) return;

            _abilitySlot = GUILayout.Toolbar(_abilitySlot, SlotLabels(loadout));
            var catalog = loadout.Catalog;
            for (var id = 0; id < catalog.Count; id += Columns)
            {
                GUILayout.BeginHorizontal();
                for (var column = 0; column < Columns && id + column < catalog.Count; column++)
                    if (GUILayout.Button(catalog.Get(id + column).DisplayName)) loadout.DebugSetRpc(_abilitySlot, id + column);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Clear slot")) loadout.DebugSetRpc(_abilitySlot, AbilityCatalog.None);
        }

        private void DrawPlayerChoice()
        {
            var players = PlayerIdentity.All;
            GUILayout.BeginHorizontal();
            for (var i = 0; i < players.Count; i++)
            {
                var index = players[i].PlayerIndex;
                if (index == PlayerIdentity.Unassigned) continue;
                var label = (index == _playerSlot ? "> " : "") + PlayerPrefix + (index + 1);
                if (GUILayout.Button(label)) _playerSlot = index;
            }
            GUILayout.EndHorizontal();
        }

        private AbilityLoadout SelectedLoadout()
        {
            var players = PlayerIdentity.All;
            PlayerIdentity selected = null;
            for (var i = 0; i < players.Count; i++)
                if (players[i].PlayerIndex == _playerSlot) selected = players[i];

            if (!ReferenceEquals(selected, _cachedPlayer))
            {
                _cachedPlayer = selected;
                _cachedLoadout = selected != null ? selected.GetComponent<AbilityLoadout>() : null;
            }
            return _cachedLoadout;
        }

        private static string[] SlotLabels(AbilityLoadout loadout)
        {
            var labels = new string[AbilityLoadout.SlotCount];
            for (var i = 0; i < labels.Length; i++)
            {
                var ability = loadout.Get(i);
                labels[i] = (i + 1) + ": " + (ability != null ? ability.DisplayName : "-");
            }
            return labels;
        }
    }
}
