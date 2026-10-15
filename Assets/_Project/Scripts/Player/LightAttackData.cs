using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The 3-hit light attack combo. Starting values come from docs/GDD.md (10 stamina, 8 damage
    /// per hit, the final hit knocks back); tune the asset under Data/Player, never the code.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Light Attack Data", fileName = "LightAttackData")]
    public class LightAttackData : ScriptableObject
    {
        [SerializeField] private LightAttackHit[] _hits =
        {
            new(8, Vector2.zero, new Vector2(1.4f, 1f), new Vector2(0.9f, 0f), 0.08f, 0.1f, 0.22f, 10f),
            new(8, Vector2.zero, new Vector2(1.4f, 1f), new Vector2(0.9f, 0f), 0.08f, 0.1f, 0.22f, 10f),
            new(8, new Vector2(10f, 5f), new Vector2(1.8f, 1.2f), new Vector2(1.1f, 0f), 0.14f, 0.12f, 0.35f, 10f),
        };

        [Header("Chaining")]
        [SerializeField, Min(0f), Tooltip("A press this long before the current swing ends still starts the next one.")]
        private float _inputBuffer = 0.3f;
        [SerializeField, Min(0f), Tooltip("After a swing ends, a press within this time continues the combo instead of restarting it.")]
        private float _chainWindow = 0.35f;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f), Tooltip("Run speed multiplier while swinging.")]
        private float _moveSpeedMultiplier = 0.2f;

        public IReadOnlyList<LightAttackHit> Hits => _hits;
        public float InputBuffer => _inputBuffer;
        public float ChainWindow => _chainWindow;
        public float MoveSpeedMultiplier => _moveSpeedMultiplier;
    }
}
