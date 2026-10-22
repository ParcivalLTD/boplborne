using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class BuffStacksTests
    {
        private static readonly PartyBuffTuning Tuning = new(25, 1.2f, 1.15f, 1.25f);

        [Test]
        public void PlusCountsEachBuffSeparately()
        {
            var stacks = default(BuffStacks).Plus(PartyBuff.EmpoweredAttacks).Plus(PartyBuff.EmpoweredAttacks).Plus(PartyBuff.IronSkin);
            Assert.AreEqual(2, stacks.Of(PartyBuff.EmpoweredAttacks));
            Assert.AreEqual(1, stacks.Of(PartyBuff.IronSkin));
            Assert.AreEqual(0, stacks.Of(PartyBuff.Swiftstride));
            Assert.IsFalse(stacks.IsEmpty);
        }

        [Test]
        public void StacksAddTheirBonus()
        {
            var two = default(BuffStacks).Plus(PartyBuff.EmpoweredAttacks).Plus(PartyBuff.EmpoweredAttacks);
            Assert.AreEqual(1.4f, Tuning.DamageMultiplier(two), 0.0001f);
            Assert.AreEqual(1f, Tuning.DamageMultiplier(default(BuffStacks)), 0.0001f);
            var iron = default(BuffStacks).Plus(PartyBuff.IronSkin).Plus(PartyBuff.IronSkin).Plus(PartyBuff.IronSkin);
            Assert.AreEqual(75, Tuning.BonusMaxHp(iron));
            var swift = default(BuffStacks).Plus(PartyBuff.Swiftstride).Plus(PartyBuff.Swiftstride);
            Assert.AreEqual(1.3f, Tuning.MoveMultiplier(swift), 0.0001f);
            Assert.AreEqual(1.5f, Tuning.StaminaRegenMultiplier(swift), 0.0001f);
        }

        [Test]
        public void EachStackCostsMore()
        {
            var one = default(BuffStacks).Plus(PartyBuff.IronSkin);
            Assert.AreEqual(75, Tuning.CostOfNext(PartyBuff.IronSkin, default, 75));
            Assert.AreEqual(100, Tuning.CostOfNext(PartyBuff.IronSkin, one, 75));
            Assert.AreEqual(75, Tuning.CostOfNext(PartyBuff.Swiftstride, one, 75));
        }

        [Test]
        public void BuyingStopsAtTheCapOrWithoutMoney()
        {
            var full = default(BuffStacks).Plus(PartyBuff.IronSkin).Plus(PartyBuff.IronSkin).Plus(PartyBuff.IronSkin);
            Assert.IsFalse(RestStopRules.CanBuyBuff(PartyBuff.IronSkin, full, 3, 1000, 75));
            Assert.IsTrue(RestStopRules.CanBuyBuff(PartyBuff.IronSkin, full.Plus(PartyBuff.None), 4, 1000, 75));
            Assert.IsFalse(RestStopRules.CanBuyBuff(PartyBuff.EmpoweredAttacks, default, 3, 50, 75));
            Assert.IsTrue(RestStopRules.CanBuyBuff(PartyBuff.EmpoweredAttacks, default, 3, 75, 75));
            Assert.IsFalse(RestStopRules.CanBuyBuff(PartyBuff.None, default, 3, 1000, 75));
        }
    }
}
