using System.Collections.Generic;
using GraceRun.Core.Framing;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Lays out the arena prefabs at their wide sizes and gives each an <see cref="ArenaBounds"/>.
    /// Re-running it re-lays every arena from <see cref="Layouts"/> (floor, walls, extra pillars,
    /// one-way platforms); the numbers to tune are the rows below and the ArenaBounds on the prefab.
    /// </summary>
    public static class ArenaLayoutSetup
    {
        private const string Folder = "Assets/_Project/Prefabs/Arenas/";
        private const string PlatformPrefab = Folder + "OneWayPlatform.prefab";

        private const float FloorTop = -5f;
        private const float FloorThickness = 2f;
        private const float WallThickness = 2f;
        private const float ViewBottom = -9.25f;
        private const float ViewHeight = 22.5f;
        private const float LowTopAboveFloor = 2.5f;
        private const float HighTopAboveFloor = 6f;
        private const float PlatformThickness = 0.5f;
        private const float LowWidth = 6f;
        private const float HighWidth = 7f;
        private const float PillarSpacing = 16f;

        /// <summary>Inner half width and extra height above the standard 22.5-unit view, per arena.</summary>
        private static readonly (string Name, float HalfWidth, float ExtraHeight)[] Layouts =
        {
            ("HubArena", 16f, 0f),
            ("GatekeeperArena", 28f, 0f),
            ("TestBossArena", 28f, 0f),
            ("BroodMotherArena", 32f, 0f),
            ("BellTwinsArena", 40f, 0f),
            ("FinalTestBossArena", 36f, 10f),
        };

        [MenuItem("Grace Run/Lay Out Wide Arenas")]
        public static void Run()
        {
            foreach (var layout in Layouts) Apply(layout.Name, layout.HalfWidth, layout.ExtraHeight);
            AssetDatabase.SaveAssets();
            Debug.Log("Arena prefabs laid out.");
        }

        private static void Apply(string name, float half, float extra)
        {
            var path = Folder + name + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RemoveGenerated(root.transform);
                var viewTop = ViewBottom + ViewHeight + extra;
                SetBox(root, "Floor", 0f, FloorTop - FloorThickness * 0.5f, 2f * (half + WallThickness), FloorThickness);
                SetBox(root, "FloorTrim", 0f, FloorTop - 0.1f, 2f * half, 0.2f);
                var wallHeight = viewTop - (FloorTop - FloorThickness);
                var wallY = (viewTop + FloorTop - FloorThickness) * 0.5f;
                SetBox(root, "WallLeft", -(half + WallThickness * 0.5f), wallY, WallThickness, wallHeight);
                SetBox(root, "WallRight", half + WallThickness * 0.5f, wallY, WallThickness, wallHeight);
                AddPillars(root.transform, half);
                AddPlatforms(root.transform, half, extra);

                var bounds = root.GetComponent<ArenaBounds>();
                if (bounds == null) bounds = root.AddComponent<ArenaBounds>();
                bounds.Configure(
                    new Rect(-half, FloorTop, 2f * half, viewTop - FloorTop),
                    new Rect(-(half + WallThickness), ViewBottom, 2f * (half + WallThickness), viewTop - ViewBottom));
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RemoveGenerated(Transform root)
        {
            var doomed = new List<GameObject>();
            foreach (Transform child in root)
            {
                if (child.name.StartsWith("Platform") || child.name.StartsWith("PillarExtra")) doomed.Add(child.gameObject);
            }
            foreach (var go in doomed) Object.DestroyImmediate(go);
        }

        private static void SetBox(GameObject root, string childName, float x, float y, float width, float height)
        {
            var child = root.transform.Find(childName);
            if (child == null) return;
            child.localPosition = new Vector3(x, y, child.localPosition.z);
            child.localScale = new Vector3(width, height, 1f);
        }

        private static void AddPillars(Transform root, float half)
        {
            foreach (var side in new[] { "PillarLeft", "PillarRight" })
            {
                var source = root.Find(side);
                if (source == null) continue;
                var sign = Mathf.Sign(source.localPosition.x);
                for (var x = Mathf.Abs(source.localPosition.x) + PillarSpacing; x < half - 3f; x += PillarSpacing)
                {
                    var copy = Object.Instantiate(source.gameObject, root);
                    copy.name = "PillarExtra" + (sign < 0 ? "Left" : "Right") + Mathf.RoundToInt(x);
                    copy.transform.localPosition = new Vector3(sign * x, source.localPosition.y, source.localPosition.z);
                }
            }
        }

        private static void AddPlatforms(Transform root, float half, float extra)
        {
            var lowY = FloorTop + LowTopAboveFloor - PlatformThickness * 0.5f;
            var highY = FloorTop + HighTopAboveFloor - PlatformThickness * 0.5f;
            var spacing = 18f;

            Add(root, "PlatformHigh", 0f, highY, HighWidth);
            for (var x = 10f; x < half - 3f; x += spacing)
            {
                Add(root, "PlatformLowLeft" + Mathf.RoundToInt(x), -x, lowY, LowWidth);
                Add(root, "PlatformLowRight" + Mathf.RoundToInt(x), x, lowY, LowWidth);
            }
            for (var x = spacing; x < half - 3f; x += spacing * 2f)
            {
                Add(root, "PlatformHighLeft" + Mathf.RoundToInt(x), -x, highY, HighWidth);
                Add(root, "PlatformHighRight" + Mathf.RoundToInt(x), x, highY, HighWidth);
            }

            if (extra <= 0f) return;
            // Taller arenas: a ladder of ledges the air jump can chain, 6 units apart.
            for (var tier = 1; FloorTop + HighTopAboveFloor * (tier + 1) < FloorTop + ViewHeight + extra - 4f; tier++)
            {
                var y = FloorTop + HighTopAboveFloor * (tier + 1) - PlatformThickness * 0.5f;
                var offset = tier % 2 == 1 ? 14f : 0f;
                if (offset == 0f) Add(root, "PlatformTier" + tier, 0f, y, HighWidth);
                else
                {
                    Add(root, "PlatformTier" + tier + "Left", -offset, y, HighWidth);
                    Add(root, "PlatformTier" + tier + "Right", offset, y, HighWidth);
                }
            }
        }

        private static void Add(Transform root, string name, float x, float y, float width)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlatformPrefab);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            instance.name = name;
            instance.transform.localPosition = new Vector3(x, y, 0f);
            instance.transform.localScale = new Vector3(width, instance.transform.localScale.y, 1f);
        }
    }
}
