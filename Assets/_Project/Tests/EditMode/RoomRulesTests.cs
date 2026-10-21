using System.Collections.Generic;
using GraceRun.Net;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class RoomRulesTests
    {
        private const string Mine = "0.1.0";

        private static RoomListing Room(string host, int players = 1, string version = Mine, bool fight = false, string code = "K7M2QX") =>
            new(host, code, players, 4, version, fight);

        [Test]
        public void OpenRoomCanBeJoined()
        {
            Assert.AreEqual(RoomStatus.Open, RoomRules.StatusOf(Room("Ann"), Mine));
            Assert.IsTrue(RoomRules.CanJoin(Room("Ann"), Mine));
        }

        [Test]
        public void OtherVersionComesFirstInTheChecks()
        {
            Assert.AreEqual(RoomStatus.WrongVersion, RoomRules.StatusOf(Room("Ann", 4, "0.2.0", true), Mine));
        }

        [Test]
        public void FullAndFightingRoomsAreNotJoinable()
        {
            Assert.AreEqual(RoomStatus.Full, RoomRules.StatusOf(Room("Ann", 4), Mine));
            Assert.AreEqual(RoomStatus.InFight, RoomRules.StatusOf(Room("Ann", 2, Mine, true), Mine));
            Assert.IsFalse(RoomRules.CanJoin(Room("Ann", 4), Mine));
        }

        [Test]
        public void BrokenCodeCannotBeJoined() =>
            Assert.IsFalse(RoomRules.CanJoin(Room("Ann", 1, Mine, false, "nope"), Mine));

        [Test]
        public void SortPutsOpenFirstThenFullerRooms()
        {
            var rooms = new List<RoomListing>
            {
                Room("Zed", 4),
                Room("Bob", 1),
                Room("Cat", 3),
                Room("Dan", 2, "0.2.0"),
            };
            RoomRules.Sort(rooms, Mine);
            Assert.AreEqual("Cat", rooms[0].Host);
            Assert.AreEqual("Bob", rooms[1].Host);
        }
    }
}
