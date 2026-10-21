using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GraceRun.Net
{
    /// <summary>
    /// Keeps a public room listed while the host is playing: publishes it, refreshes the player
    /// count and fight flag, and removes it on <see cref="Stop"/>. A failure only means the room is
    /// not listed; friends can still join with the code.
    /// </summary>
    public class PublicRoomPublisher : MonoBehaviour
    {
        private int _run;
        private string _lobbyId;

        public bool IsListed => _lobbyId != null;

        private void OnDestroy() => Stop();

        /// <summary>Lists the room and refreshes it every <paramref name="refreshSeconds"/> using the latest <paramref name="snapshot"/>.</summary>
        public async void Begin(Func<RoomListing> snapshot, float refreshSeconds)
        {
            var run = ++_run;
            try
            {
                var id = await LobbyConnector.CreateAsync(snapshot());
                if (run != _run || this == null)
                {
                    await LobbyConnector.DeleteAsync(id);
                    return;
                }

                _lobbyId = id;
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(refreshSeconds));
                    if (run != _run || this == null) return;
                    await LobbyConnector.RefreshAsync(id, snapshot());
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Public room listing stopped: {e.Message}");
                if (run == _run) _lobbyId = null;
            }
        }

        /// <summary>Takes the room off the list.</summary>
        public async void Stop()
        {
            _run++;
            var id = _lobbyId;
            _lobbyId = null;
            if (id == null) return;
            try
            {
                await LobbyConnector.DeleteAsync(id);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not remove the public room: {e.Message}");
            }
        }
    }
}
