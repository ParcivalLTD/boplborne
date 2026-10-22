using System;
using GraceRun.Bosses;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using UnityEditor;
using UnityEngine;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// Turns a copy of the Test Boss prefab into a new boss: points every boss component at the new
    /// <see cref="BossData"/>, adds the lunge and attack sound components, sizes the body and swaps in new shapes.
    /// </summary>
    public static class BossPrefabShell
    {
        /// <param name="path">The copied prefab asset.</param>
        /// <param name="data">The boss data the components read.</param>
        /// <param name="bodySize">Size of the root collider (the boss root sits half of this above the floor).</param>
        /// <param name="build">Fills the emptied <c>Visuals</c> child: (visuals, square sprite).</param>
        /// <param name="extra">Optional: add and wire boss-specific components on the root.</param>
        public static void Build(string path, BossData data, Vector2 bodySize, Action<Transform, Sprite> build, Action<GameObject> extra = null)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                root.name = System.IO.Path.GetFileNameWithoutExtension(path);
                foreach (var type in new[] { typeof(BossAttackRunner), typeof(BossMotor), typeof(BossTargetSelector), typeof(BossBrain) })
                    BossBuildKit.SetObject(root.GetComponent(type), "_data", data);
                BossBuildKit.SetObject(root.GetComponent<Health>(), "_stats", data);

                var box = root.GetComponent<BoxCollider2D>();
                box.size = bodySize;
                box.offset = Vector2.zero;

                var visuals = root.transform.Find("Visuals");
                var square = BossBuildKit.ClearShapes(visuals);
                build(visuals, square);
                WireFlash(root, visuals);

                if (root.GetComponent<BossLunge>() == null) root.AddComponent<BossLunge>();
                var audio = root.GetComponent<BossAttackAudio>() ?? root.AddComponent<BossAttackAudio>();
                BossBuildKit.SetObject(audio, "_runner", root.GetComponent<BossAttackRunner>());

                extra?.Invoke(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireFlash(GameObject root, Transform visuals)
        {
            var renderers = visuals.GetComponentsInChildren<SpriteRenderer>(true);
            var so = new SerializedObject(root.GetComponent<SpriteFlash>());
            var list = so.FindProperty("_renderers");
            list.arraySize = renderers.Length;
            for (var i = 0; i < renderers.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
