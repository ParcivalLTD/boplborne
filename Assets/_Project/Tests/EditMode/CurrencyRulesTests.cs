using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class CurrencyRulesTests
    {
        [Test]
        public void BaseRewardOnlyWhenDeathsAndSlow()
        {
            var reward = CurrencyRules.CalculateBossReward(100, false, 50, false, 50);
            Assert.AreEqual(100, reward);
        }

        [Test]
        public void NoDeathsBonusAddedWhenCleanFight()
        {
            var reward = CurrencyRules.CalculateBossReward(100, true, 50, false, 50);
            Assert.AreEqual(150, reward);
        }

        [Test]
        public void FastKillBonusAddedWhenUnderTimeThreshold()
        {
            var reward = CurrencyRules.CalculateBossReward(100, false, 50, true, 50);
            Assert.AreEqual(150, reward);
        }

        [Test]
        public void BothBonusesAddedForFastCleanFight()
        {
            var reward = CurrencyRules.CalculateBossReward(100, true, 50, true, 50);
            Assert.AreEqual(200, reward);
        }

        [Test]
        public void NegativeRewardClampedToZero()
        {
            var reward = CurrencyRules.CalculateBossReward(-10, false, 0, false, 0);
            Assert.AreEqual(0, reward);
        }

        [TestCase(100, 50, true)]
        [TestCase(50, 50, true)]
        [TestCase(49, 50, false)]
        [TestCase(0, 10, false)]
        [TestCase(10, 0, true)]
        public void CanAffordChecksCostAgainstCurrent(int current, int cost, bool expected)
        {
            Assert.AreEqual(expected, CurrencyRules.CanAfford(current, cost));
        }

        [Test]
        public void SpendDeductsCostWhenAffordable()
        {
            Assert.AreEqual(50, CurrencyRules.Spend(100, 50));
            Assert.AreEqual(0, CurrencyRules.Spend(50, 50));
        }

        [Test]
        public void SpendLeavesBalanceUnchangedWhenNotAffordable()
        {
            Assert.AreEqual(40, CurrencyRules.Spend(40, 50));
        }
    }
}
