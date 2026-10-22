using System.Collections.Generic;
using System.IO;
using GraceRun.Abilities.Effects;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Ability FX: cuts the PixelLab effect sheets in Art/FX/Abilities/Sheets into
    /// frames (imported 1 unit wide, so a visual's scale is its world width), imports the prop sprites
    /// in Art/FX/Abilities/Props, and gives every ability prefab its art. Safe to run again.
    /// </summary>
    public static class AbilityFxBuilder
    {
        private const string Root = "Assets/_Project/Art/FX/Abilities";
        private const string PrefabDir = "Assets/_Project/Prefabs/Abilities";

        // sheet, canvas size, frame count, origin pixel (from the top-left), where the sprite's pivot sits.
        private static readonly (string name, int w, int h, int count, int ox, int oy)[] Sheets =
        {
            ("fx_explosion", 96, 96, 12, 48, 48), ("fx_heal_pillar", 80, 112, 12, 40, 92),
            ("fx_portal", 96, 112, 12, 48, 56), ("fx_lightning", 96, 128, 10, 48, 110),
            ("fx_shockwave", 112, 64, 8, 56, 40), ("fx_fire_pool", 71, 40, 12, 34, 24),
            ("fx_spike_trap", 56, 65, 14, 31, 43), ("fx_tornado", 72, 112, 8, 36, 104),
            ("fx_gust", 108, 57, 12, 56, 26),
            ("fx_orb_frost", 48, 48, 8, 24, 24), ("fx_orb_arcane", 48, 48, 8, 24, 24),
            ("fx_orb_holy", 48, 48, 8, 24, 24), ("fx_orb_fire", 48, 48, 8, 24, 24),
            ("fx_orb_electric", 48, 48, 8, 24, 24), ("fx_orb_nature", 48, 48, 8, 24, 24),
        };

        private static readonly string[] Props =
        {
            "arrow", "spear", "boomerang", "bomb", "flask", "grenade", "meteor", "sentry", "wall", "mine", "bouncepad", "blade", "totem",
        };

        /// <summary>One prefab's art. Frames from a sheet (animated) or one prop; width is the world width, or a factor on the ability diameter.</summary>
        private readonly struct Wire
        {
            public readonly string Prefab, Sheet, Prop;
            public readonly float Width, RadiusFactor, FadeTail, Spin, Fps;
            public readonly bool OverLifetime;
            public readonly int First, Last;

            public Wire(string prefab, string sheet = null, string prop = null, float width = 1f, float radiusFactor = 0f,
                float fadeTail = 0f, float spin = 0f, float fps = 12f, bool overLifetime = false, int first = 0, int last = -1)
            {
                Prefab = prefab; Sheet = sheet; Prop = prop; Width = width; RadiusFactor = radiusFactor;
                FadeTail = fadeTail; Spin = spin; Fps = fps; OverLifetime = overLifetime; First = first; Last = last;
            }
        }

        private static readonly Wire[] Wires =
        {
            new("Arrow", prop: "arrow", width: 1.9f), new("SpearThrow", prop: "spear", width: 3f),
            new("Boomerang", prop: "boomerang", width: 1.5f, spin: 900f),
            new("Bomb", prop: "bomb", width: 1.1f), new("Bomblet", prop: "bomb", width: 0.7f),
            new("ClusterBomb", prop: "bomb", width: 1.2f), new("Firebomb", prop: "flask", width: 1f),
            new("GravityGrenade", prop: "grenade", width: 1f),
            new("Sentry", prop: "sentry", width: 2.6f), new("ProximityMine", prop: "mine", width: 1.3f),
            new("BouncePad", prop: "bouncepad", width: 2.4f), new("StoneWall", prop: "wall", width: 1f),
            new("Meteor", sheet: "fx_orb_fire", width: 3f), new("FrostBolt", sheet: "fx_orb_frost", width: 1.5f),
            new("ShrinkRay", sheet: "fx_orb_arcane", width: 1.5f), new("WispSwarm", sheet: "fx_orb_nature", width: 1.1f),
            new("SentryBolt", sheet: "fx_orb_holy", width: 1.2f),
            new("BlackHole", sheet: "fx_portal", radiusFactor: 0.6f, fadeTail: 0.2f),
            new("MiniHole", sheet: "fx_portal", radiusFactor: 0.6f, fadeTail: 0.2f),
            new("Tornado", sheet: "fx_tornado", radiusFactor: 0.5f, fadeTail: 0.15f, fps: 10f),
            new("GraceTotem", sheet: "fx_heal_pillar", radiusFactor: 0.8f, fadeTail: 0.15f),
            new("FirePool", sheet: "fx_fire_pool", radiusFactor: 1.3f, fadeTail: 0.15f, fps: 10f),
            new("SpikeTrap", sheet: "fx_spike_trap", radiusFactor: 1.15f, fps: 8f, first: 3, last: 9),
            new("Gust", sheet: "fx_gust", radiusFactor: 1.6f, fadeTail: 0.5f),
        };

        [MenuItem("Grace Run/Build Ability FX")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var s in Sheets) CutSheet(s.name, s.w, s.h, s.count, s.ox, s.oy);
            foreach (var p in Props) ImportProp(p);
            AssetDatabase.SaveAssets();
            foreach (var wire in Wires) WirePrefab(wire);
            WireBursts();
            AssetDatabase.SaveAssets();
            Debug.Log("Ability FX built.");
        }

        private static string FramePath(string sheet, int i) => $"{Root}/Frames/{sheet}/{sheet}_{i:00}.png";

        private static void CutSheet(string name, int w, int h, int count, int ox, int oy)
        {
            var sheetPath = $"{Root}/Sheets/{name}.png";
            if (!File.Exists(sheetPath)) { Debug.LogWarning($"Ability FX: missing {sheetPath}"); return; }
            var sheet = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            sheet.LoadImage(File.ReadAllBytes(sheetPath));
            var cols = sheet.width / w;
            Directory.CreateDirectory($"{Root}/Frames/{name}");
            for (var i = 0; i < count; i++)
            {
                var x = (i % cols) * w;
                var y = sheet.height - (i / cols + 1) * h;
                var frame = new Texture2D(w, h, TextureFormat.RGBA32, false);
                frame.SetPixels(sheet.GetPixels(x, y, w, h));
                File.WriteAllBytes(FramePath(name, i), frame.EncodeToPNG());
                Object.DestroyImmediate(frame);
            }
            Object.DestroyImmediate(sheet);
            AssetDatabase.Refresh();
            var pivot = new Vector2((float)ox / w, 1f - (float)oy / h);
            for (var i = 0; i < count; i++) ImportSprite(FramePath(name, i), w, pivot);
        }

        private static void ImportProp(string name)
        {
            var path = $"{Root}/Props/{name}.png";
            if (!File.Exists(path)) { Debug.LogWarning($"Ability FX: missing {path}"); return; }
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(path));
            var width = tex.width;
            Object.DestroyImmediate(tex);
            var grounded = name == "sentry" || name == "mine" || name == "bouncepad" || name == "totem";
            ImportSprite(path, width, new Vector2(0.5f, grounded ? 0f : 0.5f));
        }

        private static void ImportSprite(string path, int pixelsPerUnit, Vector2 pivot)
        {
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static void WirePrefab(Wire wire)
        {
            var path = $"{PrefabDir}/{wire.Prefab}.prefab";
            if (!File.Exists(path)) { Debug.LogWarning($"Ability FX: no prefab {path}"); return; }

            var frames = new List<Sprite>();
            if (wire.Sheet != null)
            {
                var count = 0;
                foreach (var s in Sheets) if (s.name == wire.Sheet) count = s.count;
                var last = wire.Last >= 0 ? wire.Last : count - 1;
                for (var i = wire.First; i <= last; i++) frames.Add(Load(FramePath(wire.Sheet, i)));
            }
            else frames.Add(Load($"{Root}/Props/{wire.Prop}.png"));
            if (frames.Count == 0 || frames[0] == null) { Debug.LogWarning($"Ability FX: no art for {wire.Prefab}"); return; }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visual = root.transform.Find("Visual");
                if (visual == null) { Debug.LogWarning($"Ability FX: {wire.Prefab} has no Visual"); return; }
                var core = root.transform.Find("Core");
                if (core != null) core.gameObject.SetActive(false); // the portal art has its own dark heart
                foreach (var old in root.GetComponents<FadeView>()) Object.DestroyImmediate(old);
                foreach (var old in root.GetComponents<PulseView>()) Object.DestroyImmediate(old);

                var renderer = visual.GetComponent<SpriteRenderer>();
                renderer.sprite = frames[0];
                renderer.color = new Color(1f, 1f, 1f, 1f);
                if (wire.Prop == "bomb" && wire.Prefab == "ClusterBomb") renderer.color = new Color(1f, 0.92f, 0.55f, 1f);

                var animated = frames.Count > 1 || wire.Spin != 0f || wire.RadiusFactor > 0f;
                if (!animated)
                {
                    visual.localScale = new Vector3(wire.Width, wire.Width, 1f);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    return;
                }

                var flip = visual.GetComponent<SpriteFlipbook>();
                if (flip == null) flip = visual.gameObject.AddComponent<SpriteFlipbook>();
                var so = new SerializedObject(flip);
                var arr = so.FindProperty("_frames");
                arr.arraySize = frames.Count;
                for (var i = 0; i < frames.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                so.FindProperty("_fps").floatValue = wire.Fps;
                so.FindProperty("_playback").enumValueIndex = wire.OverLifetime ? 1 : 0;
                so.FindProperty("_fixedWidth").floatValue = wire.Width;
                so.FindProperty("_radiusFactor").floatValue = wire.RadiusFactor;
                so.FindProperty("_fadeTail").floatValue = wire.FadeTail;
                so.FindProperty("_spin").floatValue = wire.Spin;
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        /// <summary>The shared blast prefab plays the explosion; Thunderclap gets its own lightning blast; blast zones hide their old disc.</summary>
        private static void WireBursts()
        {
            const string bombBurst = "Assets/_Project/Prefabs/FX/BombBurst.prefab";
            const string thunderBurst = "Assets/_Project/Prefabs/FX/ThunderBurst.prefab";
            SetBurstFrames(bombBurst, "fx_explosion", 12, 1.5f);

            if (!File.Exists(thunderBurst)) AssetDatabase.CopyAsset(bombBurst, thunderBurst);
            SetBurstFrames(thunderBurst, "fx_lightning", 10, 1.2f);

            var thunderPrefab = $"{PrefabDir}/Thunderclap.prefab";
            var burst = AssetDatabase.LoadAssetAtPath<GameObject>(thunderBurst).GetComponent<BurstFx>();
            var root = PrefabUtility.LoadPrefabContents(thunderPrefab);
            try
            {
                foreach (var comp in root.GetComponents<MonoBehaviour>())
                {
                    var so = new SerializedObject(comp);
                    var prop = so.FindProperty("_fxPrefab");
                    if (prop == null) continue;
                    prop.objectReferenceValue = burst;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, thunderPrefab);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }

            foreach (var blast in new[] { "CollapseBlast", "GrenadeBlast", "TornadoBlast" })
            {
                var path = $"{PrefabDir}/{blast}.prefab";
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var visual = contents.transform.Find("Visual");
                    if (visual != null) visual.gameObject.SetActive(false);
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
        }

        private static void SetBurstFrames(string path, string sheet, int count, float radiusFactor)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var burst = root.GetComponent<BurstFx>();
                var so = new SerializedObject(burst);
                var arr = so.FindProperty("_frames");
                arr.arraySize = count;
                for (var i = 0; i < count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = Load(FramePath(sheet, i));
                so.FindProperty("_radiusFactor").floatValue = radiusFactor;
                so.ApplyModifiedPropertiesWithoutUndo();
                var sr = root.GetComponentInChildren<SpriteRenderer>();
                sr.color = Color.white;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
