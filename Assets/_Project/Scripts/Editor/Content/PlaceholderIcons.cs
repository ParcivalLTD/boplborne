using System.IO;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Writes a 32×32 placeholder ability icon (a disc in the ability's colour on a dark tile)
    /// unless one already exists, and imports it as a point-filtered sprite. The art pass replaces them.
    /// </summary>
    public static class PlaceholderIcons
    {
        private const int Size = 32;
        private static readonly Color Tile = new(0.165f, 0.145f, 0.2f, 1f);
        private static readonly Color Rim = new(0.929f, 0.906f, 0.965f, 1f);

        public static Sprite Ensure(string folder, string id, Color color)
        {
            var path = $"{folder}/ability_{id}.png";
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, Draw(new Color(color.r, color.g, color.b, 1f)).EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = Size;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Texture2D Draw(Color color)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var centre = (Size - 1) * 0.5f;
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var distance = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));
                var pixel = distance < 9.5f ? color : distance < 11f ? Rim : Tile;
                texture.SetPixel(x, y, pixel);
            }
            texture.Apply();
            return texture;
        }
    }
}
