using System.Collections.Generic;
using System.IO;
using GraceRun.Bosses;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Grace Run → Bosses → Fit Pose Hurtboxes: fills <c>_hurtboxes</c> of every boss pose set with a
    /// body-only box per pose sprite, measured from the sprite's opaque pixels (see
    /// <see cref="HurtboxFit"/>). Boxes are stored for a boss facing right (the body art is mirrored
    /// with <c>flipX</c>) relative to the boss root. Safe to run again after any art change.
    /// </summary>
    public static class BossHurtboxFitter
    {
        /// <summary>A column or row needs this share of the densest one to count as body (small: stray specks only, thin parts are removed by the opening).</summary>
        private const float BodyDensity = 0.08f;
        /// <summary>Pixels below this alpha are baked-in effects (swoosh arcs, glows), not body.</summary>
        private const byte MinAlpha = 200;
        private const byte PaleChannel = 190;
        /// <summary>Outline ink: r + g + b below this. Body art is outlined, a baked-in slash arc is not.</summary>
        private const int InkSum = 210;
        /// <summary>Body pixels must lie within this many pixels of ink.</summary>
        private const int InkRadius = 14;
        /// <summary>Parts thinner than 2 * this + 1 pixels (legs, blades, rapier) are not body.</summary>
        private const int OpenRadius = 4;
        /// <summary>A fitted box smaller than this (units) is a failed fit; the pose keeps the prefab box.</summary>
        private const float MinBoxSize = 1f;
        private const string DataDir = "Assets/_Project/Data/Bosses";
        private const string PrefabDir = "Assets/_Project/Prefabs/Bosses";

        [MenuItem("Grace Run/Bosses/Fit Pose Hurtboxes")]
        public static void FitAll()
        {
            var count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:BossPoseSet", new[] { DataDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var folder = Path.GetFileName(Path.GetDirectoryName(path));
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{folder}.prefab");
                if (prefab == null) continue;
                count += Fit(AssetDatabase.LoadAssetAtPath<BossPoseSet>(path), prefab);
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Fitted {count} pose hurtboxes.");
        }

        private static int Fit(BossPoseSet set, GameObject prefab)
        {
            var bodyPosition = BodyPosition(prefab);
            var sprites = new List<Sprite>();
            var so = new SerializedObject(set);
            Collect(so, "_idle", sprites);
            Collect(so, "_death", sprites);
            AddUnique(sprites, so.FindProperty("_stagger").objectReferenceValue as Sprite);
            var attacks = so.FindProperty("_attacks");
            for (var i = 0; i < attacks.arraySize; i++)
            {
                var attack = attacks.GetArrayElementAtIndex(i);
                foreach (var pose in new[] { "Windup", "Strike", "Recovery" })
                    AddUnique(sprites, attack.FindPropertyRelative(pose).objectReferenceValue as Sprite);
            }

            var boxes = so.FindProperty("_hurtboxes");
            boxes.arraySize = 0;
            foreach (var sprite in sprites)
            {
                if (!TryFit(sprite, bodyPosition, out var offset, out var size)) continue;
                boxes.arraySize++;
                var entry = boxes.GetArrayElementAtIndex(boxes.arraySize - 1);
                entry.FindPropertyRelative("Sprite").objectReferenceValue = sprite;
                entry.FindPropertyRelative("Offset").vector2Value = offset;
                entry.FindPropertyRelative("Size").vector2Value = size;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(set);
            return boxes.arraySize;
        }

        /// <summary>Where the Body renderer's pivot sits relative to the boss root.</summary>
        private static Vector2 BodyPosition(GameObject prefab)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.name == "Body") return renderer.transform.position - prefab.transform.position;
            return Vector2.zero;
        }

        private static bool TryFit(Sprite sprite, Vector2 bodyPosition, out Vector2 offset, out Vector2 size)
        {
            offset = default;
            size = default;
            var path = AssetDatabase.GetAssetPath(sprite.texture);
            var texture = new Texture2D(2, 2);
            ImageConversion.LoadImage(texture, File.ReadAllBytes(path));
            var rect = sprite.textureRect;
            var width = (int)rect.width;
            var height = (int)rect.height;
            var pixels = texture.GetPixels32();
            var opaque = new bool[width * height];
            var ink = new bool[width * height];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var pixel = pixels[((int)rect.yMin + y) * texture.width + (int)rect.xMin + x];
                opaque[y * width + x] = pixel.a >= MinAlpha && !IsPale(pixel);
                ink[y * width + x] = pixel.a >= MinAlpha && pixel.r + pixel.g + pixel.b < InkSum;
            }
            opaque = HurtboxFit.Open(HurtboxFit.NearInk(opaque, ink, width, height, InkRadius), width, height, OpenRadius);
            Object.DestroyImmediate(texture);

            var box = HurtboxFit.BodyBox(opaque, width, height, BodyDensity);
            if (box.width <= 0) return false;

            // Pixel (0,0) of the rect is the sprite's trimmed corner, offset from the untrimmed corner the pivot is measured from.
            var originX = sprite.textureRectOffset.x;
            var originY = sprite.textureRectOffset.y;
            var ppu = sprite.pixelsPerUnit;
            var left = -((originX + box.xMax - sprite.pivot.x) / ppu); // flipX mirrors around the pivot
            var right = -((originX + box.xMin - sprite.pivot.x) / ppu);
            var bottom = (originY + box.yMin - sprite.pivot.y) / ppu;
            var top = (originY + box.yMax - sprite.pivot.y) / ppu;
            offset = new Vector2((left + right) / 2f, (bottom + top) / 2f) + bodyPosition;
            size = new Vector2(right - left, top - bottom);
            return size.x >= MinBoxSize && size.y >= MinBoxSize; // a degenerate fit keeps the prefab box
        }

        /// <summary>Near-white pixels are slash arcs, web balls, lightning and gloves, never the body mass.</summary>
        private static bool IsPale(Color32 pixel) => pixel.r > PaleChannel && pixel.g > PaleChannel && pixel.b > PaleChannel;

        private static void Collect(SerializedObject so, string array, List<Sprite> into)
        {
            var property = so.FindProperty(array);
            for (var i = 0; i < property.arraySize; i++)
                AddUnique(into, property.GetArrayElementAtIndex(i).objectReferenceValue as Sprite);
        }

        private static void AddUnique(List<Sprite> into, Sprite sprite)
        {
            if (sprite != null && !into.Contains(sprite)) into.Add(sprite);
        }
    }
}
