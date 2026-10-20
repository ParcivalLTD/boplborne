using UnityEngine;

namespace GraceRun.Core
{
    /// <summary>
    /// Project-wide settings: session size, networking defaults and per-player identity.
    /// Values live in the GameConfig asset under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _maxPlayers;
        [SerializeField, Range(1, 65535)] private int _port;
        [SerializeField] private string _gameVersion;
        [SerializeField] private string _defaultAddress;
        [SerializeField, Min(1f)] private float _connectTimeoutSeconds = 10f;
        [SerializeField] private Color[] _playerColors;
        [SerializeField, Tooltip("Body shades per player; the sprite shader swaps P1's ramp for these.")]
        private PlayerRamp[] _playerRamps;
        [SerializeField] private Sprite[] _hatSprites;
        [SerializeField, Tooltip("Defaults and limits for the settings screen.")]
        private global::GraceRun.Core.Settings.SettingsData _settings;
        [SerializeField, Tooltip("Tuning for a dedicated server build that runs around the clock.")]
        private Net.ServerData _server;

        public int MaxPlayers => _maxPlayers;
        public ushort Port => (ushort)_port;
        public string GameVersion => _gameVersion;
        public string DefaultAddress => _defaultAddress;

        /// <summary>Defaults for the player-facing settings (volumes, shake, display).</summary>
        public global::GraceRun.Core.Settings.SettingsData Settings => _settings;

        /// <summary>Idle reset, status logging and frame rate of a dedicated server.</summary>
        public Net.ServerData Server => _server;

        /// <summary>How long the Join button waits for the host before giving up.</summary>
        public float ConnectTimeoutSeconds => _connectTimeoutSeconds;

        /// <summary>Color for player index 0-3 (P1 blue, P2 orange, P3 green, P4 pink).</summary>
        public Color GetPlayerColor(int playerIndex) => _playerColors[playerIndex];

        /// <summary>Body ramp for player index 0-3; index 0 is the P1 blue the art is drawn in.</summary>
        public PlayerRamp GetPlayerRamp(int playerIndex) => _playerRamps[playerIndex];

        /// <summary>Hat sprite for player index 0-3; null until hat art exists.</summary>
        public Sprite GetHatSprite(int playerIndex) =>
            playerIndex < _hatSprites.Length ? _hatSprites[playerIndex] : null;
    }
}
