using GraceRun.Player;
using UnityEngine;

namespace GraceRun.UI
{
    /// <summary>
    /// The bottom HUD on every PC: one <see cref="PlayerPanel"/> per player slot, shown only while
    /// a player holds that slot, so P1 is always leftmost. Finds players through
    /// <see cref="PlayerIdentity.All"/>, which works on clients as well as the server.
    /// </summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField, Tooltip("One panel per slot, in slot order (P1 first).")]
        private PlayerPanel[] _panels;

        private void LateUpdate()
        {
            for (var slot = 0; slot < _panels.Length; slot++)
            {
                var panel = _panels[slot];
                panel.Bind(FindPlayer(slot));
                panel.Refresh();
            }
        }

        private static PlayerIdentity FindPlayer(int slot)
        {
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
                if (players[i].PlayerIndex == slot) return players[i];
            return null;
        }
    }
}
