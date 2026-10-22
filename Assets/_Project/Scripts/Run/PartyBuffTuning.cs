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

        [SerializeField, Min(1), Tooltip("How many times each buff can be bought for the next fight. Stacks add their bonus (two Empowered Attacks = +40%).")]
        private int _maxStacks = 3;
        [SerializeField, Min(0), Tooltip("Each stack already owned makes the next one of the same buff this much dearer.")]
        private int _stackCostStep = 25;

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

        public int MaxStacks => _maxStacks;
        public int StackCostStep => _stackCostStep;

        /// <summary>Extra max HP for every player from every Iron Skin bought.</summary>
        public int BonusMaxHp(BuffStacks stacks) => _ironSkinBonusMaxHp * stacks.IronSkin;

        /// <summary>Damage multiplier against enemies: each Empowered Attacks adds its bonus.</summary>
        public float DamageMultiplier(BuffStacks stacks) => 1f + (_empoweredDamageMultiplier - 1f) * stacks.Empowered;

        /// <summary>Run speed multiplier: each Swiftstride adds its bonus.</summary>
        public float MoveMultiplier(BuffStacks stacks) => 1f + (_swiftstrideMoveMultiplier - 1f) * stacks.Swiftstride;

        /// <summary>Stamina regen multiplier: each Swiftstride adds its bonus.</summary>
        public float StaminaRegenMultiplier(BuffStacks stacks) => 1f + (_swiftstrideStaminaRegenMultiplier - 1f) * stacks.Swiftstride;

        /// <summary>Price of the next <paramref name="buff"/> with <paramref name="stacks"/> already owned.</summary>
        public int CostOfNext(PartyBuff buff, BuffStacks stacks, int baseCost) => baseCost + _stackCostStep * stacks.Of(buff);

        public int BonusMaxHp(PartyBuff active) => active == PartyBuff.IronSkin ? _ironSkinBonusMaxHp : 0;
        public float DamageMultiplier(PartyBuff active) => active == PartyBuff.EmpoweredAttacks ? _empoweredDamageMultiplier : 1f;
        public float MoveMultiplier(PartyBuff active) => active == PartyBuff.Swiftstride ? _swiftstrideMoveMultiplier : 1f;
        public float StaminaRegenMultiplier(PartyBuff active) => active == PartyBuff.Swiftstride ? _swiftstrideStaminaRegenMultiplier : 1f;

        /// <summary>The run's tuning and active buff on this PC; false outside the Game scene (no RunManager or config).</summary>
        public static bool TryGetActive(out PartyBuffTuning tuning, out BuffStacks active)
        {
            var run = RunManager.Instance;
            tuning = run != null && run.Config != null ? run.Config.PartyBuffs : null;
            active = tuning != null ? run.Buffs : default;
            return tuning != null;
        }
    }
}
