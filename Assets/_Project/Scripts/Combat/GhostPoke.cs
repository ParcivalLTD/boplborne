using GraceRun.Bosses;
using GraceRun.Core;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// A ghost's only attack: Attack near the boss pokes it for no damage. The poke plays the hit
    /// feedback through the normal hit report (flash, sparks, a short hitstop) and asks the server
    /// for a tiny stagger. Owner input; <see cref="Ghost"/> enables it.
    /// </summary>
    public class GhostPoke : NetworkBehaviour
    {
        [SerializeField] private ReviveData _data;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private Health _health;

        private readonly Collider2D[] _hits = new Collider2D[4];
        private ContactFilter2D _bossFilter;
        private float _readyAt;

        private void Awake()
        {
            _bossFilter = new ContactFilter2D { useLayerMask = true, layerMask = Layers.Mask(Layers.BossIndex) };
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) _input.AttackPressed += OnAttackPressed;
        }

        public override void OnNetworkDespawn()
        {
            if (_input != null) _input.AttackPressed -= OnAttackPressed;
        }

        private void OnAttackPressed(bool _)
        {
            if (!enabled || Time.time < _readyAt) return;
            var boss = FindBoss();
            if (boss == null) return;

            _readyAt = Time.time + _data.PokeCooldown;
            boss.Health.ReportHit(new DamageInfo(0, Vector2.zero, NetworkObjectId, Team.Player));
            StaggerRpc(boss.NetworkObject);
        }

        private BossBrain FindBoss()
        {
            var count = Physics2D.OverlapCircle(transform.position, _data.PokeRange, _bossFilter, _hits);
            for (var i = 0; i < count; i++)
            {
                if (!_hits[i].TryGetComponent(out Hurtbox hurtbox) || hurtbox.Health == null) continue;
                if (hurtbox.Health.TryGetComponent(out BossBrain boss) && !boss.Health.IsDead) return boss;
            }
            return null;
        }

        [Rpc(SendTo.Server)]
        private void StaggerRpc(NetworkObjectReference bossRef)
        {
            if (!_health.IsDead || !bossRef.TryGet(out var bossObject)) return;
            if (!bossObject.TryGetComponent(out BossBrain boss)) return;
            boss.ServerStagger(_data.PokeStagger);
        }
    }
}
