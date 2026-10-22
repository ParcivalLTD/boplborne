using UnityEngine;

namespace GraceRun.Core.Environment
{
    /// <summary>
    /// A soft ellipse on the ground under a body (player, boss, dummy, crawler). It sits on the ground
    /// or platform below the lowest point of the body's collider, shrinks and fades as the body rises,
    /// and vanishes when nothing is underneath. Local cosmetic only; no shadow in batch mode.
    /// </summary>
    public class GroundShadow : MonoBehaviour
    {
        private const int Pixels = 32;
        private static Sprite _sprite;

        [SerializeField] private ShadowData _data;
        [SerializeField, Min(0.2f), Tooltip("Width of the shadow on the ground, in units.")]
        private float _width = 1.4f;
        [SerializeField, Tooltip("The collider whose lowest point the shadow follows; empty: the first collider found.")]
        private Collider2D _body;
        [SerializeField, Tooltip("Sorting layer and order of the shadow: behind the body art, in front of the floor.")]
        private SpriteRenderer _reference;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[1];
        private SpriteRenderer _renderer;
        private ContactFilter2D _filter;

        private void Awake()
        {
            if (Application.isBatchMode || _data == null)
            {
                enabled = false;
                return;
            }
            if (_body == null) _body = GetComponentInChildren<Collider2D>();
            _filter = new ContactFilter2D { useLayerMask = true, layerMask = Layers.StandableMask, useTriggers = false };

            var go = new GameObject("GroundShadow");
            go.transform.SetParent(null);
            _renderer = go.AddComponent<SpriteRenderer>();
            _renderer.sprite = Ellipse();
            if (_reference != null)
            {
                _renderer.sortingLayerID = _reference.sortingLayerID;
                _renderer.sortingOrder = _reference.sortingOrder - 2;
            }
        }

        private void OnDisable()
        {
            if (_renderer != null) _renderer.enabled = false;
        }

        private void OnDestroy()
        {
            if (_renderer != null) Destroy(_renderer.gameObject);
        }

        private void LateUpdate()
        {
            if (_body == null || !_body.enabled || !_body.gameObject.activeInHierarchy)
            {
                _renderer.enabled = false;
                return;
            }

            var bounds = _body.bounds;
            var origin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
            if (Physics2D.Raycast(origin, Vector2.down, _filter, _hits, _data.SearchDistance) == 0)
            {
                _renderer.enabled = false;
                return;
            }

            var height = Mathf.Max(0f, _hits[0].distance - 0.05f);
            var strength = ShadowMath.Strength(height, _data.FadeHeight);
            _renderer.enabled = strength > 0.01f;
            if (!_renderer.enabled) return;

            var scale = _width * ShadowMath.Scale(height, _data.FadeHeight, _data.SmallestScale);
            var shadowHeight = scale * _data.Flatness / SpriteAspect;
            // Most of the ellipse sits above the ground line: the floor art draws over anything below it.
            _renderer.transform.position = new Vector3(origin.x, _hits[0].point.y + shadowHeight * 0.4f, 0f);
            _renderer.transform.localScale = new Vector3(scale, shadowHeight, 1f);
            var color = _data.Color;
            color.a *= strength;
            _renderer.color = color;
        }

        /// <summary>The sprite is a unit-wide ellipse, so its height is width times this.</summary>
        private const float SpriteAspect = 1f;

        /// <summary>A white, hard-edged ellipse one unit wide (tinted by the renderer colour), made once.</summary>
        private static Sprite Ellipse()
        {
            if (_sprite != null) return _sprite;
            var texture = new Texture2D(Pixels, Pixels, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var half = (Pixels - 1) / 2f;
            for (var y = 0; y < Pixels; y++)
            for (var x = 0; x < Pixels; x++)
            {
                var dx = (x - half) / (Pixels / 2f);
                var dy = (y - half) / (Pixels / 2f);
                var inside = dx * dx + dy * dy <= 1f;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
            texture.Apply();
            _sprite = Sprite.Create(texture, new Rect(0, 0, Pixels, Pixels), new Vector2(0.5f, 0.5f), Pixels);
            return _sprite;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _sprite = null;
    }
}
