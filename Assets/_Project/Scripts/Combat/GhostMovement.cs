using GraceRun.Player;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Owner only, while a ghost: floats freely in every direction with no gravity, and stays
    /// inside the camera view because the Ghost layer passes through walls and floor.
    /// <see cref="Ghost"/> enables it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class GhostMovement : MonoBehaviour
    {
        [SerializeField] private ReviveData _data;
        [SerializeField] private PlayerInputReader _input;

        private Rigidbody2D _body;
        private Camera _camera;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _camera = Camera.main;
            _body.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            var input = Vector2.ClampMagnitude(_input.Move, 1f);
            var wanted = input * _data.GhostSpeed;
            _body.linearVelocity = Vector2.MoveTowards(_body.linearVelocity, wanted, _data.GhostAcceleration * Time.fixedDeltaTime);
            KeepOnScreen();
        }

        private void KeepOnScreen()
        {
            if (_camera == null || !_camera.orthographic) return;
            var center = (Vector2)_camera.transform.position;
            var half = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
            half -= Vector2.one * _data.GhostScreenMargin;

            var position = _body.position;
            var clamped = new Vector2(
                Mathf.Clamp(position.x, center.x - half.x, center.x + half.x),
                Mathf.Clamp(position.y, center.y - half.y, center.y + half.y));
            if (clamped == position) return;

            _body.position = clamped;
            var velocity = _body.linearVelocity;
            if (!Mathf.Approximately(clamped.x, position.x)) velocity.x = 0f;
            if (!Mathf.Approximately(clamped.y, position.y)) velocity.y = 0f;
            _body.linearVelocity = velocity;
        }
    }
}
