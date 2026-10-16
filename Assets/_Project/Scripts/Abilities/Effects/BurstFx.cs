using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>Local placeholder blast: a disc that grows to the radius and fades, then removes itself. Not networked.</summary>
    public class BurstFx : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;

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
            var diameter = _radius * 2f * Mathf.Sqrt(t);
            _sprite.transform.localScale = new Vector3(diameter, diameter, 1f);
            _sprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * (1f - t));
            if (t >= 1f) Destroy(gameObject);
        }

        /// <summary>Starts the visual: grows to <paramref name="radius"/> over <paramref name="duration"/> seconds.</summary>
        public void Play(float radius, float duration)
        {
            _radius = radius;
            _duration = Mathf.Max(0.01f, duration);
            _age = 0f;
        }
    }
}
