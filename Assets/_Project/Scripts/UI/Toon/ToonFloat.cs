using UnityEngine;

namespace GraceRun.UI.Toon
{
    /// <summary>Bobs and wiggles an element forever, on twos (decor: stars, the title sign). Cosmetic.</summary>
    public class ToonFloat : MonoBehaviour
    {
        [SerializeField] private float _bob = 8f;
        [SerializeField] private float _wiggle = 3f;
        [SerializeField, Min(0.1f)] private float _speed = 1.6f;

        private Vector2 _home;
        private float _phase;

        public void Configure(float bob, float wiggle, float speed = 1.6f)
        {
            _bob = bob;
            _wiggle = wiggle;
            _speed = speed;
        }

        private void Awake()
        {
            _home = ((RectTransform)transform).anchoredPosition;
            _phase = Random.value * 6.28f;
        }

        private void Update()
        {
            var t = ToonMotion.Stepped(Time.unscaledTime, 12f) * _speed + _phase;
            var rect = (RectTransform)transform;
            rect.anchoredPosition = _home + new Vector2(0f, Mathf.Sin(t) * _bob);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f + 1f) * _wiggle);
        }
    }
}
