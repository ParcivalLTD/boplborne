using GraceRun.Player;
using GraceRun.Run;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class WeaponTests
    {
        private const float Threshold = 0.5f;

        private static LightAttackHit Hit(int damage) =>
            new(damage, Vector2.zero, Vector2.one, Vector2.right, 0.1f, 0.1f, 0.2f, 5f);

        [Test]
        public void HeavyButtonAlwaysGivesHeavy()
        {
            Assert.AreEqual(WeaponMove.Heavy, WeaponMoves.Select(true, true, 0f, Threshold));
            Assert.AreEqual(WeaponMove.Heavy, WeaponMoves.Select(true, false, 1f, Threshold));
        }

        [Test]
        public void LightButtonFollowsTheMoveStickAndTheGround()
        {
            Assert.AreEqual(WeaponMove.Light, WeaponMoves.Select(false, true, 0f, Threshold));
            Assert.AreEqual(WeaponMove.Aerial, WeaponMoves.Select(false, false, 0f, Threshold));
            Assert.AreEqual(WeaponMove.Up, WeaponMoves.Select(false, true, 0.8f, Threshold));
            Assert.AreEqual(WeaponMove.Up, WeaponMoves.Select(false, false, 0.5f, Threshold));
            Assert.AreEqual(WeaponMove.Down, WeaponMoves.Select(false, false, -0.9f, Threshold));
            Assert.AreEqual(WeaponMove.Light, WeaponMoves.Select(false, true, -0.3f, Threshold));
        }

        [Test]
        public void MoveSetPutsTheComboFirstThenTheOtherMovesInOrder()
        {
            var set = new WeaponMoveSet(new[] { Hit(1), Hit(2), Hit(3) }, Hit(10), Hit(20), Hit(30), Hit(40));

            Assert.AreEqual(7, set.Hits.Count);
            Assert.AreEqual(3, set.ComboLength);
            Assert.AreEqual(0, set.IndexFor(WeaponMove.Light));
            Assert.AreEqual(3, set.IndexFor(WeaponMove.Heavy));
            Assert.AreEqual(4, set.IndexFor(WeaponMove.Aerial));
            Assert.AreEqual(5, set.IndexFor(WeaponMove.Up));
            Assert.AreEqual(6, set.IndexFor(WeaponMove.Down));
            Assert.AreEqual(30, set.Hits[set.IndexFor(WeaponMove.Up)].Damage);
        }

        [Test]
        public void MissingMovesFallBackToTheLightCombo()
        {
            var set = new WeaponMoveSet(new[] { Hit(1), Hit(2) }, Hit(10), null, null, Hit(40));

            Assert.AreEqual(4, set.Hits.Count);
            Assert.AreEqual(0, set.IndexFor(WeaponMove.Aerial));
            Assert.AreEqual(0, set.IndexFor(WeaponMove.Up));
            Assert.AreEqual(3, set.IndexFor(WeaponMove.Down));
        }

        [Test]
        public void MoveOfNamesEachHitsMove()
        {
            var set = new WeaponMoveSet(new[] { Hit(1), Hit(2) }, Hit(10), Hit(20), null, null);

            Assert.AreEqual(WeaponMove.Light, set.MoveOf(1));
            Assert.AreEqual(WeaponMove.Heavy, set.MoveOf(2));
            Assert.AreEqual(WeaponMove.Aerial, set.MoveOf(3));
            Assert.AreEqual(WeaponMove.Light, set.MoveOf(99));
        }

        [Test]
        public void FinishersAreTheLastLightHitAndHeavy()
        {
            var set = new WeaponMoveSet(new[] { Hit(1), Hit(2), Hit(3) }, Hit(10), Hit(20), null, null);

            Assert.IsFalse(set.IsFinisher(0));
            Assert.IsTrue(set.IsFinisher(2));
            Assert.IsTrue(set.IsFinisher(3));
            Assert.IsFalse(set.IsFinisher(4));
        }

        [Test]
        public void WeaponsChangeOnlyInTheHubAndAtRestStops()
        {
            Assert.IsTrue(WeaponRules.CanChange(FightPhase.Draft, true));
            Assert.IsTrue(WeaponRules.CanChange(FightPhase.RestStop, false));
            Assert.IsFalse(WeaponRules.CanChange(FightPhase.Draft, false));
            Assert.IsFalse(WeaponRules.CanChange(FightPhase.Fighting, true));
            Assert.IsTrue(WeaponRules.CanChange(FightPhase.Waiting, true));
            Assert.IsFalse(WeaponRules.CanChange(FightPhase.Waiting, false));
            Assert.IsFalse(WeaponRules.CanChange(FightPhase.Countdown, true));
        }
    }
}
