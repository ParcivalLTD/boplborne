using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Manages state, visual crown fracture, and aura radiance for The Crowned Hollow (Final Boss).
    /// Tracks phase transitions: in Phase 2, the shattered crown ignites with radiant golden flame
    /// and unlocks player ability mimicry and the royal Cataclysm supernova.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(BossBrain), typeof(BossMotor))]
    public class CrownedHollowState : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer _crownRenderer;
        [SerializeField] private SpriteRenderer _auraRenderer;

        private Health _health;
        private BossBrain _brain;
        private BossMotor _motor;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _brain = GetComponent<BossBrain>();
            _motor = GetComponent<BossMotor>();
        }

        private void Update()
        {
            var facing = _motor != null ? _motor.Facing : 1f;

            if (_crownRenderer != null)
            {
                var hoverY = 2.8f + (Mathf.Sin(Time.time * 2.8f) * 0.18f);
                _crownRenderer.transform.localPosition = new Vector3(0.2f * facing, hoverY, 0f);

                var isPhase2 = _brain != null && _brain.Phase >= 2;
                if (isPhase2)
                {
                    var pulse = 0.85f + (0.15f * Mathf.Sin(Time.time * 5f));
                    _crownRenderer.color = new Color(1f, 0.85f, 0.45f, pulse);
                    _crownRenderer.transform.localScale = new Vector3(1.4f * pulse, 1.4f * pulse, 1f);
                }
                else
                {
                    _crownRenderer.color = new Color(0.78f, 0.49f, 0.60f, 0.85f);
                    _crownRenderer.transform.localScale = Vector3.one;
                }
            }

            if (_auraRenderer != null)
            {
                var auraPulse = 0.4f + (0.15f * Mathf.Sin(Time.time * 3f));
                _auraRenderer.color = new Color(0.78f, 0.49f, 0.60f, auraPulse);
            }
        }
    }
}
