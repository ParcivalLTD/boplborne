using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The mouse slice: hold the slice button (left mouse by default) and drag the cursor across a target, like a blade. Once the
    /// press becomes a drag it is claimed, so Ability 1 (also on left mouse) does not fire on
    /// release. Hits are detected on the owner's PC along the cursor path, inside the player's
    /// reach, one per target per stroke, and reported to the server like every player attack.
    /// Other PCs only see the slash (<see cref="StrokeRpc"/>).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class PlayerSlice : NetworkBehaviour
    {
        private const int MaxHitsPerCut = 8;

        [SerializeField] private SliceData _data;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerStamina _stamina;
        [SerializeField] private PlayerDodge _dodge;
        [SerializeField] private SliceView _view;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[MaxHitsPerCut];
        private readonly HashSet<Health> _alreadyHit = new();
        private readonly List<Vector2> _stroke = new();
        private ContactFilter2D _filter;
        private Health _health;
        private bool _pressing;
        private bool _stroking;
        private int _slot;
        private Vector2 _pressPoint;
        private Vector2 _last;
        private float _strokeTime;
        private float _readyAt;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.BossIndex) | Layers.Mask(Layers.PlayerIndex),
                useTriggers = false,
            };
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return;
            _input.PointerAbilityPressed += OnPressed;
            _input.PointerAbilityReleased += OnReleased;
        }

        public override void OnNetworkDespawn()
        {
            if (_input == null) return;
            _input.PointerAbilityPressed -= OnPressed;
            _input.PointerAbilityReleased -= OnReleased;
        }

        private void Update()
        {
            if (!IsOwner || !_pressing) return;
            if (_health.IsDead)
            {
                Stop();
                return;
            }
            if (_input.TryGetCursorWorld(out var cursor)) Follow(cursor);
        }

        private void OnPressed(int slot)
        {
            if (slot != _input.SliceSlot || _health.IsDead || !_input.TryGetCursorWorld(out _pressPoint)) return;
            _slot = slot;
            _pressing = true;
            _last = _pressPoint;
        }

        /// <summary>A flick between two frames still counts: the release gets one last look at the cursor.</summary>
        private void OnReleased(int slot)
        {
            if (slot != _slot || !_pressing) return;
            if (_input.TryGetCursorWorld(out var cursor)) Follow(cursor);
            Stop();
        }

        private void Follow(Vector2 cursor)
        {
            if (!_stroking)
            {
                if (!SliceMath.IsDrag(_pressPoint, cursor, _data.StartDistance)) return;
                _input.ClaimPointerPress(_slot);
                if (!TryStartStroke())
                {
                    _pressing = false;
                    return;
                }
            }

            Cut(_last, cursor);
            _last = cursor;
            _strokeTime += Time.deltaTime;
            if (_strokeTime >= _data.MaxStrokeTime) Stop();
        }

        private bool TryStartStroke()
        {
            if (_dodge.IsDodging || Time.time < _readyAt || !_stamina.TrySpend(_data.StaminaCost)) return false;
            _stroking = true;
            _strokeTime = 0f;
            _alreadyHit.Clear();
            _stroke.Clear();
            return true;
        }

        private void Cut(Vector2 from, Vector2 to)
        {
            if (!SliceMath.ClipToCircle(from, to, transform.position, _data.Reach, out var a, out var b)) return;
            AddTrail(a);
            AddTrail(b);

            var count = Physics2D.Linecast(a, b, _filter, _hits);
            for (var i = 0; i < count; i++)
            {
                if (!_hits[i].collider.TryGetComponent(out Hurtbox hurtbox)) continue;
                var target = hurtbox.Health;
                if (target == null || target == _health || !target.IsSpawned || target.IsDead) continue;
                if (hurtbox.IsInvulnerable || !_alreadyHit.Add(target)) continue;
                var push = SliceMath.Knockback(b - a, _data.Knockback, _data.KnockbackLift);
                target.ReportHit(new DamageInfo(_data.Damage, push, NetworkObjectId, Team.Player));
            }
        }

        private void AddTrail(Vector2 point)
        {
            if (_stroke.Count >= _data.MaxTrailPoints) return;
            if (_stroke.Count > 0 && (_stroke[_stroke.Count - 1] - point).sqrMagnitude < 0.0001f) return;
            _stroke.Add(point);
            _view.Draw(_stroke);
        }

        private void Stop()
        {
            _pressing = false;
            if (!_stroking) return;
            _stroking = false;
            _readyAt = Time.time + _data.Cooldown;
            _view.FadeOut();
            if (_stroke.Count >= 2) StrokeRpc(_stroke.ToArray());
        }

        /// <summary>Owner to everyone else: draw the finished slash.</summary>
        [Rpc(SendTo.NotOwner)]
        private void StrokeRpc(Vector2[] points) => _view.Flash(points);
    }
}
