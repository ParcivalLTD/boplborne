using System.Collections.Generic;
using GraceRun.Bosses;
using GraceRun.Combat;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class BossFeelTests
    {
        private const float Eps = 0.0001f;

        [Test]
        public void IdleScaleIsOneForOnePlayerAndShrinksPerExtraPlayer()
        {
            Assert.AreEqual(1f, BossFeelMath.IdleScale(1, 0.1f), Eps);
            Assert.AreEqual(1f, BossFeelMath.IdleScale(0, 0.1f), Eps);
            Assert.AreEqual(0.9f, BossFeelMath.IdleScale(2, 0.1f), Eps);
            Assert.AreEqual(0.7f, BossFeelMath.IdleScale(4, 0.1f), Eps);
        }

        [Test]
        public void IdleScaleFloorsAtHalfAndNeverExceedsOne()
        {
            Assert.AreEqual(0.5f, BossFeelMath.IdleScale(4, 0.5f), Eps);
            Assert.AreEqual(0.5f, BossFeelMath.IdleScale(50, 0.1f), Eps);
            Assert.AreEqual(1f, BossFeelMath.IdleScale(3, -0.2f), Eps);
        }

        [Test]
        public void PressureChanceIsZeroUpToTwoPlayers()
        {
            Assert.AreEqual(0f, BossFeelMath.PressureChance(0, 0.2f), Eps);
            Assert.AreEqual(0f, BossFeelMath.PressureChance(1, 0.2f), Eps);
            Assert.AreEqual(0f, BossFeelMath.PressureChance(2, 0.2f), Eps);
        }

        [Test]
        public void PressureChanceGrowsPerExtraPlayerAndCapsAtOne()
        {
            Assert.AreEqual(0.2f, BossFeelMath.PressureChance(3, 0.2f), Eps);
            Assert.AreEqual(0.4f, BossFeelMath.PressureChance(4, 0.2f), Eps);
            Assert.AreEqual(1f, BossFeelMath.PressureChance(20, 0.2f), Eps);
            Assert.AreEqual(0f, BossFeelMath.PressureChance(4, 0f), Eps);
        }

        [Test]
        public void ScaledPoolGrowsPerExtraPlayerOnly()
        {
            Assert.AreEqual(100f, BossFeelMath.ScaledPool(100f, 0.5f, 1), Eps);
            Assert.AreEqual(100f, BossFeelMath.ScaledPool(100f, 0.5f, 0), Eps);
            Assert.AreEqual(200f, BossFeelMath.ScaledPool(100f, 0.5f, 3), Eps);
        }

        [Test]
        public void WeakPointWindowIncludesBothEdgesAndTheGrace()
        {
            // punish 0.5 + grace 0.25 gives the window [0, 0.75]
            Assert.IsTrue(BossFeelMath.InWeakPointWindow(true, 0f, 0.5f, 0.25f));
            Assert.IsTrue(BossFeelMath.InWeakPointWindow(true, 0.6f, 0.5f, 0.25f));
            Assert.IsTrue(BossFeelMath.InWeakPointWindow(true, 0.75f, 0.5f, 0.25f));
            Assert.IsFalse(BossFeelMath.InWeakPointWindow(true, 0.751f, 0.5f, 0.25f));
            Assert.IsFalse(BossFeelMath.InWeakPointWindow(true, -0.001f, 0.5f, 0.25f));
        }

        [Test]
        public void WeakPointWindowNeedsTheBossToBeRecovering()
        {
            Assert.IsFalse(BossFeelMath.InWeakPointWindow(false, 0.1f, 0.5f, 0.25f));
        }

        [Test]
        public void PickVariantIsNoneWhenTheRollMissesTheChance()
        {
            // 0.25 is exact in binary, so roll == chance is a true tie
            Assert.AreEqual(BossVariant.None, BossFeelMath.PickVariant(0.25f, 0.25, 0));
            Assert.AreEqual(BossVariant.None, BossFeelMath.PickVariant(0.25f, 0.9, 2));
            Assert.AreEqual(BossVariant.None, BossFeelMath.PickVariant(0f, 0.0, 1));
        }

        [Test]
        public void PickVariantGivesAVariantWhenTheRollHitsTheChance()
        {
            Assert.AreEqual(BossVariant.Swift, BossFeelMath.PickVariant(0.25f, 0.2499, 0));
            Assert.AreEqual(BossVariant.Swift, BossFeelMath.PickVariant(1f, 0.0, 0));
            Assert.AreEqual(BossVariant.Brutal, BossFeelMath.PickVariant(1f, 0.0, 1));
            Assert.AreEqual(BossVariant.Tough, BossFeelMath.PickVariant(1f, 0.0, 2));
            Assert.AreEqual(BossVariant.Relentless, BossFeelMath.PickVariant(1f, 0.0, 3));
            Assert.AreEqual(BossVariant.Swift, BossFeelMath.PickVariant(1f, 0.0, 4));
            Assert.AreEqual(BossVariant.Brutal, BossFeelMath.PickVariant(1f, 0.0, -1));
            Assert.AreEqual(BossVariant.Relentless, BossFeelMath.PickVariant(1f, 0.0, -3));
        }

        [Test]
        public void PickVariantOnlyReturnsNoneOrAVariantInRange()
        {
            for (var pick = -50; pick <= 50; pick++)
            {
                var variant = BossFeelMath.PickVariant(1f, 0.5, pick);
                Assert.That((int)variant, Is.InRange(1, 4), "pick " + pick);
            }
        }

        [Test]
        public void VariantScalesTouchOnlyTheirOwnStat()
        {
            Assert.AreEqual(1.25f, BossFeelMath.DamageScale(BossVariant.Brutal), Eps);
            Assert.AreEqual(1f, BossFeelMath.DamageScale(BossVariant.Swift), Eps);
            Assert.AreEqual(1.25f, BossFeelMath.HpScale(BossVariant.Tough), Eps);
            Assert.AreEqual(1f, BossFeelMath.HpScale(BossVariant.Brutal), Eps);
            Assert.AreEqual(0.75f, BossFeelMath.IdleScale(BossVariant.Swift), Eps);
            Assert.AreEqual(0.85f, BossFeelMath.IdleScale(BossVariant.Relentless), Eps);
            Assert.AreEqual(1f, BossFeelMath.IdleScale(BossVariant.None), Eps);
            Assert.AreEqual(1.2f, BossFeelMath.SpeedScale(BossVariant.Swift), Eps);
            Assert.AreEqual(1f, BossFeelMath.SpeedScale(BossVariant.Tough), Eps);
        }

        [Test]
        public void PoiseBreaksOnlyOnceWhenItFills()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            Assert.IsFalse(poise.Add(4f));
            Assert.IsFalse(poise.IsBroken);
            Assert.IsTrue(poise.Add(6f));
            Assert.IsTrue(poise.IsBroken);
            Assert.IsFalse(poise.Add(3f));
            Assert.AreEqual(10f, poise.Value, Eps);
        }

        [Test]
        public void PoiseOverfillBreaksInOneHitAndCapsAtMax()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            Assert.IsTrue(poise.Add(15f));
            Assert.AreEqual(10f, poise.Value, Eps);
            Assert.AreEqual(1f, poise.Normalized, Eps);
        }

        [Test]
        public void PoiseIgnoresZeroDamageAndUnconfiguredMeters()
        {
            var poise = new PoiseMeter();
            Assert.IsFalse(poise.Add(5f));
            poise.Configure(10f);
            Assert.IsFalse(poise.Add(0f));
            Assert.IsFalse(poise.Add(-2f));
            Assert.AreEqual(0f, poise.Value, Eps);
        }

        [Test]
        public void PoiseNormalizedReportsTheFilledShare()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            poise.Add(5f);
            Assert.AreEqual(0.5f, poise.Normalized, Eps);
        }

        [Test]
        public void PoiseRegensOnlyAfterTheQuietDelay()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            poise.Add(5f);
            poise.Tick(0.9f, 2f, 1f);
            Assert.AreEqual(5f, poise.Value, Eps);
            // 1.9s since the hit: this tick drains 2 * 1s
            poise.Tick(1f, 2f, 1f);
            Assert.AreEqual(3f, poise.Value, Eps);
        }

        [Test]
        public void PoiseHitRestartsTheQuietDelay()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            poise.Add(2f);
            poise.Tick(0.9f, 1f, 1f);
            poise.Add(1f);
            poise.Tick(0.9f, 1f, 1f);
            Assert.AreEqual(3f, poise.Value, Eps);
        }

        [Test]
        public void PoiseRegenFloorsAtZero()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            poise.Add(2f);
            poise.Tick(5f, 100f, 0f);
            Assert.AreEqual(0f, poise.Value, Eps);
        }

        [Test]
        public void BrokenPoiseDoesNotDrain()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            poise.Add(10f);
            poise.Tick(5f, 100f, 0f);
            Assert.IsTrue(poise.IsBroken);
            Assert.AreEqual(10f, poise.Value, Eps);
        }

        [Test]
        public void PoiseResetClearsTheMeterAndCanBreakAgain()
        {
            var poise = new PoiseMeter();
            poise.Configure(10f);
            poise.Add(10f);
            poise.Reset();
            Assert.IsFalse(poise.IsBroken);
            Assert.AreEqual(0f, poise.Value, Eps);
            Assert.AreEqual(0f, poise.Normalized, Eps);
            Assert.IsTrue(poise.Add(10f));
        }

        [Test]
        public void PatternIsBlockedOnceTheSameAttackHitsMaxRepeats()
        {
            var memory = new PatternMemory();
            Assert.IsFalse(memory.IsBlocked(1, 2));
            memory.Record(1);
            Assert.IsFalse(memory.IsBlocked(1, 2));
            memory.Record(1);
            Assert.IsTrue(memory.IsBlocked(1, 2));
            memory.Record(1);
            Assert.IsTrue(memory.IsBlocked(1, 2));
        }

        [Test]
        public void PatternBlocksOnlyTheRepeatedAttack()
        {
            var memory = new PatternMemory();
            memory.Record(1);
            memory.Record(1);
            Assert.IsTrue(memory.IsBlocked(1, 2));
            Assert.IsFalse(memory.IsBlocked(2, 2));
        }

        [Test]
        public void DifferentAttackResetsTheRepeatCount()
        {
            var memory = new PatternMemory();
            memory.Record(1);
            memory.Record(1);
            memory.Record(2);
            Assert.IsFalse(memory.IsBlocked(1, 2));
            Assert.IsFalse(memory.IsBlocked(2, 2));
            memory.Record(1);
            Assert.IsFalse(memory.IsBlocked(1, 2));
        }

        [Test]
        public void MaxRepeatsOfOneBlocksAtOnceAndClearForgets()
        {
            var memory = new PatternMemory();
            memory.Record(3);
            Assert.IsTrue(memory.IsBlocked(3, 1));
            memory.Clear();
            Assert.IsFalse(memory.IsBlocked(3, 1));
        }

        [Test]
        public void AdaptationIsZeroUnderMinHits()
        {
            var tracker = new AdaptationTracker();
            tracker.Record(5, 0f);
            tracker.Record(5, 0.1f);
            Assert.AreEqual((ushort)0, tracker.Dominant(0.2f, 10f, 3, 0.5f));
            tracker.Record(5, 0.2f);
            Assert.AreEqual((ushort)5, tracker.Dominant(0.2f, 10f, 3, 0.5f));
        }

        [Test]
        public void AdaptationNeedsTheShareOfHits()
        {
            var tracker = new AdaptationTracker();
            tracker.Record(5, 0f);
            tracker.Record(5, 0.1f);
            tracker.Record(5, 0.2f);
            tracker.Record(7, 0.3f);
            Assert.AreEqual((ushort)5, tracker.Dominant(0.3f, 10f, 1, 0.75f));
            Assert.AreEqual((ushort)0, tracker.Dominant(0.3f, 10f, 1, 0.8f));
        }

        [Test]
        public void AdaptationShareIsInclusive()
        {
            var tracker = new AdaptationTracker();
            tracker.Record(5, 0f);
            tracker.Record(5, 0.1f);
            tracker.Record(7, 0.2f);
            tracker.Record(7, 0.3f);
            Assert.AreNotEqual((ushort)0, tracker.Dominant(0.3f, 10f, 1, 0.5f));
            Assert.AreEqual((ushort)0, tracker.Dominant(0.3f, 10f, 1, 0.6f));
        }

        [Test]
        public void AdaptationDropsHitsOutsideTheWindow()
        {
            var tracker = new AdaptationTracker();
            for (var i = 0; i < 3; i++) tracker.Record(5, 0f);
            for (var i = 0; i < 3; i++) tracker.Record(7, 100f);
            Assert.AreEqual((ushort)7, tracker.Dominant(100f, 10f, 3, 0.5f));
        }

        [Test]
        public void AdaptationOldHitsDoNotCountTowardMinHitsLater()
        {
            var tracker = new AdaptationTracker();
            for (var i = 0; i < 3; i++) tracker.Record(5, 0f);
            Assert.AreEqual((ushort)0, tracker.Dominant(100f, 10f, 3, 0.5f));
            tracker.Record(7, 100f);
            tracker.Record(7, 100f);
            Assert.AreEqual((ushort)0, tracker.Dominant(100f, 10f, 3, 0.5f));
            tracker.Record(7, 100f);
            Assert.AreEqual((ushort)7, tracker.Dominant(100f, 10f, 3, 0.5f));
        }

        [Test]
        public void AdaptationKeepsAHitExactlyOnTheWindowEdge()
        {
            var edge = new AdaptationTracker();
            edge.Record(4, 0f);
            Assert.AreEqual((ushort)4, edge.Dominant(10f, 10f, 1, 1f));

            var past = new AdaptationTracker();
            past.Record(4, 0f);
            Assert.AreEqual((ushort)0, past.Dominant(10.5f, 10f, 1, 1f));
        }

        [Test]
        public void AdaptationClearForgetsAllHits()
        {
            var tracker = new AdaptationTracker();
            for (var i = 0; i < 3; i++) tracker.Record(5, 0f);
            tracker.Clear();
            Assert.AreEqual((ushort)0, tracker.Dominant(0f, 10f, 1, 0.5f));
        }

        [Test]
        public void DamageSourceOfAbilityIsZeroForNullOrEmpty()
        {
            Assert.AreEqual(DamageSource.Unknown, DamageSource.OfAbility(null));
            Assert.AreEqual(DamageSource.Unknown, DamageSource.OfAbility(""));
        }

        [Test]
        public void DamageSourceOfAbilityNeverReturnsZeroOrMelee()
        {
            for (var i = 0; i < 5000; i++)
            {
                var id = "ability_" + i;
                var source = DamageSource.OfAbility(id);
                Assert.That(source, Is.Not.EqualTo(DamageSource.Unknown), id);
                Assert.That(source, Is.Not.EqualTo(DamageSource.Melee), id);
            }
        }

        [Test]
        public void DamageSourceOfAbilityIsStableForTheSameId()
        {
            var first = DamageSource.OfAbility("dash");
            Assert.AreEqual(first, DamageSource.OfAbility("dash"));
            Assert.AreEqual(first, DamageSource.OfAbility(new string("dash".ToCharArray())));
        }

        [Test]
        public void DamageSourceOfAbilitySpreadsDifferentIds()
        {
            var seen = new HashSet<ushort>();
            for (var i = 0; i < 200; i++) seen.Add(DamageSource.OfAbility("ability_" + i));
            Assert.That(seen.Count, Is.GreaterThan(150));
        }
    }
}
