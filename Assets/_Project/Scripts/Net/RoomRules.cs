using System.Collections.Generic;

namespace GraceRun.Net
{
    /// <summary>Whether a public room can be joined right now, and if not, why.</summary>
    public enum RoomStatus : byte
    {
        Open,
        InFight,
        Full,
        WrongVersion,
    }

    /// <summary>Pure rules for the public room browser: status, order and the text next to each room.</summary>
    public static class RoomRules
    {
        public static RoomStatus StatusOf(in RoomListing room, string myVersion)
        {
            if (room.Version != myVersion) return RoomStatus.WrongVersion;
            if (room.Players >= room.MaxPlayers) return RoomStatus.Full;
            if (room.InFight) return RoomStatus.InFight;
            return RoomStatus.Open;
        }

        public static bool CanJoin(in RoomListing room, string myVersion) =>
            StatusOf(room, myVersion) == RoomStatus.Open && JoinCodeRules.LooksLikeCode(room.JoinCode);

        public static string StatusText(RoomStatus status) => status switch
        {
            RoomStatus.Open => "Open",
            RoomStatus.InFight => "In a fight",
            RoomStatus.Full => "Full",
            _ => "Other version",
        };

        /// <summary>Joinable rooms first, then fuller rooms first (people want company), then by host name.</summary>
        public static void Sort(List<RoomListing> rooms, string myVersion)
        {
            rooms.Sort((a, b) =>
            {
                var byStatus = StatusOf(a, myVersion).CompareTo(StatusOf(b, myVersion));
                if (byStatus != 0) return byStatus;
                var byPlayers = b.Players.CompareTo(a.Players);
                return byPlayers != 0 ? byPlayers : string.CompareOrdinal(a.Host, b.Host);
            });
        }
    }
}
