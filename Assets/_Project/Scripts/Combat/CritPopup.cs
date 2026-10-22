using GraceRun.UI.Skin;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// A short world-space word that pops, rises and fades above a crit, gold over an ink outline.
    /// Local cosmetic only; destroys itself when done. Does nothing in batch mode (server-only builds).
    /// </summary>
    public class CritPopup : MonoBehaviour
    {
        private const float PopTime = 0.12f;
        private const float PopOvershoot = 1.35f;
        private const float CharacterSize = 0.15f;
        private const float OutlineOffset = 0.07f;
        private static readonly Color Ink = new(0.07f, 0.06f, 0.09f, 1f);
        private static readonly Vector2[] OutlineDirections = { new(1f, 0f), new(-1f, 0f), new(0f, 1f), new(0f, -1f), new(1f, 1f), new(-1f, 1f), new(1f, -1f), new(-1f, -1f) };

        private TextMesh _text;
        private TextMesh[] _outline;
        private Color _color;
        private float _lifetime;
        private float _rise;
        private float _age;
        private Vector3 _start;

        /// <summary>Spawns a popup at <paramref name="position"/>.</summary>
        public static void Spawn(string text, Vector3 position, Color color, int size, float lifetime, float rise)
        {
            if (Application.isBatchMode || string.IsNullOrEmpty(text)) return;

            var popup = new GameObject("CritPopup").AddComponent<CritPopup>();
            popup.Init(text, position, color, size, lifetime, rise);
        }

        private void Init(string text, Vector3 position, Color color, int size, float lifetime, float rise)
        {
            var font = UiKit.DisplayFont;
            _text = MakeText(transform, "Text", text, font, size, color, 501, Vector2.zero);
            _outline = new TextMesh[OutlineDirections.Length];
            for (var i = 0; i < _outline.Length; i++)
                _outline[i] = MakeText(transform, "Outline" + i, text, font, size, Ink, 500, OutlineDirections[i] * OutlineOffset);

            _color = color;
            _lifetime = Mathf.Max(0.01f, lifetime);
            _rise = rise;
            _start = position;
            transform.position = position;
        }

        private static TextMesh MakeText(Transform parent, string name, string text, Font font, int size, Color color, int order, Vector2 offset)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = offset;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = font;
            mesh.fontSize = size;
            mesh.characterSize = CharacterSize;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = order;
            return mesh;
        }

        private void Update()
        {
            _age += Time.unscaledDeltaTime;
            var t = _age / _lifetime;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = _start + Vector3.up * (_rise * (1f - (1f - t) * (1f - t)));
            var pop = _age < PopTime ? Mathf.Lerp(0.3f, PopOvershoot, _age / PopTime)
                : Mathf.Lerp(PopOvershoot, 1f, Mathf.Clamp01((_age - PopTime) / PopTime));
            transform.localScale = Vector3.one * pop;

            var alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
            var color = _color;
            color.a = alpha;
            _text.color = color;
            var ink = Ink;
            ink.a = alpha;
            foreach (var outline in _outline) outline.color = ink;
        }
    }
}
