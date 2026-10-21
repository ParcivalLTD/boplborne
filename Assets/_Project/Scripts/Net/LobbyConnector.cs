using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace GraceRun.Net
{
    /// <summary>
    /// Talks to the Unity Lobby service, which is only the public room list: a public host
    /// publishes one lobby holding its Relay join code and player count, and the browser reads them.
    /// The lobby never carries players; they connect over Relay as usual. Throws when the service is unreachable.
    /// </summary>
    public static class LobbyConnector
    {
        private const string CodeKey = "code";
        private const string HostKey = "host";
        private const string PlayersKey = "players";
        private const string MaxKey = "max";
        private const string VersionKey = "version";
        private const string FightKey = "fight";
        private const int MaxListed = 20;

        /// <summary>Publishes a public room and returns its lobby id.</summary>
        public static async Task<string> CreateAsync(RoomListing room)
        {
            await SignInAsync();
            var options = new CreateLobbyOptions { IsPrivate = false, Data = Data(room) };
            // The lobby always seats only the host; the real player count is in the data.
            var lobby = await LobbyService.Instance.CreateLobbyAsync($"{room.Host}'s room", 2, options);
            return lobby.Id;
        }

        /// <summary>Keeps the room alive and refreshes its player count and fight flag.</summary>
        public static async Task RefreshAsync(string lobbyId, RoomListing room)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions { Data = Data(room) });
        }

        public static async Task DeleteAsync(string lobbyId)
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }

        /// <summary>Lists the public rooms (unsorted).</summary>
        public static async Task<List<RoomListing>> ListAsync()
        {
            await SignInAsync();
            var response = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions { Count = MaxListed });
            var rooms = new List<RoomListing>();
            foreach (var lobby in response.Results)
                if (TryRead(lobby, out var room)) rooms.Add(room);
            return rooms;
        }

        private static Dictionary<string, DataObject> Data(RoomListing room) => new()
        {
            [CodeKey] = new DataObject(DataObject.VisibilityOptions.Public, room.JoinCode),
            [HostKey] = new DataObject(DataObject.VisibilityOptions.Public, room.Host),
            [PlayersKey] = new DataObject(DataObject.VisibilityOptions.Public, room.Players.ToString()),
            [MaxKey] = new DataObject(DataObject.VisibilityOptions.Public, room.MaxPlayers.ToString()),
            [VersionKey] = new DataObject(DataObject.VisibilityOptions.Public, room.Version),
            [FightKey] = new DataObject(DataObject.VisibilityOptions.Public, room.InFight ? "1" : "0"),
        };

        private static bool TryRead(Lobby lobby, out RoomListing room)
        {
            room = default;
            var data = lobby.Data;
            if (data == null || !data.TryGetValue(CodeKey, out var code) || !data.TryGetValue(HostKey, out var host)) return false;
            if (!int.TryParse(Value(data, PlayersKey), out var players) || !int.TryParse(Value(data, MaxKey), out var max)) return false;
            room = new RoomListing(host.Value, code.Value, players, max, Value(data, VersionKey), Value(data, FightKey) == "1");
            return true;
        }

        private static string Value(Dictionary<string, DataObject> data, string key) =>
            data.TryGetValue(key, out var value) ? value.Value : string.Empty;

        private static async Task SignInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
