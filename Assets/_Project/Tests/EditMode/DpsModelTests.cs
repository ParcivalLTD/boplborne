using System.Collections.Generic;
using GraceRun.Combat;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class DpsModelTests
    {
        [Test]
        public void AbilityDpsIsScaledDamageTimesHitsOverCooldown()
        {
            Assert.AreEqual(13.5f, DpsModel.AbilityDps(6, 3, 4f, 3f), 0.001f);
        }

        [Test]
        public void ZeroCooldownGivesNoDps() => Assert.AreEqual(0f, DpsModel.AbilityDps(10, 1, 0f, 1f));

        [Test]
        public void UnlimitedStaminaWeaponIsDamageOverSwingTime()
        {
            var combo = new List<SwingStat> { new(10, 0.5f, 0f) };
            Assert.AreEqual(20f, DpsModel.SustainedWeaponDps(combo, 1f, 100f, 40f, 0.5f, 30f), 0.5f);
        }

        [Test]
        public void StaminaLimitsASwingSpammer()
        {
            var combo = new List<SwingStat> { new(10, 0.25f, 20f) };
            var unlimited = DpsModel.SustainedWeaponDps(combo, 1f, 1000000f, 40f, 0.5f, 60f);
            var limited = DpsModel.SustainedWeaponDps(combo, 1f, 100f, 40f, 0.5f, 60f);
            Assert.Less(limited, unlimited * 0.8f);
        }

        [Test]
        public void CooldownShaveScalesWithDamageAndCaps()
        {
            var combat = UnityEngine.ScriptableObject.CreateInstance<CombatData>();
            Assert.AreEqual(0f, combat.CooldownShave(0));
            Assert.AreEqual(7 * 0.025f, combat.CooldownShave(7), 0.0001f);
            Assert.AreEqual(0.6f, combat.CooldownShave(100), 0.0001f);
            UnityEngine.Object.DestroyImmediate(combat);
        }

        [Test]
        public void DamageMultiplierPicksWeaponOrAbility()
        {
            var combat = UnityEngine.ScriptableObject.CreateInstance<CombatData>();
            Assert.AreEqual(combat.WeaponDamageMultiplier, combat.DamageMultiplierFor(DamageSource.Unknown));
            Assert.AreEqual(combat.WeaponDamageMultiplier, combat.DamageMultiplierFor(DamageSource.Melee));
            Assert.AreEqual(combat.AbilityDamageMultiplier, combat.DamageMultiplierFor(DamageSource.OfAbility("bomb")));
            UnityEngine.Object.DestroyImmediate(combat);
        }
    }
}
