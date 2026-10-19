using GraceRun.Player;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class ActionAimTests
    {
        private const float Tolerance = 0.001f;

        private static Vector2 World(Vector2 localOffset, float angle) =>
            Quaternion.Euler(0f, 0f, angle) * localOffset;

        [Test]
        public void FlatAimKeepsTheOldMirroredHitbox()
        {
            var aim = Vector2.left;
            Assert.AreEqual(0f, ActionAim.HitboxAngle(aim), Tolerance);
            Assert.AreEqual(new Vector2(-0.8f, 0.1f), ActionAim.HitboxLocalOffset(new Vector2(0.8f, 0.1f), aim));
        }

        [Test]
        public void HitboxAimedUpLeftSitsUpLeft()
        {
            var aim = new Vector2(-1f, 1f).normalized;
            var world = World(ActionAim.HitboxLocalOffset(new Vector2(1f, 0f), aim), ActionAim.HitboxAngle(aim));
            Assert.AreEqual(aim.x, world.x, Tolerance);
            Assert.AreEqual(aim.y, world.y, Tolerance);
        }

        [Test]
        public void HitboxAimedStraightUpSitsAbove()
        {
            var world = World(ActionAim.HitboxLocalOffset(new Vector2(0.8f, 0f), Vector2.up), ActionAim.HitboxAngle(Vector2.up));
            Assert.AreEqual(0f, world.x, Tolerance);
            Assert.AreEqual(0.8f, world.y, Tolerance);
        }

        [Test]
        public void KnockbackTurnsWithTheAim()
        {
            var down = ActionAim.Knockback(new Vector2(10f, 0f), Vector2.down);
            Assert.AreEqual(0f, down.x, Tolerance);
            Assert.AreEqual(-10f, down.y, Tolerance);
            Assert.AreEqual(new Vector2(-10f, 5f), ActionAim.Knockback(new Vector2(10f, 5f), Vector2.left));
        }

        [Test]
        public void DodgeFollowsTheMoveInputDiagonally()
        {
            var dir = ActionAim.DodgeDirection(new Vector2(-1f, 1f), 1f, false);
            Assert.AreEqual(-Mathf.Sqrt(0.5f), dir.x, Tolerance);
            Assert.AreEqual(Mathf.Sqrt(0.5f), dir.y, Tolerance);
        }

        [Test]
        public void DodgeWithoutInputGoesAlongFacing() =>
            Assert.AreEqual(Vector2.left, ActionAim.DodgeDirection(Vector2.zero, -1f, true));

        [Test]
        public void GroundedDodgeNeverGoesIntoTheFloor()
        {
            Assert.AreEqual(Vector2.right, ActionAim.DodgeDirection(new Vector2(1f, -1f), 1f, true));
            Assert.AreEqual(Vector2.left, ActionAim.DodgeDirection(Vector2.down, -1f, true));
        }

        [Test]
        public void GroundedDodgeIsAlwaysAllowed() =>
            Assert.IsTrue(ActionAim.CanDodge(true, 5, 1));

        [Test]
        public void OnlyOneDodgeInTheAir()
        {
            Assert.IsTrue(ActionAim.CanDodge(false, 0, 1));
            Assert.IsFalse(ActionAim.CanDodge(false, 1, 1));
        }

        [Test]
        public void NoAirDodgesMeansNoneInTheAir() =>
            Assert.IsFalse(ActionAim.CanDodge(false, 0, 0));

        [Test]
        public void AirborneDodgeCanGoStraightDown() =>
            Assert.AreEqual(Vector2.down, ActionAim.DodgeDirection(Vector2.down, 1f, false));
    }
}
