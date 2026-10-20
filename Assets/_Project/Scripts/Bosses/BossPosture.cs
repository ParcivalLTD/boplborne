using GraceRun.Combat;
using GraceRun.Core.Audio;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Elden Ring-style posture and stagger component on the boss.
    /// Tracks posture build-up from damage. When posture breaks, the boss is immobilized
    /// for a duration, plays the stagger cue, shows the critical weak point, and receives critical damage.
    /// </summary>
    [RequireComponent(typeof(BossBrain), typeof(Health))]
    public class BossPosture : NetworkBehaviour
    {
        private readonly NetworkVariable<float> _currentPosture = new(
            0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<bool> _isStaggered = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private BossBrain _boss;
        private Health _health;
        private Telegraph _telegraph;
        private float _timeSinceLastHit;
        private float _staggerTimer;

        public float Current => _currentPosture.Value;
        public float Max => _boss != null && _boss.Data != null ? _boss.Data.MaxPosture : 100f;
        public float Normalized => BossPostureRules.Normalized(_currentPosture.Value, Max);
        public bool IsStaggered => _isStaggered.Value;
        public float StaggerDamageMultiplier => _boss != null && _boss.Data != null ? _boss.Data.StaggerDamageMultiplier : 1.5f;

        private void Awake()
        {
            _boss = GetComponent<BossBrain>();
            _health = GetComponent<Health>();
            _telegraph = GetComponent<Telegraph>();
        }

        public override void OnNetworkSpawn()
        {
            _isStaggered.OnValueChanged += OnStaggerChanged;
            if (IsServer)
            {
                _currentPosture.Value = 0f;
                _isStaggered.Value = false;
                _timeSinceLastHit = 0f;
                _staggerTimer = 0f;
            }
        }

        public override void OnNetworkDespawn()
        {
            _isStaggered.OnValueChanged -= OnStaggerChanged;
        }

        private void Update()
        {
            if (!IsServer || !IsSpawned || _health.IsDead) return;

            if (_isStaggered.Value)
            {
                _staggerTimer -= Time.deltaTime;
                if (_staggerTimer <= 0f)
                {
                    _isStaggered.Value = false;
                    _currentPosture.Value = 0f;
                    _timeSinceLastHit = 0f;
                    _boss.ServerRecoverFromStagger();
                }
            }
            else
            {
                _timeSinceLastHit += Time.deltaTime;
                var data = _boss.Data;
                if (data != null && _currentPosture.Value > 0f)
                {
                    _currentPosture.Value = BossPostureRules.UpdatePosture(
                        _currentPosture.Value, _timeSinceLastHit, data.PostureRecoveryDelay, data.PostureRecoveryRate, Time.deltaTime);
                }
            }
        }

        /// <summary>Server only: inflicts posture damage from a landed attack.</summary>
        public void ServerInflictDamage(int rawDamage, bool isHeavy)
        {
            if (!IsServer || !IsSpawned || _health.IsDead || _isStaggered.Value) return;

            _timeSinceLastHit = 0f;
            var data = _boss.Data;
            var max = data != null ? data.MaxPosture : 100f;
            var postureDamage = BossPostureRules.CalculatePostureDamage(rawDamage, isHeavy);
            _currentPosture.Value = Mathf.Min(max, _currentPosture.Value + postureDamage);

            if (BossPostureRules.IsBroken(_currentPosture.Value, max))
            {
                _isStaggered.Value = true;
                _staggerTimer = data != null ? data.StaggerDuration : 3.5f;
                _boss.ServerStaggerBreak(_staggerTimer);
            }
        }

        private void OnStaggerChanged(bool previous, bool current)
        {
            if (_telegraph != null) _telegraph.SetStaggerWeakPoint(current);

            if (current)
            {
                if (_boss != null && _boss.Data != null && _boss.Data.StaggerCue != null)
                {
                    AudioCuePlayer.Play2D(_boss.Data.StaggerCue);
                }
                Shake.Play(1.2f);
            }
        }
    }
}
