using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Clockwork Colossus Phase 2 hazard: powers the arena floor conveyor belt,
    /// shifting players on the main floor horizontally unless they climb onto elevated platforms.
    /// Direction periodically reverses over time.
    /// </summary>
    public class ClockworkConveyor : NetworkBehaviour
    {
        [SerializeField, Min(0f)] private float _baseSpeed = 3.5f;
        [SerializeField, Min(1f)] private float _switchInterval = 6.0f;
        [SerializeField] private float _floorTop = -5.0f;

        private readonly NetworkVariable<float> _currentSpeed = new(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _isActive = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float _timer;

        public float CurrentSpeed => _currentSpeed.Value;
        public bool IsActive => _isActive.Value;

        public void ServerSetActive(bool active)
        {
            if (!IsServer) return;
            _isActive.Value = active;
            if (!active)
            {
                _currentSpeed.Value = 0f;
                _timer = 0f;
            }
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || !_isActive.Value) return;

            _timer += Time.deltaTime;
            _currentSpeed.Value = ClockworkColossusRules.ConveyorVelocity(_baseSpeed, _timer, _switchInterval);
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !_isActive.Value) return;

            var speed = _currentSpeed.Value;
            if (Mathf.Abs(speed) < 0.01f) return;

            var deltaX = speed * Time.fixedDeltaTime;
            var ownedPlayers = PlayerMovement.AllOwned;

            for (var i = 0; i < ownedPlayers.Count; i++)
            {
                var player = ownedPlayers[i];
                if (player == null || !player.IsGrounded) continue;

                if (ClockworkColossusRules.IsOnFloor(player.transform.position.y, _floorTop))
                {
                    var body = player.GetComponent<Rigidbody2D>();
                    if (body != null)
                    {
                        body.position += new Vector2(deltaX, 0f);
                    }
                }
            }
        }
    }
}
