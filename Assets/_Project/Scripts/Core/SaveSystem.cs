using System;
using System.IO;
using UnityEngine;

namespace GraceRun.Core
{
    /// <summary>
    /// Resilient local file persistence using atomic writes and JSON serialization (docs/ARCHITECTURE.md).
    /// Prevents file corruption during crashes or mid-write interruptions.
    /// </summary>
    public static class SaveSystem
    {
        /// <summary>
        /// Serializes <paramref name="data"/> to JSON and saves it atomically to <paramref name="filePath"/>.
        /// </summary>
        public static bool SaveJson<T>(string filePath, T data, bool prettyPrint = true)
        {
            if (string.IsNullOrEmpty(filePath) || data == null) return false;

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonUtility.ToJson(data, prettyPrint);
                var tempPath = filePath + ".tmp";

                File.WriteAllText(tempPath, json);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempPath, filePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Failed to save JSON to '{filePath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads and deserializes JSON from <paramref name="filePath"/> into <paramref name="data"/>.
        /// </summary>
        public static bool TryLoadJson<T>(string filePath, out T data)
        {
            data = default;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;

            try
            {
                var json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json)) return false;

                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Failed to load JSON from '{filePath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>Deletes the file at <paramref name="filePath"/> if it exists.</summary>
        public static bool Delete(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveSystem] Failed to delete '{filePath}': {ex.Message}");
                return false;
            }
        }
    }
}
