using GraceRun.Abilities;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ComboLookupTests
    {
        private static readonly ComboRule[] Rules =
        {
            new(AbilityTags.Projectile, ComboSubject.Area, ComboInteraction.TakeForce, 1f),
            new(AbilityTags.Movement, ComboSubject.Summon, ComboInteraction.Latch),
            new(AbilityTags.Area, ComboSubject.Teammate, ComboInteraction.TakeForce, 0.5f),
            new(AbilityTags.Projectile, ComboSubject.Summon, ComboInteraction.Ricochet, 0.8f, 2),
        };

        [Test]
        public void ProjectileThroughAreaTakesForce()
        {
            Assert.IsTrue(ComboLookup.TryFind(Rules, AbilityTags.Projectile, ComboSubject.Area, out var rule));
            Assert.AreEqual(ComboInteraction.TakeForce, rule.Interaction);
        }

        [Test]
        public void MovementLatchesOnSummon()
        {
            Assert.IsTrue(ComboLookup.TryFind(Rules, AbilityTags.Movement, ComboSubject.Summon, out var rule));
            Assert.AreEqual(ComboInteraction.Latch, rule.Interaction);
        }

        [Test]
        public void AreaCatchesTeammate()
        {
            Assert.IsTrue(ComboLookup.TryFind(Rules, AbilityTags.Area, ComboSubject.Teammate, out var rule));
            Assert.AreEqual(0.5f, rule.Strength);
        }

        [Test]
        public void ProjectileRicochetsOffSummon()
        {
            Assert.IsTrue(ComboLookup.TryFind(Rules, AbilityTags.Projectile, ComboSubject.Summon, out var rule));
            Assert.AreEqual(ComboInteraction.Ricochet, rule.Interaction);
            Assert.AreEqual(2, rule.MaxRepeats);
        }

        [Test]
        public void SourceWithSeveralTagsMatchesAnyOfThem()
        {
            var bomb = AbilityTags.Projectile | AbilityTags.Area;
            Assert.IsTrue(ComboLookup.TryFind(Rules, bomb, ComboSubject.Teammate, out var rule));
            Assert.AreEqual(AbilityTags.Area, rule.Source);
        }

        [Test]
        public void TargetWithSeveralTagsMatchesAnyOfThem()
        {
            var spikeTrap = ComboSubject.Summon | ComboSubject.Area;
            Assert.IsTrue(ComboLookup.TryFind(Rules, AbilityTags.Movement, spikeTrap, out var rule));
            Assert.AreEqual(ComboInteraction.Latch, rule.Interaction);
        }

        [Test]
        public void FirstMatchWins()
        {
            var bomb = AbilityTags.Projectile | AbilityTags.Area;
            Assert.IsTrue(ComboLookup.TryFind(Rules, bomb, ComboSubject.Area, out var rule));
            Assert.AreEqual(AbilityTags.Projectile, rule.Source);
        }

        [Test]
        public void NoRuleMeansNoCombo()
        {
            Assert.IsFalse(ComboLookup.TryFind(Rules, AbilityTags.Summon, ComboSubject.Movement, out var rule));
            Assert.AreEqual(ComboInteraction.None, rule.Interaction);
            Assert.IsFalse(ComboLookup.TryFind(Rules, AbilityTags.Movement, ComboSubject.Teammate, out _));
        }

        [Test]
        public void NoneTagsAndMissingRulesNeverMatch()
        {
            Assert.IsFalse(ComboLookup.TryFind(Rules, AbilityTags.None, ComboSubject.Area, out _));
            Assert.IsFalse(ComboLookup.TryFind(Rules, AbilityTags.Projectile, ComboSubject.None, out _));
            Assert.IsFalse(ComboLookup.TryFind(null, AbilityTags.Projectile, ComboSubject.Area, out _));
        }

        [Test]
        public void NoneRulesAreSkipped()
        {
            var rules = new[]
            {
                new ComboRule(AbilityTags.Projectile, ComboSubject.Area, ComboInteraction.None),
                new ComboRule(AbilityTags.Projectile, ComboSubject.Area, ComboInteraction.Absorb),
            };
            Assert.IsTrue(ComboLookup.TryFind(rules, AbilityTags.Projectile, ComboSubject.Area, out var rule));
            Assert.AreEqual(ComboInteraction.Absorb, rule.Interaction);
        }

        [Test]
        public void SubjectBitsMatchAbilityTags()
        {
            Assert.AreEqual(ComboSubject.Projectile | ComboSubject.Area,
                ComboLookup.SubjectOf(AbilityTags.Projectile | AbilityTags.Area));
            Assert.AreEqual(ComboSubject.Summon, ComboLookup.SubjectOf(AbilityTags.Summon));
        }
    }
}
