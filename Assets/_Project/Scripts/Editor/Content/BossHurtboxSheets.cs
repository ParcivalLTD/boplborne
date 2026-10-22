using System.Collections.Generic;
using System.IO;
using System.Text;
using GraceRun.Bosses;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Bosses → Export Hurtbox Sheets: draws, per boss, the idle pose and the strike pose
    /// of every attack as the boss looks facing right, with its fitted hurtbox (blue) and the attack
    /// hitbox (red), into <c>docs/art/hitbox_audit/&lt;Boss&gt;_fitted.png</c>, and logs how much of
    /// each pose lies inside its hurtbox. Re-run it after any art or hitbox change.
    /// </summary>
    public static class BossHurtboxSheets
    {
        private const string OutDir = "docs/art/hitbox_audit";
        private const string DataDir = "Assets/_Project/Data/Bosses";
        private const string PrefabDir = "Assets/_Project/Prefabs/Bosses";
        private const int Scale = 28;
        private const float MinX = -7f, MaxX = 7f, MinY = -4f, MaxY = 4.5f;
        private static readonly Color32 Paper = new(235, 225, 205, 255);
        private static readonly Color32 Blue = new(0, 90, 255, 255);
        private static readonly Color32 Red = new(255, 0, 0, 255);

        private readonly struct Panel
        {
            public readonly string Label;
            public readonly Sprite Sprite;
            public readonly bool IsAttack;
            public readonly Vector2 HitOffset, HitSize;

            public Panel(string label, Sprite sprite, bool isAttack, Vector2 hitOffset, Vector2 hitSize)
            {
                Label = label;
                Sprite = sprite;
                IsAttack = isAttack;
                HitOffset = hitOffset;
                HitSize = hitSize;
            }
        }

        [MenuItem("Grace Run/Bosses/Export Hurtbox Sheets")]
        public static void ExportAll()
        {
            Directory.CreateDirectory(OutDir);
            var log = new StringBuilder("Hurtbox sheets (share of the pose inside its hurtbox):\n");
            foreach (var folder in AssetDatabase.GetSubFolders(DataDir))
            {
                var name = Path.GetFileName(folder);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{name}.prefab");
                var guids = AssetDatabase.FindAssets("t:BossPoseSet", new[] { folder });
                if (prefab == null || guids.Length == 0) continue;
                var poses = AssetDatabase.LoadAssetAtPath<BossPoseSet>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (poses.Idle == null || poses.Idle.Length == 0) continue;
                Export(name, prefab, poses, folder, log);
            }
            Debug.Log(log.ToString());
        }

        private static void Export(string name, GameObject prefab, BossPoseSet poses, string folder, StringBuilder log)
        {
            var bodyPosition = Vector2.zero;
            foreach (var renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name == "Body") bodyPosition = renderer.transform.position - prefab.transform.position;

            var panels = new List<Panel> { new("idle", poses.Idle[0], false, default, default) };
            var seen = new HashSet<Sprite>();
            foreach (var guid in AssetDatabase.FindAssets("t:AttackData", new[] { folder }))
            {
                var attack = AssetDatabase.LoadAssetAtPath<AttackData>(AssetDatabase.GUIDToAssetPath(guid));
                var pose = poses.PoseFor(attack.Id);
                if (pose == null || pose.Strike == null || !seen.Add(pose.Strike)) continue;
                panels.Add(new Panel(attack.Id, pose.Strike, true, attack.HitboxOffset, attack.HitboxSize));
            }

            var width = (int)((MaxX - MinX) * Scale);
            var height = (int)((MaxY - MinY) * Scale);
            const int columns = 3;
            var rows = (panels.Count + columns - 1) / columns;
            var sheet = new Texture2D(width * columns, height * rows, TextureFormat.RGBA32, false);
            var fill = new Color32[sheet.width * sheet.height];
            for (var i = 0; i < fill.Length; i++) fill[i] = Paper;
            sheet.SetPixels32(fill);

            for (var i = 0; i < panels.Count; i++)
            {
                var originX = i % columns * width;
                var originY = (rows - 1 - i / columns) * height;
                var panel = panels[i];
                poses.TryGetHurtbox(panel.Sprite, out var hurtOffset, out var hurtSize);
                var hurt = new Rect(hurtOffset - hurtSize / 2f, hurtSize);
                var share = DrawSprite(sheet, panel.Sprite, bodyPosition, hurt, originX, originY, width, height);
                log.AppendLine($"  {name} {panel.Label}: {share:P0}, box {hurtSize.x:F1} x {hurtSize.y:F1}");
                DrawRect(sheet, hurt, originX, originY, Blue);
                if (panel.IsAttack)
                    DrawRect(sheet, new Rect(panel.HitOffset - panel.HitSize / 2f, panel.HitSize), originX, originY, Red);
            }
            sheet.Apply();
            File.WriteAllBytes($"{OutDir}/{name}_fitted.png", ImageConversion.EncodeToPNG(sheet));
            Object.DestroyImmediate(sheet);
        }

        /// <summary>Paints the sprite mirrored (flipX) and returns the share of its pixels inside <paramref name="hurt"/>.</summary>
        private static float DrawSprite(Texture2D sheet, Sprite sprite, Vector2 bodyPosition, Rect hurt, int originX, int originY, int width, int height)
        {
            var texture = new Texture2D(2, 2);
            ImageConversion.LoadImage(texture, File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite.texture)));
            var rect = sprite.textureRect;
            var pixels = texture.GetPixels32();
            var untrimmedX = rect.xMin - sprite.textureRectOffset.x;
            var untrimmedY = rect.yMin - sprite.textureRectOffset.y;
            int total = 0, inside = 0;
            for (var y = (int)rect.yMin; y < (int)rect.yMax; y++)
            for (var x = (int)rect.xMin; x < (int)rect.xMax; x++)
            {
                var pixel = pixels[y * texture.width + x];
                if (pixel.a < 40) continue;
                var worldX = -((x - untrimmedX - sprite.pivot.x) / sprite.pixelsPerUnit) + bodyPosition.x;
                var worldY = (y - untrimmedY - sprite.pivot.y) / sprite.pixelsPerUnit + bodyPosition.y;
                total++;
                if (hurt.Contains(new Vector2(worldX, worldY))) inside++;
                var sx = originX + (int)((worldX - MinX) * Scale);
                var sy = originY + (int)((worldY - MinY) * Scale);
                if (sx >= originX && sx < originX + width && sy >= originY && sy < originY + height) sheet.SetPixel(sx, sy, pixel);
            }
            Object.DestroyImmediate(texture);
            return total == 0 ? 0f : inside / (float)total;
        }

        private static void DrawRect(Texture2D sheet, Rect rect, int originX, int originY, Color32 color)
        {
            var x0 = originX + (int)((rect.xMin - MinX) * Scale);
            var x1 = originX + (int)((rect.xMax - MinX) * Scale);
            var y0 = originY + (int)((rect.yMin - MinY) * Scale);
            var y1 = originY + (int)((rect.yMax - MinY) * Scale);
            for (var x = x0; x <= x1; x++) { Set(sheet, x, y0, color); Set(sheet, x, y1, color); }
            for (var y = y0; y <= y1; y++) { Set(sheet, x0, y, color); Set(sheet, x1, y, color); }
        }

        private static void Set(Texture2D sheet, int x, int y, Color32 color)
        {
            if (x >= 0 && x < sheet.width && y >= 0 && y < sheet.height) sheet.SetPixel(x, y, color);
        }
    }
}
