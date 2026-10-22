using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Local blast visual, then removes itself. Not networked. With frames it plays the drawn
    /// animation over its duration, sized so the art's body fills the ability radius; without them
    /// it is the placeholder disc that grows and fades.
    /// </summary>
    public class BurstFx : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField, Tooltip("Drawn frames (imported 1 unit wide). Empty = the placeholder disc.")]
        private Sprite[] _frames;
        [SerializeField, Tooltip("Width of the frames = this x the radius x 2 (the art fills only part of its canvas).")]
        private float _radiusFactor = 1.5f;

        private Color _baseColor;
        private float _radius;
        private float _duration = 1f;
        private float _age;

        private void Awake()
        {
            _baseColor = _sprite.color;
        }

        private void Update()
        {
            _age += Time.deltaTime;
            var t = Mathf.Clamp01(_age / _duration);
            if (_frames != null && _frames.Length > 0)
            {
                var width = _radius * 2f * _radiusFactor;
                _sprite.transform.localScale = new Vector3(width, width, 1f);
                _sprite.sprite = _frames[Mathf.Min(_frames.Length - 1, Mathf.FloorToInt(t * _frames.Length))];
            }
            else
            {
                var diameter = _radius * 2f * Mathf.Sqrt(t);
                _sprite.transform.localScale = new Vector3(diameter, diameter, 1f);
                _sprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * (1f - t));
            }
            if (t >= 1f) Destroy(gameObject);
        }

        /// <summary>Starts the visual: fills <paramref name="radius"/> over <paramref name="duration"/> seconds.</summary>
        public void Play(float radius, float duration)
        {
            _radius = radius;
            _duration = Mathf.Max(0.01f, duration);
            _age = 0f;
        }
    }
}
