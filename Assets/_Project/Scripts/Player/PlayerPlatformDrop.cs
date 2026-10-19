using System.Collections.Generic;
using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Down + jump on a one-way ledge drops you through it: the ledges under your feet stop
    /// colliding with you for <see cref="PlayerMovementData.DropThroughTime"/>. Owner only, like
    /// the rest of the movement; <see cref="PlayerMovement"/> asks for it on a jump press.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerPlatformDrop : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData _data;
        [SerializeField] private Transform _groundCheck;

        private readonly Collider2D[] _hits = new Collider2D[4];
        private readonly List<Collider2D> _ignored = new();
        private Collider2D _collider;
        private ContactFilter2D _filter;
        private float _timer;

        /// <summary>True while ledges are being fallen through.</summary>
        public bool IsDropping => _timer > 0f;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.PlatformIndex),
                useTriggers = false,
            };
        }

        private void OnDisable() => Restore();

        private void FixedUpdate()
        {
            if (_timer <= 0f) return;
            _timer -= Time.fixedDeltaTime;
            if (_timer <= 0f) Restore();
        }

        /// <summary>Starts a drop when a ledge is under the feet; false when standing on solid ground.</summary>
        public bool TryDrop()
        {
            var count = Physics2D.OverlapBox(_groundCheck.position, _data.GroundCheckSize, 0f, _filter, _hits);
            if (count == 0) return false;
            Restore();
            for (var i = 0; i < count; i++)
            {
                Physics2D.IgnoreCollision(_collider, _hits[i], true);
                _ignored.Add(_hits[i]);
            }
            _timer = _data.DropThroughTime;
            return true;
        }

        private void Restore()
        {
            _timer = 0f;
            foreach (var platform in _ignored)
                if (platform != null && _collider != null) Physics2D.IgnoreCollision(_collider, platform, false);
            _ignored.Clear();
        }
    }
}
