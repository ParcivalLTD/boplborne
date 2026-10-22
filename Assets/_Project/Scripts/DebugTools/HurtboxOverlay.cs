using System.Collections.Generic;
using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.DebugTools
{
    /// <summary>
    /// While "Show hitboxes" is on in the F1 menu, outlines every live hurtbox (bosses, crawlers,
    /// players) in blue, using the collider's world bounds. Armed attack boxes are drawn by
    /// <see cref="HitboxDebugView"/>. Debug only; created by <see cref="DebugMenu"/>.
    /// </summary>
    public class HurtboxOverlay : MonoBehaviour
    {
        private const float LineWidth = 0.06f;
        private const int SortingOrder = 1000;
        private static readonly Color OutlineColor = new(0.1f, 0.45f, 1f, 1f);

        private readonly List<LineRenderer> _lines = new();
        private Material _material;

        private void LateUpdate()
        {
            var used = 0;
            if (DebugMenu.Instance != null && DebugMenu.Instance.ShowHitboxes)
            {
                foreach (var hurtbox in Hurtbox.All)
                {
                    if (hurtbox == null || !hurtbox.isActiveAndEnabled) continue;
                    if (!hurtbox.TryGetComponent(out Collider2D collider) || !collider.enabled) continue;
                    Outline(Line(used++), collider.bounds);
                }
            }
            for (var i = used; i < _lines.Count; i++) _lines[i].enabled = false;
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }

        private LineRenderer Line(int index)
        {
            if (index < _lines.Count) return _lines[index];

            if (_material == null) _material = new Material(Shader.Find("Sprites/Default"));
            var child = new GameObject("HurtboxOutline");
            child.transform.SetParent(transform, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.loop = true;
            line.positionCount = 4;
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = LineWidth;
            line.startColor = line.endColor = OutlineColor;
            line.sortingOrder = SortingOrder;
            _lines.Add(line);
            return line;
        }

        private static void Outline(LineRenderer line, Bounds bounds)
        {
            line.enabled = true;
            var min = bounds.min;
            var max = bounds.max;
            line.SetPosition(0, new Vector3(min.x, min.y, 0f));
            line.SetPosition(1, new Vector3(min.x, max.y, 0f));
            line.SetPosition(2, new Vector3(max.x, max.y, 0f));
            line.SetPosition(3, new Vector3(max.x, min.y, 0f));
        }
    }
}
