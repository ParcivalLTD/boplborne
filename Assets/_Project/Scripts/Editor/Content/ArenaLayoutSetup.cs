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

        /// <summary>One ledge, mirrored to the other side unless X is 0: centre x, top above the floor, width.</summary>
        private readonly struct Ledge
        {
            public readonly string Id;
            public readonly float X, Rise, Width;
            public Ledge(string id, float x, float rise, float width) { Id = id; X = x; Rise = rise; Width = width; }
        }

        // Rises stay under the jump + air jump reach (checked in Validate); gaps between ledges are crossed by running.

        // The Gatekeeper teaches dodging: a clear duel floor in the middle (his slam and sweep reach 4.5),
        // low guard ledges to hop over the sweep, balconies for aerial pokes at his head, one risky perch above the gate.
        private static readonly Ledge[] GatekeeperLedges =
        {
            new Ledge("Perch", 0f, 6f, 4f),
            new Ledge("Guard", 11f, 2.5f, 6f),
            new Ledge("Step", 16.5f, 4f, 3.5f),
            new Ledge("Balcony", 22f, 5.5f, 7f),
        };

        // The Brood Mother punishes bunching and clutter: stepping stones above her webs and crawlers (a crawler hops
        // about 2 units), staggered heights so the group spreads out, and a clear middle where her ceiling drop lands.
        private static readonly Ledge[] BroodMotherLedges =
        {
            new Ledge("Stone", 7f, 2.5f, 5f),
            new Ledge("Branch", 14f, 4.5f, 6f),
            new Ledge("Root", 20.5f, 2.5f, 5f),
            new Ledge("Rafter", 25.5f, 5f, 5f),
            new Ledge("Corner", 30f, 3f, 3f),
        };

        // The Bell Twins split the team: two mirrored belfry stacks (one per twin) with a low bridge between them,
        // plenty of airtime over the floor shockwaves, and high perches for the pair that must both hit a toll.
        private static readonly Ledge[] BellTwinsLedges =
        {
            new Ledge("Bridge", 0f, 5.5f, 6f),
            new Ledge("Landing", 9f, 3f, 6f),
            new Ledge("Gallery", 18f, 5.5f, 7f),
            new Ledge("Stage", 27f, 3f, 7f),
            new Ledge("Loft", 35f, 5.5f, 6f),
        };

        // The Storm Wyrm's Storm Slam removes about half of all ledges for 6 s, so there are many ledges at varied heights
        // (some always remain), a few high perches for air control, and bare floor between stacks to run on
        // (Lightning Columns land under every player, the Tail Sweep is a low 10-wide hit).
        private static readonly Ledge[] StormWyrmLedges =
        {
            new Ledge("Drift", 0f, 3.5f, 5f),
            new Ledge("Cloudstep", 8f, 2.5f, 5f),
            new Ledge("Spire", 14f, 5.5f, 4f),
            new Ledge("Terrace", 20f, 3.5f, 6f),
            new Ledge("Arch", 26f, 5f, 4f),
            new Ledge("Rubble", 31f, 2.5f, 3f),
        };

        // The Mirror Duelist is a duel stage: a clear middle for the reflections to cross, and mirrored low and mid ledges
        // on both flanks (its reflections stand across the target and across the centre), none above a single jump and a half.
        private static readonly Ledge[] MirrorDuelistLedges =
        {
            new Ledge("Dais", 10f, 2.5f, 5f),
            new Ledge("Gallery", 17.5f, 4.5f, 5f),
            new Ledge("Balcony", 25f, 3f, 5f),
        };

        /// <summary>Inner half width, extra height above the standard 22.5-unit view and ledges (null = the generic grid), per arena.</summary>
        private static readonly (string Name, float HalfWidth, float ExtraHeight, Ledge[] Ledges)[] Layouts =
        {
            ("HubArena", 16f, 0f, null),
            ("GatekeeperArena", 28f, 0f, GatekeeperLedges),
            ("TestBossArena", 28f, 0f, null),
            ("BroodMotherArena", 32f, 0f, BroodMotherLedges),
            ("BellTwinsArena", 40f, 0f, BellTwinsLedges),
            ("FinalTestBossArena", 36f, 10f, null),
            ("StormWyrmArena", 34f, 0f, StormWyrmLedges),
            ("MirrorDuelistArena", 30f, 0f, MirrorDuelistLedges),
        };

        /// <summary>Lays out just the arena called <paramref name="name"/> from its row in the table.</summary>
        public static void LayOut(string name)
        {
            foreach (var layout in Layouts)
                if (layout.Name == name) Apply(layout.Name, layout.HalfWidth, layout.ExtraHeight, layout.Ledges);
        }

        [MenuItem("Grace Run/Lay Out Wide Arenas")]
        public static void Run()
        {
            foreach (var layout in Layouts) Apply(layout.Name, layout.HalfWidth, layout.ExtraHeight, layout.Ledges);
            AssetDatabase.SaveAssets();
            Debug.Log("Arena prefabs laid out.");
        }

        private static void AddLedges(Transform root, float half, Ledge[] ledges)
        {
            const float maxRise = 6.5f; // 4.5 jump + 3 air jump, less a margin
            foreach (var l in ledges)
            {
                if (l.Rise > maxRise) Debug.LogError($"{root.name}: ledge {l.Id} is {l.Rise} high, out of jump reach.");
                if (Mathf.Abs(l.X) + l.Width * 0.5f > half - 0.5f) Debug.LogError($"{root.name}: ledge {l.Id} pokes into the wall.");
                var y = FloorTop + l.Rise - PlatformThickness * 0.5f;
                if (l.X == 0f) Add(root, "Platform" + l.Id, 0f, y, l.Width);
                else
                {
                    Add(root, "Platform" + l.Id + "Left", -l.X, y, l.Width);
                    Add(root, "Platform" + l.Id + "Right", l.X, y, l.Width);
                }
            }
        }

        private static void Apply(string name, float half, float extra, Ledge[] ledges)
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
                if (ledges != null) AddLedges(root.transform, half, ledges);
                else AddPlatforms(root.transform, half, extra);

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
