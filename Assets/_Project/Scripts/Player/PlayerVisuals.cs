using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Placeholder look of a player: a rounded body in the player color with a white eye stripe,
    /// and the hat shape that belongs to the player slot. Real art arrives in M8.
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private SpriteRenderer _hat;

        /// <summary>Paints the body and picks the hat shape for player index 0-3.</summary>
        public void Apply(GameConfig config, int playerIndex)
        {
            var color = config.GetPlayerColor(playerIndex);
            _body.color = color;
            _hat.color = Color.white;

            var hat = config.GetHatSprite(playerIndex);
            _hat.sprite = hat;
            _hat.enabled = hat != null;
        }
    }
}
