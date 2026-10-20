using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Look of a player: the body in the player's colour and the hat that belongs to the slot.
    /// The body's sprite shader swaps P1's blue ramp for the player's ramp (real art);
    /// the renderer tint colours the white placeholder shapes until that art arrives (M8.2).
    /// </summary>
    public class PlayerVisuals : MonoBehaviour
    {
        private static readonly int RampTo0Id = Shader.PropertyToID("_RampTo0");
        private static readonly int RampTo1Id = Shader.PropertyToID("_RampTo1");
        private static readonly int RampTo2Id = Shader.PropertyToID("_RampTo2");

        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private SpriteRenderer _hat;

        /// <summary>Paints the body and picks the hat shape for player index 0-3.</summary>
        public void Apply(GameConfig config, int playerIndex)
        {
            var color = config.GetPlayerColor(playerIndex);
            _body.color = color;
            _hat.color = color;
            ApplyRamp(config.GetPlayerRamp(playerIndex));

            var hat = config.GetHatSprite(playerIndex);
            _hat.sprite = hat;
            _hat.enabled = hat != null;
        }

        private void ApplyRamp(PlayerRamp ramp)
        {
            // Read first: SpriteFlash keeps the flash amount in the same block.
            var block = new MaterialPropertyBlock();
            _body.GetPropertyBlock(block);
            block.SetColor(RampTo0Id, ramp.Dark);
            block.SetColor(RampTo1Id, ramp.Mid);
            block.SetColor(RampTo2Id, ramp.Light);
            _body.SetPropertyBlock(block);
        }
    }
}
