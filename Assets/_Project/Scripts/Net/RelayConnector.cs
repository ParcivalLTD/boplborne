using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace GraceRun.Net
{
    /// <summary>A Relay room the host opened: the transport data and the code friends type to join.</summary>
    public readonly struct RelayRoom
    {
        public readonly RelayServerData Data;
        public readonly string Code;

        public RelayRoom(RelayServerData data, string code)
        {
            Data = data;
            Code = code;
        }
    }

    /// <summary>
    /// Talks to Unity Relay so players can host and join across the internet with a short code,
    /// with no port forwarding or VPN. Throws when the services are unreachable or the project
    /// is not linked to Unity Cloud; the caller falls back to a direct connection.
    /// </summary>
    public static class RelayConnector
    {
        private const string ConnectionType = "dtls";

        /// <summary>Opens a room for <paramref name="maxPlayers"/> (the host plays, so the Relay gets one fewer).</summary>
        public static async Task<RelayRoom> CreateRoomAsync(int maxPlayers)
        {
            await SignInAsync();
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            var code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            var endpoint = Endpoint(allocation.ServerEndpoints);
            var data = new RelayServerData(endpoint.Host, (ushort)endpoint.Port, allocation.AllocationIdBytes,
                allocation.ConnectionData, allocation.ConnectionData, allocation.Key, endpoint.Secure);
            return new RelayRoom(data, code);
        }

        /// <summary>Looks the room up by its code.</summary>
        public static async Task<RelayServerData> JoinRoomAsync(string code)
        {
            await SignInAsync();
            var allocation = await RelayService.Instance.JoinAllocationAsync(JoinCodeRules.Normalize(code));
            var endpoint = Endpoint(allocation.ServerEndpoints);
            return new RelayServerData(endpoint.Host, (ushort)endpoint.Port, allocation.AllocationIdBytes,
                allocation.ConnectionData, allocation.HostConnectionData, allocation.Key, endpoint.Secure);
        }

        private static RelayServerEndpoint Endpoint(System.Collections.Generic.List<RelayServerEndpoint> endpoints)
        {
            foreach (var endpoint in endpoints)
                if (endpoint.ConnectionType == ConnectionType) return endpoint;
            throw new System.InvalidOperationException($"Relay offered no {ConnectionType} endpoint.");
        }

        private static async Task SignInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
