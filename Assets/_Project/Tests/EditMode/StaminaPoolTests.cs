using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class StaminaPoolTests
    {
        private const float Max = 100f;
        private const float Cost = 25f;
        private const float Delay = 0.5f;
        private const float Regen = 40f;

        [Test]
        public void StartsFull()
        {
            Assert.AreEqual(Max, new StaminaPool(Max).Current);
        }

        [Test]
        public void FourDodgesEmptyIt()
        {
            var pool = new StaminaPool(Max);
            for (var i = 0; i < 4; i++) Assert.IsTrue(pool.TrySpend(Cost, Delay));

            Assert.AreEqual(0f, pool.Current, 0.0001f);
            Assert.IsFalse(pool.TrySpend(Cost, Delay));
        }

        [Test]
        public void CannotSpendWithoutTheFullCost()
        {
            var pool = new StaminaPool(Max);
            pool.TrySpend(90f, Delay);

            Assert.IsFalse(pool.TrySpend(Cost, Delay));
            Assert.AreEqual(10f, pool.Current, 0.0001f);
        }

        [Test]
        public void NoRegenDuringTheDelay()
        {
            var pool = new StaminaPool(Max);
            pool.TrySpend(Cost, Delay);
            pool.Tick(Delay * 0.9f, Regen);

            Assert.AreEqual(Max - Cost, pool.Current, 0.0001f);
        }

        [Test]
        public void RegensAfterTheDelayIncludingLeftoverTime()
        {
            var pool = new StaminaPool(Max);
            pool.TrySpend(Cost, Delay);
            pool.Tick(Delay + 0.25f, Regen);

            Assert.AreEqual(Max - Cost + Regen * 0.25f, pool.Current, 0.0001f);
        }

        [Test]
        public void SpendingRestartsTheDelay()
        {
            var pool = new StaminaPool(Max);
            pool.TrySpend(Cost, Delay);
            pool.Tick(Delay * 0.9f, Regen);
            pool.TrySpend(Cost, Delay);
            pool.Tick(Delay * 0.9f, Regen);

            Assert.AreEqual(Max - 2f * Cost, pool.Current, 0.0001f);
        }

        [Test]
        public void NeverRegensAboveMax()
        {
            var pool = new StaminaPool(Max);
            pool.TrySpend(Cost, Delay);
            pool.Tick(10f, Regen);

            Assert.AreEqual(Max, pool.Current);
        }
    }
}
