using GraceRun.Combat;
using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class RestStopRulesTests
    {
        [Test]
        public void CanUpgradeWhenLevel1AndAffordable()
        {
            Assert.IsTrue(RestStopRules.CanUpgradeAbility(true, 1, 100, 100));
            Assert.IsTrue(RestStopRules.CanUpgradeAbility(true, 1, 150, 100));
        }

        [Test]
        public void CannotUpgradeWhenEmptyOrLevel2OrCannotAfford()
        {
            Assert.IsFalse(RestStopRules.CanUpgradeAbility(false, 1, 100, 100));
            Assert.IsFalse(RestStopRules.CanUpgradeAbility(true, 2, 100, 100));
            Assert.IsFalse(RestStopRules.CanUpgradeAbility(true, 1, 99, 100));
        }

        [Test]
        public void CanBuyRerollChecksCost()
        {
            Assert.IsTrue(RestStopRules.CanBuyReroll(50, 50));
            Assert.IsFalse(RestStopRules.CanBuyReroll(40, 50));
        }

        [Test]
        public void CanBuyBuffChecksAffordabilityAndDifferentBuff()
        {
            Assert.IsTrue(RestStopRules.CanBuyBuff(PartyBuff.EmpoweredAttacks, PartyBuff.None, 75, 75));
            Assert.IsFalse(RestStopRules.CanBuyBuff(PartyBuff.EmpoweredAttacks, PartyBuff.EmpoweredAttacks, 75, 75));
            Assert.IsFalse(RestStopRules.CanBuyBuff(PartyBuff.EmpoweredAttacks, PartyBuff.None, 50, 75));
            Assert.IsFalse(RestStopRules.CanBuyBuff(PartyBuff.None, PartyBuff.None, 100, 75));
        }

        [Test]
        public void EmpoweredAttacksBuffsDamageAgainstEnemiesOnly()
        {
            var tuning = new PartyBuffTuning(25, 1.2f, 1.15f, 1.25f);
            var empowered = tuning.DamageMultiplier(PartyBuff.EmpoweredAttacks);

            Assert.AreEqual(12, RestStopRules.BuffedDamage(10, false, Team.Player, empowered));
            Assert.AreEqual(10, RestStopRules.BuffedDamage(10, true, Team.Player, empowered), "friendly fire is not buffed");
            Assert.AreEqual(10, RestStopRules.BuffedDamage(10, false, Team.Boss, empowered), "boss hits are not buffed");
            Assert.AreEqual(10, RestStopRules.BuffedDamage(10, false, Team.Player, tuning.DamageMultiplier(PartyBuff.None)));
        }

        [Test]
        public void EachBuffOnlyChangesItsOwnStat()
        {
            var tuning = new PartyBuffTuning(30, 1.5f, 1.2f, 1.4f);

            Assert.AreEqual(30, tuning.BonusMaxHp(PartyBuff.IronSkin));
            Assert.AreEqual(0, tuning.BonusMaxHp(PartyBuff.Swiftstride));
            Assert.AreEqual(1.2f, tuning.MoveMultiplier(PartyBuff.Swiftstride));
            Assert.AreEqual(1.4f, tuning.StaminaRegenMultiplier(PartyBuff.Swiftstride));
            Assert.AreEqual(1f, tuning.MoveMultiplier(PartyBuff.IronSkin));
            Assert.AreEqual(1f, tuning.StaminaRegenMultiplier(PartyBuff.None));
            Assert.AreEqual(1f, tuning.DamageMultiplier(PartyBuff.IronSkin));
        }
    }
}
