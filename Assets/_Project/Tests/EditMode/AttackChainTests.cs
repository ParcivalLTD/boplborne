using System.Reflection;
using GraceRun.Bosses;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class AttackChainTests
    {
        private AttackData _first;
        private AttackData _second;

        [SetUp]
        public void SetUp()
        {
            _first = Make(0.9f, 0.15f, 0.25f); // 1.3 s
            _second = Make(0.85f, 0.15f, 1.3f); // 2.3 s
            Set(_first, "_followUp", _second);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
        }

        [TestCase(-0.1)]
        [TestCase(0.0)]
        [TestCase(1.29)]
        public void LinkAt_FirstLinkUntilItEnds(double elapsed)
        {
            Assert.AreSame(_first, AttackChain.LinkAt(_first, elapsed, out var start));
            Assert.AreEqual(0d, start);
        }

        [TestCase(1.31)]
        [TestCase(2.0)]
        [TestCase(10.0)]
        public void LinkAt_FollowUpStartsWhenTheFirstEnds(double elapsed)
        {
            Assert.AreSame(_second, AttackChain.LinkAt(_first, elapsed, out var start));
            Assert.AreEqual(1.3d, start, 1e-5);
        }

        [Test]
        public void LinkAt_FollowUpPhaseIsWindupRightAfterTheChain()
        {
            var link = AttackChain.LinkAt(_first, 1.4, out var start);
            Assert.AreEqual(AttackPhase.Windup, link.PhaseAt(1.4 - start));
        }

        [Test]
        public void LinkAt_WithoutFollowUpStaysOnTheAttack()
        {
            Assert.AreSame(_second, AttackChain.LinkAt(_second, 99.0, out var start));
            Assert.AreEqual(0d, start);
        }

        [Test]
        public void LinkAt_LoopingChainStops()
        {
            Set(_second, "_followUp", _first);
            Assert.IsNotNull(AttackChain.LinkAt(_first, 1000.0, out _));
        }

        private static AttackData Make(float windup, float active, float recovery)
        {
            var data = ScriptableObject.CreateInstance<AttackData>();
            Set(data, "_windup", windup);
            Set(data, "_active", active);
            Set(data, "_recovery", recovery);
            return data;
        }

        private static void Set(AttackData data, string field, object value) =>
            typeof(AttackData).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(data, value);
    }
}
