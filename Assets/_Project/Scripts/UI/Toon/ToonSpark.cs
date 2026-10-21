using UnityEngine;
using UnityEngine.UI;

namespace GraceRun.UI.Toon
{
    /// <summary>One flying star of a <see cref="ToonBurst"/>: flies out, spins, falls, fades.</summary>
    public class ToonSpark : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _spin;
        private float _life;
        private float _age;
        private Image _image;

        public static void Spawn(Transform parent, Vector2 anchoredPosition, Sprite sprite, Vector2 velocity, float life, float size)
        {
            if (sprite == null) return;
            var go = new GameObject("Spark", typeof(RectTransform), typeof(Image), typeof(ToonSpark));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(size, size);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            var spark = go.GetComponent<ToonSpark>();
            spark._velocity = velocity;
            spark._life = life;
            spark._spin = Random.Range(-360f, 360f);
            spark._image = image;
        }

        private void Update()
        {
            var dt = Time.unscaledDeltaTime;
            _age += dt;
            _velocity.y -= 900f * dt;
            var rect = (RectTransform)transform;
            rect.anchoredPosition += _velocity * dt;
            rect.localRotation = Quaternion.Euler(0f, 0f, ToonMotion.Stepped(_age, 12f) * _spin);
            var fade = Mathf.Clamp01(1f - (_age - _life * 0.6f) / (_life * 0.4f));
            _image.color = new Color(1f, 1f, 1f, fade);
            if (_age >= _life) Destroy(gameObject);
        }
    }
}
