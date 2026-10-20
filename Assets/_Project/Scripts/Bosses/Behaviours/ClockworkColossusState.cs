using GraceRun.Combat;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Manages unique state for The Clockwork Colossus:
    /// detects hits landed against its exposed back core to award double posture damage,
    /// and activates the arena floor conveyor belt upon entering Phase 2.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(BossBrain), typeof(BossMotor))]
    public class ClockworkColossusState : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer _backCoreRenderer;
        [SerializeField] private ClockworkConveyor _conveyor;

        private Health _health;
        private BossBrain _brain;
        private BossMotor _motor;
        private BossPosture _posture;
        private float _flashTimer;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _brain = GetComponent<BossBrain>();
            _motor = GetComponent<BossMotor>();
            _posture = GetComponent<BossPosture>();
            if (_conveyor == null) _conveyor = GetComponent<ClockworkConveyor>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.HitTaken += OnHitTaken;
        }

        private void OnDisable()
        {
            if (_health != null) _health.HitTaken -= OnHitTaken;
            if (_conveyor != null && IsServer) _conveyor.ServerSetActive(false);
        }

        private void Update()
        {
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && _backCoreRenderer != null)
                {
                    _backCoreRenderer.color = new Color(1f, 0.79f, 0.29f, 0.6f);
                }
            }

            if (!IsServer || !IsSpawned) return;

            if (_brain.Phase >= 2 && _conveyor != null && !_conveyor.IsActive && !_health.IsDead)
            {
                _conveyor.ServerSetActive(true);
            }
            else if (_health.IsDead && _conveyor != null && _conveyor.IsActive)
            {
                _conveyor.ServerSetActive(false);
            }
        }

        private void OnHitTaken(DamageInfo info)
        {
            if (!IsServer || _health.IsDead || _posture == null) return;
            if (info.AttackerId == DamageInfo.NoAttacker || info.AttackerTeam != Team.Player) return;

            var spawnManager = NetworkManager.Singleton != null ? NetworkManager.Singleton.SpawnManager : null;
            if (spawnManager == null || !spawnManager.SpawnedObjects.TryGetValue(info.AttackerId, out var attackerObj)) return;

            var attackerX = attackerObj.transform.position.x;
            var bossX = transform.position.x;
            var facing = _motor.Facing;

            if (ClockworkColossusRules.IsBackHit(attackerX, bossX, facing))
            {
                // Hit landed against the back weak point: deal additional posture damage
                var extraPosture = ClockworkColossusRules.CalculateBackHitPostureDamage(info.Amount, isBackHit: true);
                _posture.ServerInflictDamage(Mathf.RoundToInt(extraPosture), isHeavy: true);
                FlashBackCoreClientRpc();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void FlashBackCoreClientRpc()
        {
            if (_backCoreRenderer != null)
            {
                _backCoreRenderer.color = Color.white;
                _flashTimer = 0.2f;
            }
            Shake.Play(0.5f);
        }
    }
}
