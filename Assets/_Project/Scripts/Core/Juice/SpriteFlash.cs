using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Turns a set of sprites solid white, either for a moment (<see cref="Flash"/>) or for as
    /// long as something holds it (<see cref="SetHeld"/>). Local cosmetic only.
    /// Drives <c>_FlashAmount</c> of the GraceRun/Sprite Ramp Flash shader through a material
    /// property block, so the renderer colour (player tint, ghost, enrage) is never touched.
    /// </summary>
    public class SpriteFlash : MonoBehaviour
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        [SerializeField] private SpriteRenderer[] _renderers;
        [SerializeField] private Color _flashColor = Color.white;

        private MaterialPropertyBlock _block;
        private bool _held;
        private float _timeLeft;
        private bool _showing;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_timeLeft <= 0f) return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) Refresh();
        }

        private void OnDisable()
        {
            _held = false;
            _timeLeft = 0f;
            Refresh();
        }

        /// <summary>Shows the flash for <paramref name="duration"/> seconds.</summary>
        public void Flash(float duration)
        {
            if (!isActiveAndEnabled) return;
            _timeLeft = Mathf.Max(_timeLeft, duration);
            Refresh();
        }

        /// <summary>Keeps the flash on until called again with false.</summary>
        public void SetHeld(bool held)
        {
            if (!isActiveAndEnabled) return;
            _held = held;
            Refresh();
        }

        private void Refresh()
        {
            var show = _held || _timeLeft > 0f;
            if (show == _showing) return;
            _showing = show;

            foreach (var sprite in _renderers)
            {
                if (sprite == null) continue;
                // Read first: PlayerVisuals keeps the player's ramp in the same block.
                sprite.GetPropertyBlock(_block);
                _block.SetColor(FlashColorId, _flashColor);
                _block.SetFloat(FlashAmountId, show ? 1f : 0f);
                sprite.SetPropertyBlock(_block);
            }
        }
    }
}
