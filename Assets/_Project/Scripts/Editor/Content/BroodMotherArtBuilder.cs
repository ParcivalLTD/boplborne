using System.IO;
using GraceRun.Bosses;
using GraceRun.Bosses.Behaviours;
using GraceRun.Combat;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Build Brood Mother Art (docs/ART_PIPELINE.md step 4): imports the cleaned PNGs
    /// in Art/Bosses/BroodMother, fills <c>BroodMotherPoses.asset</c> and <c>CrawlerArt.asset</c> and
    /// wires <see cref="BossPoseView"/> / <see cref="BroodCrawlerView"/> into the two prefabs. Safe to run again.
    /// </summary>
    public static class BroodMotherArtBuilder
    {
        private const string ArtDir = "Assets/_Project/Art/Bosses/BroodMother";
        private const string DataDir = "Assets/_Project/Data/Bosses/BroodMother";
        private const string MotherPrefab = "Assets/_Project/Prefabs/Bosses/BroodMother.prefab";
        private const string CrawlerPrefab = "Assets/_Project/Prefabs/Bosses/BroodCrawler.prefab";
        // The boss root is 3 units above the floor. Only her longest back leg reaches the bottom row, the other
        // legs end 0.5-0.7 units higher, so the art sinks 0.4 units (the floor draws over the extra length).
        private const float MotherFeetOffset = -3.4f;
        private const float CrawlerFeetOffset = -0.45f; // half the 0.9 collider height
        private const int CrawlerWalkFrames = 8;

        [MenuItem("Grace Run/Build Brood Mother Art")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            foreach (var path in Directory.GetFiles(ArtDir, "*.png")) Import(path.Replace('\\', '/'));
            WireMother(BuildPoseSet());
            WireCrawler(BuildCrawlerSet());
            AssetDatabase.SaveAssets();
            Debug.Log("Brood Mother art built.");
        }

        private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.png");

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static BossPoseSet BuildPoseSet()
        {
            var set = LoadOrCreate<BossPoseSet>($"{DataDir}/BroodMotherPoses.asset");
            var so = new SerializedObject(set);
            SetSprites(so.FindProperty("_idle"), S("broodmother_idle_0"), S("broodmother_idle_1"), S("broodmother_idle_2"), S("broodmother_idle_3"));
            so.FindProperty("_stagger").objectReferenceValue = S("broodmother_stagger");
            SetSprites(so.FindProperty("_death"), S("broodmother_death_0"), S("broodmother_death_1"), S("broodmother_death_2"), S("broodmother_death_3"));

            var list = so.FindProperty("_attacks");
            list.arraySize = 4;
            SetAttack(list.GetArrayElementAtIndex(0), "bite", "fang_bite", "fang_bite_double", "fang_bite_second");
            SetAttack(list.GetArrayElementAtIndex(1), "web", "web_spit");
            SetAttack(list.GetArrayElementAtIndex(2), "spawn", "spawn_crawlers");
            SetAttack(list.GetArrayElementAtIndex(3), "drop", "ceiling_drop");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static CrawlerArtSet BuildCrawlerSet()
        {
            var set = LoadOrCreate<CrawlerArtSet>($"{DataDir}/CrawlerArt.asset");
            var so = new SerializedObject(set);
            var walk = new Sprite[CrawlerWalkFrames];
            for (var i = 0; i < walk.Length; i++) walk[i] = S($"crawler_walk_{i}");
            SetSprites(so.FindProperty("_walk"), walk);
            so.FindProperty("_idle").objectReferenceValue = S("crawler_idle");
            so.FindProperty("_hit").objectReferenceValue = S("crawler_hit");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void SetAttack(SerializedProperty e, string pose, params string[] ids)
        {
            var list = e.FindPropertyRelative("AttackIds");
            list.arraySize = ids.Length;
            for (var i = 0; i < ids.Length; i++) list.GetArrayElementAtIndex(i).stringValue = ids[i];
            e.FindPropertyRelative("Windup").objectReferenceValue = S($"broodmother_{pose}_windup");
            e.FindPropertyRelative("Strike").objectReferenceValue = S($"broodmother_{pose}_strike");
            e.FindPropertyRelative("Recovery").objectReferenceValue = S($"broodmother_{pose}_recovery");
        }

        private static void SetSprites(SerializedProperty list, params Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void WireMother(BossPoseSet poses)
        {
            var root = PrefabUtility.LoadPrefabContents(MotherPrefab);
            try
            {
                var visuals = root.transform.Find("Visuals");
                foreach (var placeholder in new[] { "Fangs", "Abdomen", "Eye" }) visuals.Find(placeholder).gameObject.SetActive(false);
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                Place(body, poses.Idle[0], MotherFeetOffset, true);

                var view = root.GetComponent<BossPoseView>() ?? root.AddComponent<BossPoseView>();
                var so = new SerializedObject(view);
                so.FindProperty("_poses").objectReferenceValue = poses;
                so.FindProperty("_runner").objectReferenceValue = root.GetComponent<BossAttackRunner>();
                so.FindProperty("_brain").objectReferenceValue = root.GetComponent<BossBrain>();
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = body;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, MotherPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireCrawler(CrawlerArtSet art)
        {
            var root = PrefabUtility.LoadPrefabContents(CrawlerPrefab);
            try
            {
                var visuals = root.transform.Find("Visuals");
                foreach (var placeholder in new[] { "Eye", "Legs" }) visuals.Find(placeholder).gameObject.SetActive(false);
                var body = visuals.Find("Body").GetComponent<SpriteRenderer>();
                Place(body, art.Idle, CrawlerFeetOffset, false); // crawler art faces right, the prefab mirrors Visuals with the facing

                var view = root.GetComponent<BroodCrawlerView>() ?? root.AddComponent<BroodCrawlerView>();
                var so = new SerializedObject(view);
                so.FindProperty("_art").objectReferenceValue = art;
                so.FindProperty("_health").objectReferenceValue = root.GetComponent<Health>();
                so.FindProperty("_body").objectReferenceValue = body;
                so.ApplyModifiedProperties();
                PrefabUtility.SaveAsPrefabAsset(root, CrawlerPrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>Boss art faces left and the Visuals root is written for a boss facing right, so boss sprites are flipped.</summary>
        private static void Place(SpriteRenderer renderer, Sprite sprite, float feetOffset, bool flip)
        {
            renderer.sprite = sprite;
            renderer.flipX = flip;
            renderer.color = Color.white; // the placeholder accent tint would multiply the art's colours down
            renderer.transform.localPosition = new Vector3(0f, feetOffset, 0f);
            renderer.transform.localScale = Vector3.one;
        }

        private static void Import(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 24;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
