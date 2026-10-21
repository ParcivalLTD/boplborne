using GraceRun.Player;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class BodyMovesTests
    {
        [TestCase(0, BodyMove.LightA)]
        [TestCase(1, BodyMove.LightB)]
        [TestCase(2, BodyMove.LightA)]
        public void Light_AlternatesBetweenTwoClips(int hit, BodyMove expected) =>
            Assert.AreEqual(expected, BodyMoves.Pick(WeaponMove.Light, false, hit));

        [Test]
        public void LightFinisher_UsesTheHeavyClip() =>
            Assert.AreEqual(BodyMove.Heavy, BodyMoves.Pick(WeaponMove.Light, true, 2));

        [Test]
        public void SpecialMoves_HaveTheirOwnClip()
        {
            Assert.AreEqual(BodyMove.Heavy, BodyMoves.Pick(WeaponMove.Heavy, false, 3));
            Assert.AreEqual(BodyMove.Aerial, BodyMoves.Pick(WeaponMove.Aerial, false, 4));
            Assert.AreEqual(BodyMove.Up, BodyMoves.Pick(WeaponMove.Up, false, 5));
            Assert.AreEqual(BodyMove.Down, BodyMoves.Pick(WeaponMove.Down, false, 6));
        }
    }
}
