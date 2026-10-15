using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// A player's core numbers: health, stamina, the dodge roll and the flask.
    /// Starting values come from docs/GDD.md; tune the asset under Data/Player, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Player Stats Data", fileName = "PlayerStatsData")]
    public class PlayerStatsData : ScriptableObject, IHealthStats
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int _maxHP = 100;

        [Header("Stamina")]
        [SerializeField, Min(1f)] private float _maxStamina = 100f;
        [SerializeField, Min(0f)] private float _staminaRegenPerSecond = 40f;
        [SerializeField, Min(0f)] private float _regenDelay = 0.5f;

        [Header("Dodge roll")]
        [SerializeField, Min(0f)] private float _dodgeCost = 25f;
        [SerializeField, Min(0.01f)] private float _dodgeDuration = 0.35f;
        [SerializeField, Min(0f)] private float _dodgeInvulnerableTime = 0.25f;
        [SerializeField, Min(0f)] private float _dodgeSpeed = 16f;

        [Header("Flask")]
        [SerializeField, Min(0)] private int _flaskCharges = 2;
        [SerializeField, Min(0)] private int _flaskHeal = 40;

        public int MaxHP => _maxHP;
        public float MaxStamina => _maxStamina;
        public float StaminaRegenPerSecond => _staminaRegenPerSecond;

        /// <summary>Seconds without spending before stamina starts to refill.</summary>
        public float RegenDelay => _regenDelay;

        public float DodgeCost => _dodgeCost;
        public float DodgeDuration => _dodgeDuration;

        /// <summary>Invulnerable for this long from the start of the roll.</summary>
        public float DodgeInvulnerableTime => _dodgeInvulnerableTime;

        /// <summary>Horizontal speed held through the whole roll, in units per second.</summary>
        public float DodgeSpeed => _dodgeSpeed;

        public int FlaskCharges => _flaskCharges;
        public int FlaskHeal => _flaskHeal;
    }
}
