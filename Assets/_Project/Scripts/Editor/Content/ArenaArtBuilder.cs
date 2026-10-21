using System.Collections.Generic;
using GraceRun.Core.Environment;
using GraceRun.Core.Framing;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Arena Art: dresses the four finished arenas (hub, Gatekeeper, Brood Mother, Bell Twins)
    /// with tiled art and parallax backdrops. Gameplay objects are never touched: the old flat-colour
    /// renderers are only disabled, and the art lives on a separate "EnvArt" child sized from the colliders.
    /// Re-run it after Lay Out Wide Arenas.
    /// </summary>
    public static class ArenaArtBuilder
    {
        private const string ArenaFolder = "Assets/_Project/Prefabs/Arenas/";
        private const string ArtFolder = "Assets/_Project/Art/Environment/";
        private const string DataFolder = "Assets/_Project/Data/Environment/";
        private const float ViewWidth = 40f;
        private const float BackdropUnits = 80f;
        private const float ViewPixels = 480f;

        private const int OrderFill = -3;
        private const int OrderSolid = -2;
        private const int OrderPlatform = -11;
        private const int OrderDecor = -20;

        // name, folder, whether the gate frame (posts and lintel) is drawn
        private static readonly (string Prefab, string Folder, bool Gate)[] Arenas =
        {
            ("HubArena", "hub", true),
            ("GatekeeperArena", "gatekeeper", false),
            ("BroodMotherArena", "broodmother", false),
            ("BellTwinsArena", "belltwins", false),
        };

        private static readonly (string Name, float Follow, int Order)[] Layers =
        {
            ("far", 1f, -100), ("near", 0.5f, -80),
        };

        [MenuItem("Grace Run/Build Arena Art")]
        public static void Run()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var a in Arenas) ImportSprites(a.Folder);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Environment"))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Environment");
            foreach (var a in Arenas) Build(a.Prefab, a.Folder, a.Gate);
            AssetDatabase.SaveAssets();
            Debug.Log("Arena art built.");
        }

        private static void ImportSprites(string folder)
        {
            Import(folder, "floor", 24f, Vector4.zero);
            Import(folder, "fill", 24f, Vector4.zero);
            Import(folder, "wall", 24f, Vector4.zero);
            Import(folder, "platform", 24f, new Vector4(12, 0, 12, 0));
            Import(folder, "pillar", 24f, new Vector4(0, 24, 0, 24));
            Import(folder, "beam", 24f, new Vector4(24, 0, 24, 0));
            foreach (var l in Layers) Import(folder, "bg_" + l.Name, 12f, Vector4.zero);
        }

        private static void Import(string folder, string file, float ppu, Vector4 border)
        {
            var path = ArtFolder + folder + "/" + file + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spriteBorder = border;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Sprite Load(string folder, string file) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(ArtFolder + folder + "/" + file + ".png");

        private static void Build(string prefabName, string folder, bool gate)
        {
            var path = ArenaFolder + prefabName + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var old = root.transform.Find("EnvArt");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                var oldBackdrop = root.transform.Find("Backdrop");
                if (oldBackdrop != null) Object.DestroyImmediate(oldBackdrop.gameObject);

                var art = new GameObject("EnvArt").transform;
                art.SetParent(root.transform, false);
                BuildSolids(root.transform, art, folder);
                BuildPillars(root.transform, art, folder, gate);
                BuildBackdrop(root, folder);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BuildSolids(Transform arena, Transform art, string folder)
        {
            var floor = arena.Find("Floor");
            if (floor != null)
            {
                var r = Rect(floor);
                Tile(art, "FloorArt", Load(folder, "floor"), new Rect(r.xMin, r.yMax - 2f, r.width, 2f), OrderSolid);
                Tile(art, "FillArt", Load(folder, "fill"), new Rect(r.xMin, r.yMin - 6f, r.width, 6f), OrderFill);
                Disable(floor);
            }
            DisableNamed(arena, "FloorTrim");

            foreach (var side in new[] { "WallLeft", "WallRight" })
            {
                var wall = arena.Find(side);
                if (wall == null) continue;
                var r = Rect(wall);
                var go = Tile(art, side + "Art", Load(folder, "wall"), r, OrderSolid);
                go.GetComponent<SpriteRenderer>().flipX = side == "WallRight";
                Disable(wall);
            }

            foreach (Transform child in arena)
            {
                if (!child.name.StartsWith("Platform")) continue;
                var r = Rect(child);
                Tile(art, child.name + "Art", Load(folder, "platform"), new Rect(r.xMin, r.yMax - 0.5f, r.width, 0.5f), OrderPlatform);
                Disable(child);
            }
        }

        private static void BuildPillars(Transform arena, Transform art, string folder, bool gate)
        {
            var pillar = Load(folder, "pillar");
            var posts = new List<Transform>();
            Collect(arena, posts);
            foreach (var t in posts)
            {
                var n = t.name;
                var isPillar = n.StartsWith("Pillar");
                var isPost = n.StartsWith("GatePost");
                var isLintel = n == "GateLintel";
                if (!isPillar && !isPost && !isLintel) continue;
                Disable(t);
                var s = t.lossyScale;
                var p = t.position;
                if (isLintel)
                {
                    if (gate) Tile(art, n + "Art", Load(folder, "beam"), new Rect(p.x - s.x * 0.5f, p.y - 0.5f, s.x, 1f), OrderDecor + 1);
                }
                else if (isPillar || gate)
                {
                    Tile(art, n + "Art", pillar, new Rect(p.x - 0.5f, p.y - s.y * 0.5f, 1f, s.y), OrderDecor);
                }
            }
        }

        private static void Collect(Transform root, List<Transform> into)
        {
            foreach (Transform child in root)
            {
                if (child.name == "EnvArt" || child.name == "Backdrop") continue;
                into.Add(child);
                if (child.name == "Background") Collect(child, into);
            }
        }

        private static void BuildBackdrop(GameObject root, string folder)
        {
            var bounds = root.GetComponent<ArenaBounds>();
            var camWidth = bounds != null ? bounds.CameraArea.width : ViewWidth;
            var travelHalf = Mathf.Max(0f, (camWidth - ViewWidth) * 0.5f);

            var holder = new GameObject("Backdrop");
            holder.transform.SetParent(root.transform, false);
            var layers = new ParallaxLayer[Layers.Length];
            var transforms = new Transform[Layers.Length];
            for (var i = 0; i < Layers.Length; i++)
            {
                var l = Layers[i];
                var sprite = Load(folder, "bg_" + l.Name);
                var go = new GameObject("Layer_" + l.Name);
                go.transform.SetParent(holder.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = l.Order;
                if (sprite.rect.width <= ViewPixels)
                {
                    // One painted scene exactly the size of the view, pinned to the screen (a hair larger to hide pixel snapping).
                    go.transform.localScale = new Vector3(1.01f, 1.01f, 1f);
                }
                else
                {
                    sr.drawMode = SpriteDrawMode.Tiled;
                    sr.tileMode = SpriteTileMode.Continuous;
                    var half = ViewWidth * 0.5f + 2f + (1f - l.Follow) * travelHalf;
                    var width = Mathf.Ceil(half * 2f / BackdropUnits) * BackdropUnits;
                    sr.size = new Vector2(width, 22.5f);
                }
                transforms[i] = go.transform;
                layers[i] = new ParallaxLayer { Sprite = sprite, Follow = l.Follow, YOffset = 0f, SortingOrder = l.Order, Tint = Color.white };
            }

            var assetPath = DataFolder + folder + "Parallax.asset";
            var data = AssetDatabase.LoadAssetAtPath<ParallaxSetData>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ParallaxSetData>();
                AssetDatabase.CreateAsset(data, assetPath);
            }
            data.SetLayers(layers);
            EditorUtility.SetDirty(data);
            holder.AddComponent<ParallaxBackdrop>().Configure(data, transforms);
        }

        private static GameObject Tile(Transform parent, string name, Sprite sprite, Rect rect, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(rect.center.x, rect.center.y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = rect.size;
            sr.sortingOrder = order;
            return go;
        }

        /// <summary>World rectangle of a placeholder box (position and scale are the size; arenas sit at the origin).</summary>
        private static Rect Rect(Transform t)
        {
            var s = t.lossyScale;
            var p = t.position;
            return new Rect(p.x - s.x * 0.5f, p.y - s.y * 0.5f, s.x, s.y);
        }

        private static void Disable(Transform t)
        {
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        private static void DisableNamed(Transform arena, string name)
        {
            var t = arena.Find(name);
            if (t != null) Disable(t);
        }
    }
}
