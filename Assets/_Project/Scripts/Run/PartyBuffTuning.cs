using System;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Numbers for the one-off rest stop party buffs (part of <see cref="RunConfig"/>). Each lookup
    /// returns the neutral value (1 or 0) unless its own buff is the active one.
    /// </summary>
    [Serializable]
    public class PartyBuffTuning
    {
        [SerializeField, Min(0), Tooltip("Iron Skin: extra max HP for every player in the next fight.")]
        private int _ironSkinBonusMaxHp = 25;
        [SerializeField, Min(1f), Tooltip("Empowered Attacks: player damage multiplier against enemies (not teammates).")]
        private float _empoweredDamageMultiplier = 1.2f;
        [SerializeField, Min(1f), Tooltip("Swiftstride: run speed multiplier.")]
        private float _swiftstrideMoveMultiplier = 1.15f;
        [SerializeField, Min(1f), Tooltip("Swiftstride: stamina regen multiplier.")]
        private float _swiftstrideStaminaRegenMultiplier = 1.25f;

        public PartyBuffTuning() { }

        public PartyBuffTuning(int ironSkinBonusMaxHp, float empoweredDamageMultiplier,
            float swiftstrideMoveMultiplier, float swiftstrideStaminaRegenMultiplier)
        {
            _ironSkinBonusMaxHp = ironSkinBonusMaxHp;
            _empoweredDamageMultiplier = empoweredDamageMultiplier;
            _swiftstrideMoveMultiplier = swiftstrideMoveMultiplier;
            _swiftstrideStaminaRegenMultiplier = swiftstrideStaminaRegenMultiplier;
        }

        public int IronSkinBonusMaxHp => _ironSkinBonusMaxHp;
        public float EmpoweredDamageMultiplier => _empoweredDamageMultiplier;
        public float SwiftstrideMoveMultiplier => _swiftstrideMoveMultiplier;
        public float SwiftstrideStaminaRegenMultiplier => _swiftstrideStaminaRegenMultiplier;

        public int BonusMaxHp(PartyBuff active) => active == PartyBuff.IronSkin ? _ironSkinBonusMaxHp : 0;
        public float DamageMultiplier(PartyBuff active) => active == PartyBuff.EmpoweredAttacks ? _empoweredDamageMultiplier : 1f;
        public float MoveMultiplier(PartyBuff active) => active == PartyBuff.Swiftstride ? _swiftstrideMoveMultiplier : 1f;
        public float StaminaRegenMultiplier(PartyBuff active) => active == PartyBuff.Swiftstride ? _swiftstrideStaminaRegenMultiplier : 1f;

        /// <summary>The run's tuning and active buff on this PC; false outside the Game scene (no RunManager or config).</summary>
        public static bool TryGetActive(out PartyBuffTuning tuning, out PartyBuff active)
        {
            var run = RunManager.Instance;
            tuning = run != null && run.Config != null ? run.Config.PartyBuffs : null;
            active = tuning != null ? run.ActiveBuff : PartyBuff.None;
            return tuning != null;
        }
    }
}
