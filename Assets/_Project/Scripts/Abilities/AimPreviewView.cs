using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// The owner's aim line: shown while up or down is held, or while an ability button is held in
    /// Hold to aim mode. The line runs along the aim, and a ring marks the enemy the aim has snapped
    /// to. Drawn cream over an ink outline so it reads on any backdrop. Local cosmetic only; created
    /// by <see cref="AbilityRunner"/> on the owning PC.
    /// </summary>
    public class AimPreviewView : MonoBehaviour
    {
        private const int RingPoints = 28;
        private const int SortingOrder = 200;
        private const float OutlineScale = 2.6f;
        private static readonly Color OutlineColor = new(0.07f, 0.06f, 0.09f, 0.85f);

        private AbilityRunner _runner;
        private AbilityLoadout _loadout;
        private Material _material;
        private LineRenderer _line;
        private LineRenderer _lineOutline;
        private LineRenderer _ring;
        private LineRenderer _ringOutline;

        /// <summary>Binds the view to the runner it draws for.</summary>
        public void Setup(AbilityRunner runner, AbilityLoadout loadout)
        {
            _runner = runner;
            _loadout = loadout;
            _material = new Material(Shader.Find("Sprites/Default"));
            _lineOutline = CreateLine("AimLineOutline", 2, false, SortingOrder);
            _line = CreateLine("AimLine", 2, false, SortingOrder + 1);
            _ringOutline = CreateLine("AimRingOutline", RingPoints, true, SortingOrder);
            _ring = CreateLine("AimRing", RingPoints, true, SortingOrder + 1);
        }

        private void LateUpdate()
        {
            var data = _loadout.Catalog.AimAssist;
            var aim = _runner.PreviewAim();
            var show = data != null && (_runner.HoldingSlot >= 0 || aim.Tilted);
            var showRing = show && aim.HasTarget;
            _line.enabled = _lineOutline.enabled = show;
            _ring.enabled = _ringOutline.enabled = showRing;
            if (!show) return;

            Style(_line, data.LineWidth, data.Color);
            Style(_lineOutline, data.LineWidth * OutlineScale, OutlineColor);
            var origin = (Vector2)transform.position;
            var length = aim.HasTarget ? Mathf.Max(0f, aim.Distance - data.RingRadius) : data.PreviewLength;
            var start = origin + aim.Aim * 0.8f;
            var end = origin + aim.Aim * Mathf.Max(0.8f, length);
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
            _lineOutline.SetPosition(0, start);
            _lineOutline.SetPosition(1, end);
            if (!showRing) return;

            Style(_ring, data.LineWidth, data.Color);
            Style(_ringOutline, data.LineWidth * OutlineScale, OutlineColor);
            for (var i = 0; i < RingPoints; i++)
            {
                var angle = i / (float)RingPoints * Mathf.PI * 2f;
                var point = aim.Target + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * data.RingRadius;
                _ring.SetPosition(i, point);
                _ringOutline.SetPosition(i, point);
            }
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }

        private LineRenderer CreateLine(string objectName, int points, bool loop, int order)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = points;
            line.loop = loop;
            line.sortingOrder = order;
            line.enabled = false;
            return line;
        }

        private static void Style(LineRenderer line, float width, Color color)
        {
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
        }
    }
}
