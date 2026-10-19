using System.Collections.Generic;
using GraceRun.Bosses;
using GraceRun.Combat;
using GraceRun.Player;
using UnityEngine;

namespace GraceRun.Core.Framing
{
    /// <summary>
    /// Moves the arena Cinemachine camera to frame the living players (and a nearby boss), keeps the
    /// local player on screen, and clamps the view to the current <see cref="ArenaBounds"/>.
    /// Local presentation only: nothing here is networked, and a server without a camera never runs it.
    /// Cinemachine still applies pixel snapping and screen shake on top of this transform.
    /// </summary>
    public class ArenaCameraRig : MonoBehaviour
    {
        [SerializeField] private CameraFollowData _data;

        private readonly List<Vector2> _focus = new List<Vector2>(8);
        private readonly Dictionary<PlayerIdentity, Health> _health = new Dictionary<PlayerIdentity, Health>();
        private Camera _camera;
        private Vector2 _velocity;
        private bool _snap = true;

        /// <summary>Camera centre this rig is aiming at, for tests and the debug menu.</summary>
        public Vector2 Position => transform.position;

        private void OnEnable()
        {
            ArenaBounds.CurrentChanged += OnArenaChanged;
            _snap = true;
        }

        private void OnDisable() => ArenaBounds.CurrentChanged -= OnArenaChanged;

        private void Update()
        {
            if (Application.isBatchMode) return;
            var arena = ArenaBounds.Current;
            if (arena == null) return;
            if (_camera == null) _camera = Camera.main;
            if (_camera == null || !_camera.orthographic) return;

            var half = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
            var current = (Vector2)transform.position;
            var local = GatherFocus();
            var target = ArenaFraming.Solve(_focus, local, half, arena.CameraArea, _data.LocalMargin, current);

            if (_snap || (target - current).sqrMagnitude > _data.SnapDistance * _data.SnapDistance)
            {
                current = target;
                _velocity = Vector2.zero;
                _snap = false;
            }
            else
            {
                current = Vector2.SmoothDamp(current, target, ref _velocity, _data.SmoothTime);
                if (local.HasValue) current = ArenaFraming.KeepInside(current, local.Value, half, _data.GuardMargin);
                current = ArenaFraming.ClampToBounds(current, half, arena.CameraArea);
            }

            transform.position = new Vector3(current.x, current.y, transform.position.z);
        }

        private void OnArenaChanged(ArenaBounds arena) => _snap = true;

        /// <summary>Fills <see cref="_focus"/> and returns the local player's position, if there is one.</summary>
        private Vector2? GatherFocus()
        {
            _focus.Clear();
            Vector2? local = null;
            var anyAlive = false;
            var players = PlayerIdentity.All;

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null) continue;
                if (player.IsLocalPlayer) local = player.transform.position;
                if (!IsDead(player)) anyAlive = true;
            }

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null) continue;
                if (anyAlive && IsDead(player)) continue;
                _focus.Add(player.transform.position);
            }

            if (local.HasValue) AddNearbyBosses(local.Value);
            return local;
        }

        private void AddNearbyBosses(Vector2 local)
        {
            var bosses = BossBrain.All;
            var range = _data.BossFocusRange * _data.BossFocusRange;
            for (var i = 0; i < bosses.Count; i++)
            {
                var boss = bosses[i];
                if (boss == null) continue;
                var position = (Vector2)boss.transform.position;
                if ((position - local).sqrMagnitude <= range) _focus.Add(position);
            }
        }

        private bool IsDead(PlayerIdentity player)
        {
            if (!_health.TryGetValue(player, out var health))
            {
                health = player.GetComponent<Health>();
                _health[player] = health;
            }
            return health != null && health.IsDead;
        }
    }
}
