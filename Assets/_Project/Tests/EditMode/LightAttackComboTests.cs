using System.Collections.Generic;
using GraceRun.Player;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class LightAttackComboTests
    {
        private const float Startup = 0.1f;
        private const float Active = 0.1f;
        private const float Recovery = 0.2f;
        private const float Buffer = 0.3f;
        private const float Chain = 0.35f;
        private const float Step = 0.02f;

        private static readonly LightAttackHit Hit =
            new(8, Vector2.zero, Vector2.one, Vector2.right, Startup, Active, Recovery, 10f);

        private LightAttackCombo _combo;
        private List<int> _started;

        [SetUp]
        public void SetUp()
        {
            _combo = new LightAttackCombo(new[] { Hit, Hit, Hit }, Buffer, Chain);
            _started = new List<int>();
            _combo.SwingStarted += _started.Add;
        }

        private void Run(float seconds)
        {
            for (var t = 0f; t < seconds - 0.0001f; t += Step) _combo.Tick(Step);
        }

        [Test]
        public void PressStartsTheFirstHitAtOnce()
        {
            _combo.Press();

            Assert.AreEqual(SwingPhase.Startup, _combo.Phase);
            CollectionAssert.AreEqual(new[] { 0 }, _started);
        }

        [Test]
        public void GoesThroughStartupActiveRecovery()
        {
            var activeAt = -1f;
            var endedAt = -1f;
            var time = 0f;
            _combo.ActiveStarted += _ => activeAt = time;
            _combo.SwingEnded += () => endedAt = time;

            _combo.Press();
            for (var i = 0; i < 30; i++) { time += Step; _combo.Tick(Step); }

            Assert.AreEqual(Startup, activeAt, Step);
            Assert.AreEqual(Startup + Active + Recovery, endedAt, Step);
        }

        [Test]
        public void PressDuringRecoveryChainsIntoTheNextHit()
        {
            _combo.Press();
            Run(Startup + Active + 0.05f);
            Assert.AreEqual(SwingPhase.Recovery, _combo.Phase);

            _combo.Press();
            Run(Recovery);

            CollectionAssert.AreEqual(new[] { 0, 1 }, _started);
        }

        [Test]
        public void ThreePressesGiveTheFullCombo()
        {
            var swing = Startup + Active + Recovery;
            _combo.Press();
            Run(swing - 0.1f);
            _combo.Press();
            Run(swing);
            _combo.Press();
            Run(swing);

            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, _started);
        }

        [Test]
        public void EarlyPressOutsideTheBufferIsDropped()
        {
            _combo.Press();
            _combo.Press(); // during startup, 0.4 s before the swing ends: older than the buffer
            Run(Startup + Active + Recovery + 0.02f);

            CollectionAssert.AreEqual(new[] { 0 }, _started);
            Assert.AreEqual(SwingPhase.Idle, _combo.Phase);
        }

        [Test]
        public void PressWithinTheChainWindowContinues()
        {
            _combo.Press();
            Run(Startup + Active + Recovery + Chain - 0.1f);
            _combo.Press();

            CollectionAssert.AreEqual(new[] { 0, 1 }, _started);
        }

        [Test]
        public void ComboRestartsAfterTheChainWindow()
        {
            _combo.Press();
            Run(Startup + Active + Recovery + Chain + 0.05f);
            _combo.Press();

            CollectionAssert.AreEqual(new[] { 0, 0 }, _started);
        }

        [Test]
        public void ComboRestartsAfterTheLastHit()
        {
            _combo.ForceStart(2);
            Run(Startup + Active + Recovery + 0.02f);

            Assert.AreEqual(0, _combo.NextHit);
        }

        [Test]
        public void RefusedStartDoesNotSwing()
        {
            _combo.CanStart = _ => false;
            _combo.Press();

            Assert.AreEqual(SwingPhase.Idle, _combo.Phase);
            Assert.IsEmpty(_started);
        }

        [Test]
        public void CancelDuringActiveEndsTheHitboxAndResets()
        {
            var activeEnded = 0;
            _combo.ActiveEnded += () => activeEnded++;
            _combo.Press();
            Run(Startup + 0.04f);
            Assert.AreEqual(SwingPhase.Active, _combo.Phase);

            _combo.Cancel();

            Assert.AreEqual(SwingPhase.Idle, _combo.Phase);
            Assert.AreEqual(1, activeEnded);
            Assert.AreEqual(0, _combo.NextHit);
        }

        [Test]
        public void StandAloneMoveStartsAtOnceAndDoesNotChain()
        {
            var combo = new LightAttackCombo(new[] { Hit, Hit, Hit }, Buffer, Chain, 2);
            var started = new List<int>();
            combo.SwingStarted += started.Add;

            combo.PressMove(2);
            for (var t = 0f; t < 1f; t += Step) combo.Tick(Step);
            combo.Press();

            CollectionAssert.AreEqual(new[] { 2, 0 }, started);
        }

        [Test]
        public void ComboOfTwoWrapsAfterTheSecondHit()
        {
            var combo = new LightAttackCombo(new[] { Hit, Hit, Hit }, Buffer, Chain, 2);
            var started = new List<int>();
            combo.SwingStarted += started.Add;

            for (var press = 0; press < 3; press++)
            {
                combo.Press();
                for (var t = 0f; t < 0.5f; t += Step) combo.Tick(Step);
            }

            CollectionAssert.AreEqual(new[] { 0, 1, 0 }, started);
        }

        [Test]
        public void BufferedMoveStartsWhenTheSwingEnds()
        {
            _combo.Press();
            Run(0.2f);
            _combo.PressMove(2);
            Run(Startup + Active + Recovery - 0.2f);

            CollectionAssert.AreEqual(new[] { 0, 2 }, _started);
        }

        [Test]
        public void ConfigureCancelsTheSwingAndSwapsTheHits()
        {
            var ended = 0;
            _combo.SwingEnded += () => ended++;
            _combo.Press();

            _combo.Configure(new[] { Hit }, Buffer, Chain);

            Assert.AreEqual(SwingPhase.Idle, _combo.Phase);
            Assert.AreEqual(1, ended);
            Assert.AreEqual(0, _combo.NextHit);
        }
    }
}
