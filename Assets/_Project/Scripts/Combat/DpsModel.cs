using System.Collections.Generic;
using GraceRun.Player;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>One weapon swing as the DPS model sees it.</summary>
    public readonly struct SwingStat
    {
        public SwingStat(int damage, float seconds, float stamina)
        {
            Damage = damage;
            Seconds = seconds;
            Stamina = stamina;
        }

        public int Damage { get; }

        /// <summary>Startup + active + recovery.</summary>
        public float Seconds { get; }

        public float Stamina { get; }
    }

    /// <summary>
    /// Pure damage-per-second model used to keep weapons and abilities in balance (docs/GDD.md,
    /// "Abilities are the main way to fight"). It assumes every hit lands and ignores weak points,
    /// statuses, burning pools and combos, so it compares options, it does not predict a fight.
    /// </summary>
    public static class DpsModel
    {
        private const float Step = 0.01f;

        /// <summary>Damage per second of an ability used on cooldown: <paramref name="hits"/> hits of <paramref name="damage"/> per cast.</summary>
        public static float AbilityDps(int damage, int hits, float cooldown, float multiplier) =>
            cooldown <= 0f ? 0f : DamageMath.Scaled(damage, multiplier) * Mathf.Max(1, hits) / cooldown;

        /// <summary>
        /// Sustained damage per second of repeating <paramref name="combo"/> over <paramref name="horizon"/>
        /// seconds with a stamina pool: swings wait for stamina, which only refills after
        /// <paramref name="regenDelay"/> seconds without spending.
        /// </summary>
        public static float SustainedWeaponDps(IReadOnlyList<SwingStat> combo, float multiplier, float maxStamina,
            float regenPerSecond, float regenDelay, float horizon)
        {
            if (combo.Count == 0 || horizon <= 0f) return 0f;
            var pool = new StaminaPool(maxStamina);
            var time = 0f;
            var index = 0;
            long damage = 0;
            while (time < horizon)
            {
                var swing = combo[index];
                if (pool.TrySpend(swing.Stamina, regenDelay))
                {
                    damage += DamageMath.Scaled(swing.Damage, multiplier);
                    for (var t = 0f; t < swing.Seconds; t += Step) pool.Tick(Step, regenPerSecond);
                    time += swing.Seconds;
                    index = (index + 1) % combo.Count;
                }
                else
                {
                    pool.Tick(Step, regenPerSecond);
                    time += Step;
                }
            }
            return damage / horizon;
        }
    }
}
