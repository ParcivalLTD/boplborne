using System;
using System.Text;
using UnityEngine;

namespace GraceRun.Net
{
    /// <summary>
    /// What a joining client sends with its connection request, checked by <see cref="ConnectionApproval"/>.
    /// </summary>
    [Serializable]
    public struct ConnectionPayload
    {
        public string GameVersion;

        /// <summary>The joining player's nickname. Anything can arrive here; the server cleans it (<see cref="NicknameRules"/>).</summary>
        public string Nickname;

        public static byte[] Serialize(string gameVersion, string nickname = "") =>
            Encoding.UTF8.GetBytes(JsonUtility.ToJson(new ConnectionPayload { GameVersion = gameVersion, Nickname = nickname }));

        public static bool TryDeserialize(byte[] data, out ConnectionPayload payload)
        {
            payload = default;
            if (data == null || data.Length == 0) return false;

            try
            {
                payload = JsonUtility.FromJson<ConnectionPayload>(Encoding.UTF8.GetString(data));
            }
            catch (ArgumentException)
            {
                return false;
            }

            return !string.IsNullOrEmpty(payload.GameVersion);
        }
    }
}
