using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class FlaskRulesTests
    {
        private const int Max = 100;
        private const int Heal = 40;

        [Test]
        public void HurtPlayerWithAChargeCanDrink()
        {
            Assert.IsTrue(FlaskRules.CanDrink(2, 50, Max));
        }

        [Test]
        public void NoChargesLeftRefuses()
        {
            Assert.IsFalse(FlaskRules.CanDrink(0, 50, Max));
        }

        [Test]
        public void FullHPRefuses()
        {
            Assert.IsFalse(FlaskRules.CanDrink(2, Max, Max));
        }

        [Test]
        public void GhostCannotDrink()
        {
            Assert.IsFalse(FlaskRules.CanDrink(2, 0, Max));
        }

        [Test]
        public void HealsByTheFlaskAmount()
        {
            Assert.AreEqual(70, FlaskRules.HealedHP(30, Max, Heal));
        }

        [Test]
        public void HealIsCappedAtMax()
        {
            Assert.AreEqual(Max, FlaskRules.HealedHP(80, Max, Heal));
        }

        [Test]
        public void NegativeHealDoesNothing()
        {
            Assert.AreEqual(30, FlaskRules.HealedHP(30, Max, -10));
        }
    }
}
