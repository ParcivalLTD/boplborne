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
            // Against boss (not friendly fire, player team)
            var buffed = RestStopRules.BuffedDamage(10, false, Team.Player, PartyBuff.EmpoweredAttacks);
            Assert.AreEqual(12, buffed); // 10 * 1.2 = 12

            // Friendly fire: NOT buffed
            var friendly = RestStopRules.BuffedDamage(10, true, Team.Player, PartyBuff.EmpoweredAttacks);
            Assert.AreEqual(10, friendly);

            // With no buff: normal damage
            var unbuffed = RestStopRules.BuffedDamage(10, false, Team.Player, PartyBuff.None);
            Assert.AreEqual(10, unbuffed);
        }
    }
}
