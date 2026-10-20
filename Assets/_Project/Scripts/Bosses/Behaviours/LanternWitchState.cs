using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Manages state, visual illumination, and arena darkness gloom for The Lantern Witch.
    /// Tracks phase transitions: in Phase 2, her central lantern dims as its fire disperses
    /// into four split lanterns that illuminate the arena.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(BossBrain), typeof(BossMotor))]
    public class LanternWitchState : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer _lanternRenderer;
        [SerializeField] private SpriteRenderer _lightHaloRenderer;
        [SerializeField] private SpriteRenderer _gloomOverlay;

        private Health _health;
        private BossBrain _brain;
        private BossMotor _motor;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _brain = GetComponent<BossBrain>();
            _motor = GetComponent<BossMotor>();
        }

        public override void OnNetworkSpawn()
        {
            SetGloomVisible(true);
        }

        public override void OnNetworkDespawn()
        {
            SetGloomVisible(false);
        }

        private void OnDisable()
        {
            SetGloomVisible(false);
        }

        private void Update()
        {
            var facing = _motor != null ? _motor.Facing : 1f;

            if (_lanternRenderer != null)
            {
                var hoverY = 0.5f + (Mathf.Sin(Time.time * 3f) * 0.15f);
                _lanternRenderer.transform.localPosition = new Vector3(1.1f * facing, hoverY, 0f);
            }

            if (_lightHaloRenderer != null)
            {
                var isPhase2 = _brain != null && _brain.Phase >= 2;
                var baseAlpha = isPhase2 ? 0.35f : 0.65f;
                var pulse = baseAlpha + (0.1f * Mathf.Sin(Time.time * 4f));
                _lightHaloRenderer.color = new Color(0.95f, 0.88f, 0.72f, pulse);
            }

            if (_gloomOverlay != null)
            {
                var isPhase2 = _brain != null && _brain.Phase >= 2;
                var targetAlpha = isPhase2 ? 0.60f : 0.45f;
                var current = _gloomOverlay.color;
                current.a = Mathf.MoveTowards(current.a, targetAlpha, Time.deltaTime * 0.5f);
                _gloomOverlay.color = current;
            }
        }

        private void SetGloomVisible(bool visible)
        {
            if (_gloomOverlay != null)
            {
                _gloomOverlay.enabled = visible;
                if (!visible)
                {
                    var c = _gloomOverlay.color;
                    c.a = 0f;
                    _gloomOverlay.color = c;
                }
            }
        }
    }
}
