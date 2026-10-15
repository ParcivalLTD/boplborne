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
        [SerializeField] private Color[] _playerColors;
        [SerializeField] private Sprite[] _hatSprites;

        public int MaxPlayers => _maxPlayers;
        public ushort Port => (ushort)_port;
        public string GameVersion => _gameVersion;
        public string DefaultAddress => _defaultAddress;

        /// <summary>Color for player index 0-3 (P1 blue, P2 orange, P3 green, P4 pink).</summary>
        public Color GetPlayerColor(int playerIndex) => _playerColors[playerIndex];

        /// <summary>Hat sprite for player index 0-3; null until hat art exists.</summary>
        public Sprite GetHatSprite(int playerIndex) =>
            playerIndex < _hatSprites.Length ? _hatSprites[playerIndex] : null;
    }
}
