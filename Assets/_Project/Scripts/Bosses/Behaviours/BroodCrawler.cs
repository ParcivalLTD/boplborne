using System.Collections;
using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Networked minion spawned by The Brood Mother. The server chases the nearest living player and
    /// hops when close; because it has a <see cref="Knockback"/> on the Boss layer, player attacks
    /// and abilities (Black Hole, Gust, Bomb) gather and launch it. Its victim-side <see cref="Hitbox"/>
    /// re-arms every <see cref="CrawlerData.BiteCooldown"/> on every PC.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(Knockback))]
    public class BroodCrawler : NetworkBehaviour
    {
        [SerializeField] private CrawlerData _data;
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private Transform _visuals;

        private static readonly List<BroodCrawler> Active = new();

        private readonly NetworkVariable<sbyte> _facing = new(
            1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Rigidbody2D _body;
        private Health _health;
        private Knockback _knockback;
        private float _stunTimer;
        private float _hopTimer;
        private float _biteTimer;

        /// <summary>Number of living crawlers currently spawned.</summary>
        public static int ActiveCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < Active.Count; i++)
                    if (Active[i] != null && Active[i].IsSpawned && !Active[i]._health.IsDead) count++;
                return count;
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();
            _knockback = GetComponent<Knockback>();
        }

        public override void OnNetworkSpawn()
        {
            if (!Active.Contains(this)) Active.Add(this);
            _facing.OnValueChanged += OnFacingChanged;
            _health.Died += OnDied;
            ApplyFacing(_facing.Value);
            ArmBite();

            if (!IsServer) return;
            _knockback.Pushed += OnPushed;
            _knockback.Steered += OnSteered;
            _hopTimer = _data.BiteCooldown;
        }

        public override void OnNetworkDespawn()
        {
            Active.Remove(this);
            _facing.OnValueChanged -= OnFacingChanged;
            _health.Died -= OnDied;
            _knockback.Pushed -= OnPushed;
            _knockback.Steered -= OnSteered;
            _hitbox.Disarm();
        }

        private void Update()
        {
            if (!IsSpawned || _health.IsDead) return;
            if (IsServer && (BossBrain.Active == null || BossBrain.Active.Health.IsDead))
            {
                _health.ServerSetCurrent(0);
                return;
            }

            _biteTimer -= Time.deltaTime;
            if (_biteTimer <= 0f) ArmBite();
        }

        private void FixedUpdate()
        {
            if (!IsServer || !IsSpawned || _health.IsDead) return;
            if (_stunTimer > 0f)
            {
                _stunTimer -= Time.fixedDeltaTime;
                return;
            }

            if (_hopTimer > 0f) _hopTimer -= Time.fixedDeltaTime;
            if (!TryFindNearestPlayer(_body.position.x, out var targetX))
            {
                _body.linearVelocityX = 0f;
                return;
            }

            var dx = targetX - _body.position.x;
            var dir = Mathf.Approximately(dx, 0f) ? _facing.Value : (sbyte)(dx > 0f ? 1 : -1);
            if (_facing.Value != dir) _facing.Value = dir;

            if (Mathf.Abs(dx) <= _data.HopRange && _hopTimer <= 0f && Mathf.Abs(_body.linearVelocityY) <= 0.1f)
            {
                _hopTimer = _data.BiteCooldown;
                _stunTimer = 0.25f;
                _body.linearVelocity = new Vector2(dir * _data.HopVelocity.x, _data.HopVelocity.y);
                return;
            }

            _body.linearVelocityX = dir * _data.MoveSpeed;
        }

        /// <summary>Server only: despawns all spawned crawlers at once (arena swap, fight reset).</summary>
        public static void ServerDespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var crawler = Active[i];
                if (crawler != null && crawler.IsServer && crawler.IsSpawned)
                    crawler.NetworkObject.Despawn();
            }
            Active.Clear();
        }

        /// <summary>Server only: gives a freshly spawned crawler an initial pop velocity.</summary>
        public void ServerLaunch(Vector2 velocity, sbyte facing)
        {
            if (!IsServer) return;
            _facing.Value = facing;
            _stunTimer = _data.KnockbackStunTime;
            _body.linearVelocity = velocity;
        }

        private static bool TryFindNearestPlayer(float fromX, out float nearestX)
        {
            nearestX = 0f;
            var bestDist = float.MaxValue;
            var players = PlayerIdentity.All;
            for (var i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p == null || !p.IsSpawned) continue;
                if (p.gameObject.layer == Core.Layers.GhostIndex) continue;
                var x = p.transform.position.x;
                var d = Mathf.Abs(x - fromX);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearestX = x;
                }
            }
            return bestDist < float.MaxValue;
        }

        private void ArmBite()
        {
            _biteTimer = _data.BiteCooldown;
            _hitbox.Arm(_data.Damage, new Vector2(_data.Knockback.x * _facing.Value, _data.Knockback.y));
        }

        private void OnPushed(Vector2 _) => _stunTimer = Mathf.Max(_stunTimer, _data.KnockbackStunTime);

        private void OnSteered() => _stunTimer = Mathf.Max(_stunTimer, Time.fixedDeltaTime * 2f);

        private void OnDied()
        {
            _hitbox.Disarm();
            if (!IsServer) return;
            _body.linearVelocity = Vector2.zero;
            StartCoroutine(DespawnAfterDelay());
        }

        private IEnumerator DespawnAfterDelay()
        {
            yield return new WaitForSeconds(_data.DespawnDelay);
            if (IsSpawned) NetworkObject.Despawn();
        }

        private void OnFacingChanged(sbyte previous, sbyte current) => ApplyFacing(current);

        private void ApplyFacing(sbyte facing)
        {
            if (_visuals == null) return;
            var scale = _visuals.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            _visuals.localScale = scale;
        }
    }
}
