using GraceRun.Combat;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class DamageMathTests
    {
        private const float FriendlyMultiplier = 0.1f;
        private const int FriendlyMinHP = 1;

        private static int Resolve(int current, int amount, bool friendly = false, bool god = false) =>
            DamageMath.Resolve(current, amount, friendly, FriendlyMultiplier, FriendlyMinHP, god);

        [Test]
        public void NormalHitSubtractsDamage()
        {
            Assert.AreEqual(80, Resolve(100, 20));
        }

        [Test]
        public void HpNeverGoesBelowZero()
        {
            Assert.AreEqual(0, Resolve(15, 35));
        }

        [Test]
        public void ZeroOrNegativeDamageChangesNothing()
        {
            Assert.AreEqual(50, Resolve(50, 0));
            Assert.AreEqual(50, Resolve(50, -10));
        }

        [Test]
        public void GodModeIgnoresDamage()
        {
            Assert.AreEqual(100, Resolve(100, 999, god: true));
        }

        [Test]
        public void FriendlyFireDealsTenPercent()
        {
            Assert.AreEqual(97, Resolve(100, 30, friendly: true));
        }

        [Test]
        public void FriendlyFireDealsAtLeastOne()
        {
            Assert.AreEqual(99, Resolve(100, 8, friendly: true));
        }

        [Test]
        public void FriendlyFireCannotGoBelowOneHp()
        {
            Assert.AreEqual(1, Resolve(3, 100, friendly: true));
            Assert.AreEqual(1, Resolve(1, 100, friendly: true));
        }

        [Test]
        public void HitAtOrBelowFloorChangesNothing()
        {
            Assert.AreEqual(0, DamageMath.HpAfterHit(0, 5, FriendlyMinHP));
        }

        [Test]
        public void ZeroMultiplierTurnsFriendlyDamageOff()
        {
            Assert.AreEqual(0, DamageMath.DamageDealt(30, true, 0f));
        }

        [Test]
        public void OnlyPlayerOnOtherPlayerIsFriendlyFire()
        {
            Assert.IsTrue(DamageMath.IsFriendlyFire(Team.Player, 1, Team.Player, 2));
            Assert.IsFalse(DamageMath.IsFriendlyFire(Team.Player, 1, Team.Player, 1), "hitting yourself");
            Assert.IsFalse(DamageMath.IsFriendlyFire(Team.Player, 1, Team.Boss, 2));
            Assert.IsFalse(DamageMath.IsFriendlyFire(Team.Boss, 1, Team.Player, 2));
            Assert.IsFalse(DamageMath.IsFriendlyFire(Team.Neutral, DamageInfo.NoAttacker, Team.Player, 2));
        }
    }
}
