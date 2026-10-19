using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Ground-hugging ringing shockwave emitted by The Bell Twins. Travels left or right along the
    /// arena floor, hits players on the victim side (dodgeable by jumping over it or rolling through),
    /// stops on a <see cref="AttackBlocker"/> (Stone Wall) or arena wall, and despawns on the server.
    /// </summary>
    public class BellShockwave : NetworkBehaviour
    {
        [SerializeField] private ShockwaveData _data;
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private Transform _visuals;
        [SerializeField] private SpriteRenderer[] _renderers;

        private static readonly List<BellShockwave> Active = new();
        private static readonly RaycastHit2D[] BlockerHits = new RaycastHit2D[4];

        private readonly NetworkVariable<sbyte> _direction = new(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<float> _speed = new(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private ContactFilter2D _abilityFilter;
        private sbyte _seededDir = 1;
        private float _seededSpeed;
        private float _age;

        /// <summary>Number of active shockwaves currently in the arena.</summary>
        public static int ActiveCount => Active.Count;

        private void Awake()
        {
            _abilityFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = Layers.Mask(Layers.AbilityIndex),
                useTriggers = false
            };
        }

        /// <summary>Server only, before Spawn: sets the wave's horizontal direction (+1 or -1) and optional speed override.</summary>
        public void ServerConfigure(sbyte direction, float speedOverride = 0f)
        {
            _seededDir = (sbyte)(direction < 0 ? -1 : 1);
            _seededSpeed = speedOverride;
        }

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _age = 0f;
            if (IsServer)
            {
                _direction.Value = _seededDir;
                _speed.Value = _seededSpeed > 0f ? _seededSpeed : _data.Speed;
            }
            _direction.OnValueChanged += OnDirectionChanged;
            ApplyDirection(_direction.Value);
            UpdateAlpha();
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            _direction.OnValueChanged -= OnDirectionChanged;
            if (_hitbox != null) _hitbox.Disarm();
        }

        private void Update()
        {
            if (!IsSpawned) return;
            var dir = _direction.Value;
            var speed = _speed.Value > 0f ? _speed.Value : _data.Speed;
            transform.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);
            _age += Time.deltaTime;
            UpdateAlpha();

            if (!IsServer) return;
            if (_age >= _data.Lifetime ||
                Mathf.Abs(transform.position.x) >= _data.ArenaHalfWidth ||
                BossBrain.AreAllDefeated ||
                HitBlocker(dir))
            {
                NetworkObject.Despawn();
            }
        }

        /// <summary>Server only: despawns all active bell shockwaves at once (arena swap, fight reset).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var wave = Active[i];
                if (wave != null && wave.IsServer && wave.IsSpawned)
                    wave.NetworkObject.Despawn();
            }
            Active.Clear();
        }

        private bool HitBlocker(sbyte dir)
        {
            var origin = (Vector2)transform.position;
            var count = Physics2D.Raycast(origin, new Vector2(dir, 0f), _abilityFilter, BlockerHits, _data.BlockerCheckDistance);
            for (var i = 0; i < count; i++)
                if (BlockerHits[i].collider.TryGetComponent(out AttackBlocker _)) return true;
            return false;
        }

        private void OnDirectionChanged(sbyte previous, sbyte current) => ApplyDirection(current);

        private void ApplyDirection(sbyte dir)
        {
            if (_visuals != null)
            {
                var scale = _visuals.localScale;
                scale.x = Mathf.Abs(scale.x) * dir;
                _visuals.localScale = scale;
            }
            if (_hitbox != null && _data != null)
                _hitbox.Arm(_data.Damage, new Vector2(_data.Knockback.x * dir, _data.Knockback.y));
        }

        private void UpdateAlpha()
        {
            if (_data == null || _renderers == null) return;
            var alpha = BroodMotherMath.FadeAlpha(_age, _data.Lifetime, _data.FadeOutTime, _data.BaseAlpha);
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                var c = _renderers[i].color;
                c.a = alpha;
                _renderers[i].color = c;
            }
        }
    }
}
