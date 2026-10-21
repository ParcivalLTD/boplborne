using System.IO;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Imports the pixel effect sheet (4x2 cells of 16x16: stars, impact burst, spark, puffs, speck, ring)
    /// and wires it into the HitSparks and DodgeDust particle prefabs through Texture Sheet Animation.
    /// Menu: Grace Run/Build FX Sprites. Safe to run again.
    /// </summary>
    public static class FxSpritesBuilder
    {
        private const string Sheet = "Assets/_Project/Art/FX/fx_sheet.png";
        private const string Material = "Assets/_Project/Art/Materials/FxSprites.mat";

        [MenuItem("Grace Run/Build FX Sprites")]
        public static void Build()
        {
            AssetDatabase.ImportAsset(Sheet);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Sheet);
            var material = AssetDatabase.LoadAssetAtPath<Material>(Material);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                material = new Material(shader) { name = "FxSprites" };
                AssetDatabase.CreateAsset(material, Material);
            }
            material.SetFloat("_Surface", 1f); // transparent
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = 3000;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetTexture("_BaseMap", texture);
            material.SetTexture("_MainTex", texture);
            EditorUtility.SetDirty(material);

            // frames: 0 star, 1 small star, 2 impact burst, 3 spark | 4 puff, 5 small puff, 6 speck, 7 ring
            Wire("Assets/_Project/Prefabs/FX/HitSparks.prefab", material, 0, 3, 0.7f, 1.1f);
            Wire("Assets/_Project/Prefabs/FX/DodgeDust.prefab", material, 4, 6, 0.5f, 0.8f);
            AssetDatabase.SaveAssets();
        }

        private static void Wire(string path, Material material, int firstFrame, int lastFrame,
            float sizeMin, float sizeMax)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            var system = root.GetComponent<ParticleSystem>();
            var main = system.main;
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            root.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            var sheet = system.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Grid;
            sheet.numTilesX = 4;
            sheet.numTilesY = 2;
            sheet.animation = ParticleSystemAnimationType.SingleRow;
            sheet.rowMode = ParticleSystemAnimationRowMode.Custom;
            sheet.rowIndex = firstFrame / 4;
            sheet.cycleCount = 1;
            // random cell inside the used range: start frame picks a cell, frame over time stays flat
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            sheet.startFrame = new ParticleSystem.MinMaxCurve((firstFrame % 4) / 4f, (lastFrame % 4 + 1) / 4f - 0.001f);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
