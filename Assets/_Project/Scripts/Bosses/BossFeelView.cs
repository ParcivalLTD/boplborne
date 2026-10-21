using System.Collections.Generic;
using GraceRun.Combat;
using GraceRun.Core.Juice;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// Local cosmetics for the boss fight feel, on every PC: the gauge above each boss (poise, interrupt
    /// progress, stagger time), a label for its variant / enrage / adaptation, a marker over the player
    /// it is hunting, and the shake and sound when it roars or is staggered. Placeholder shapes, no art needed.
    /// </summary>
    public class BossFeelView : MonoBehaviour
    {
        private const float BarWidth = 4f;
        private const float BarHeight = 0.28f;
        private const float BarHeightAboveBoss = 5.2f;
        private const float MarkerHeightAbovePlayer = 2.6f;
        private const int SortingOrder = 100;

        private static readonly Color Back = new(0f, 0f, 0f, 0.6f);
        private static readonly Color PoiseColor = new(0.7f, 0.85f, 1f);
        private static readonly Color InterruptColor = new(1f, 0.8f, 0.2f);
        private static readonly Color StaggerColor = new(1f, 0.55f, 0.2f);
        private static readonly Color TargetYou = new(1f, 0.35f, 0.3f);
        private static readonly Color TargetOther = new(1f, 0.9f, 0.4f);

        private readonly Dictionary<BossBrain, Overlay> _overlays = new();
        private readonly List<BossBrain> _gone = new();
        private Sprite _square;
        private Sprite _triangle;
        private Font _font;

        private sealed class Overlay
        {
            public Transform Root;
            public Transform Fill;
            public SpriteRenderer FillRenderer;
            public SpriteRenderer BackRenderer;
            public TextMesh Label;
            public Transform Marker;
            public SpriteRenderer MarkerRenderer;
            public System.Action Roar;
            public System.Action Stagger;
        }

        private void Awake()
        {
            _square = MakeSprite(false);
            _triangle = MakeSprite(true);
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void OnDestroy()
        {
            foreach (var pair in _overlays) Release(pair.Key, pair.Value);
            _overlays.Clear();
        }

        private void LateUpdate()
        {
            var bosses = BossBrain.All;
            for (var i = 0; i < bosses.Count; i++)
            {
                var brain = bosses[i];
                if (brain == null || !brain.IsSpawned) continue;
                if (!_overlays.TryGetValue(brain, out var overlay)) _overlays[brain] = overlay = Create(brain);
                Refresh(brain, overlay);
            }

            _gone.Clear();
            foreach (var pair in _overlays)
                if (pair.Key == null || !pair.Key.IsSpawned) _gone.Add(pair.Key);
            for (var i = 0; i < _gone.Count; i++)
            {
                var brain = _gone[i];
                Release(brain, _overlays[brain]);
                _overlays.Remove(brain);
            }
        }

        private Overlay Create(BossBrain brain)
        {
            var overlay = new Overlay { Root = new GameObject("BossFeelOverlay").transform };
            overlay.BackRenderer = Quad(overlay.Root, "Back", Back, SortingOrder);
            overlay.BackRenderer.transform.localScale = new Vector3(BarWidth + 0.12f, BarHeight + 0.12f, 1f);
            overlay.FillRenderer = Quad(overlay.Root, "Fill", PoiseColor, SortingOrder + 1);
            overlay.Fill = overlay.FillRenderer.transform;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(overlay.Root, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            overlay.Label = labelObject.AddComponent<TextMesh>();
            overlay.Label.font = _font;
            overlay.Label.fontSize = 64;
            overlay.Label.characterSize = 0.08f;
            overlay.Label.anchor = TextAnchor.LowerCenter;
            overlay.Label.alignment = TextAlignment.Center;
            var labelRenderer = labelObject.GetComponent<MeshRenderer>();
            labelRenderer.sharedMaterial = _font.material;
            labelRenderer.sortingOrder = SortingOrder + 2;

            overlay.MarkerRenderer = new GameObject("TargetMarker").AddComponent<SpriteRenderer>();
            overlay.MarkerRenderer.sprite = _triangle;
            overlay.MarkerRenderer.sortingOrder = SortingOrder;
            overlay.Marker = overlay.MarkerRenderer.transform;

            overlay.Roar = () => OnRoar(brain);
            overlay.Stagger = () => Shake.Play(0.5f);
            brain.TransitionStarted += overlay.Roar;
            brain.StaggerStarted += overlay.Stagger;
            return overlay;
        }

        private static void OnRoar(BossBrain brain)
        {
            Shake.Play(1f);
            var cue = brain.Data != null ? brain.Data.Feel.TransitionCue : null;
            if (cue != null) cue.Play();
        }

        private static void Release(BossBrain brain, Overlay overlay)
        {
            if (brain != null)
            {
                brain.TransitionStarted -= overlay.Roar;
                brain.StaggerStarted -= overlay.Stagger;
            }
            if (overlay.Root != null) Destroy(overlay.Root.gameObject);
            if (overlay.Marker != null) Destroy(overlay.Marker.gameObject);
        }

        private void Refresh(BossBrain brain, Overlay overlay)
        {
            var dead = brain.Health.IsDead;
            var kind = brain.GaugeKind;
            var showBar = !dead && kind != BossGaugeKind.None;
            overlay.Root.gameObject.SetActive(!dead);
            overlay.BackRenderer.enabled = overlay.FillRenderer.enabled = showBar;

            overlay.Root.position = brain.transform.position + new Vector3(0f, BarHeightAboveBoss, 0f);
            var fill = Mathf.Clamp01(brain.GaugeFill);
            overlay.Fill.localScale = new Vector3(Mathf.Max(0.001f, BarWidth * fill), BarHeight, 1f);
            overlay.Fill.localPosition = new Vector3(-BarWidth * 0.5f + BarWidth * fill * 0.5f, 0f, 0f);
            overlay.FillRenderer.color = kind == BossGaugeKind.Interrupt ? InterruptColor
                : kind == BossGaugeKind.Stagger ? StaggerColor : PoiseColor;

            overlay.Label.text = LabelFor(brain, kind);
            overlay.Label.color = kind == BossGaugeKind.Interrupt ? InterruptColor : Color.white;
            RefreshMarker(brain, overlay, dead);
        }

        private static string LabelFor(BossBrain brain, BossGaugeKind kind)
        {
            var text = kind == BossGaugeKind.Interrupt ? "INTERRUPT: team up and hit it!"
                : kind == BossGaugeKind.Stagger ? "STAGGERED" : string.Empty;
            if (brain.Variant != BossVariant.None) text += (text.Length > 0 ? "\n" : string.Empty) + brain.Variant.ToString().ToUpperInvariant();
            if (brain.IsEnraged) text += (text.Length > 0 ? "\n" : string.Empty) + "ENRAGED";
            if (brain.AdaptedSource != 0) text += (text.Length > 0 ? "\n" : string.Empty) + "ADAPTING: vary your abilities";
            return text;
        }

        private void RefreshMarker(BossBrain brain, Overlay overlay, bool dead)
        {
            var manager = NetworkManager.Singleton;
            var visible = false;
            if (!dead && manager != null && manager.SpawnManager != null && brain.TargetId != BossTargeting.NoTarget &&
                manager.SpawnManager.SpawnedObjects.TryGetValue(brain.TargetId, out var player) &&
                player.TryGetComponent(out Health health) && !health.IsDead)
            {
                var bob = Mathf.Sin(Time.time * 6f) * 0.15f;
                overlay.Marker.position = player.transform.position + new Vector3(0f, MarkerHeightAbovePlayer + bob, 0f);
                overlay.Marker.localScale = Vector3.one * (player.IsOwner ? 1.2f : 0.8f);
                overlay.MarkerRenderer.color = player.IsOwner ? TargetYou : TargetOther;
                visible = true;
            }
            overlay.MarkerRenderer.enabled = visible;
        }

        private SpriteRenderer Quad(Transform parent, string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static Sprite MakeSprite(bool triangle)
        {
            const int size = 16;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                // A downward-pointing triangle: the row gets narrower toward y = 0.
                var half = (y + 1) * 0.5f;
                var inside = !triangle || Mathf.Abs(x + 0.5f - size * 0.5f) <= half;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
