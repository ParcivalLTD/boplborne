using System.Collections.Generic;
using GraceRun.Core;
using GraceRun.Player;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Net
{
    /// <summary>
    /// Server side: gives every connected client a player object at its own spawn point and
    /// frees the slot again when that client leaves. Lives in the Game scene.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private NetworkManager _net;
        private PlayerSlots _slots;

        private void Start()
        {
            _net = NetworkManager.Singleton;
            if (_net == null || !_net.IsServer)
            {
                enabled = false;
                return;
            }

            _slots = new PlayerSlots(_config.MaxPlayers);
            _net.OnClientConnectedCallback += SpawnFor;
            _net.OnClientDisconnectCallback += ReleaseSlot;

            var connected = new List<ulong>(_net.ConnectedClientsIds);
            foreach (var clientId in connected) SpawnFor(clientId);
        }

        private void OnDestroy()
        {
            if (_net == null) return;
            _net.OnClientConnectedCallback -= SpawnFor;
            _net.OnClientDisconnectCallback -= ReleaseSlot;
        }

        private void SpawnFor(ulong clientId)
        {
            if (_slots.Has(clientId)) return;
            if (!_slots.TryTake(clientId, out var slot))
            {
                Debug.LogWarning($"No free player slot for client {clientId}.");
                return;
            }

            var point = _spawnPoints[slot % _spawnPoints.Length];
            var player = Instantiate(_playerPrefab, point.position, Quaternion.identity);
            player.SpawnAsPlayerObject(clientId);
            player.GetComponent<PlayerIdentity>().AssignIndex(slot);
        }

        private void ReleaseSlot(ulong clientId) => _slots.Release(clientId);
    }
}
