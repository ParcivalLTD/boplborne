using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Stretches the player on take-off and squashes it on landing, then eases back.
    /// Local only: it never touches physics and never travels over the network.
    /// </summary>
    public class PlayerSquashStretch : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _data;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private Transform _visuals;

        private Vector2 _scale = Vector2.one;

        private void OnEnable()
        {
            _movement.Jumped += OnJumped;
            _movement.Landed += OnLanded;
        }

        private void OnDisable()
        {
            _movement.Jumped -= OnJumped;
            _movement.Landed -= OnLanded;
            _scale = Vector2.one;
            Apply();
        }

        private void Update()
        {
            if (_scale == Vector2.one) return;

            var step = Time.deltaTime / _data.SquashRecoverTime;
            _scale = Vector2.MoveTowards(_scale, Vector2.one, step);
            Apply();
        }

        private void OnJumped() => _scale = new Vector2(1f - _data.JumpStretch, 1f + _data.JumpStretch);

        private void OnLanded() => _scale = new Vector2(1f + _data.LandSquash, 1f - _data.LandSquash);

        private void Apply() => _visuals.localScale = new Vector3(_scale.x, _scale.y, 1f);
    }
}
