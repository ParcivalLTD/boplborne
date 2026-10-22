using GraceRun.UI.Skin;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// A short world-space word that pops, rises and fades above a crit. Local cosmetic only;
    /// destroys itself when done. Does nothing in batch mode (server-only builds).
    /// </summary>
    public class CritPopup : MonoBehaviour
    {
        private const float PopTime = 0.12f;
        private const float PopOvershoot = 1.35f;

        private TextMesh _text;
        private MeshRenderer _renderer;
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
            _text = gameObject.AddComponent<TextMesh>();
            _text.text = text;
            _text.font = font;
            _text.fontSize = size;
            _text.characterSize = 0.05f;
            _text.anchor = TextAnchor.MiddleCenter;
            _text.alignment = TextAlignment.Center;
            _text.color = color;
            _renderer = GetComponent<MeshRenderer>();
            _renderer.sharedMaterial = font.material;
            _renderer.sortingOrder = 500;

            _color = color;
            _lifetime = Mathf.Max(0.01f, lifetime);
            _rise = rise;
            _start = position;
            transform.position = position;
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

            var color = _color;
            color.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
            _text.color = color;
        }
    }
}
