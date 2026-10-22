using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Plays a hand-drawn frame strip on the effect's visual sprite on every PC: looping (auras, orbs,
    /// pools) or stretched over the effect's lifetime (traps, blasts). The art is imported 1 unit wide,
    /// so the visual's scale is its world width: fixed, or from the ability's radius.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        private enum Playback { Loop, OverLifetime }

        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(1f)] private float _fps = 12f;
        [SerializeField] private Playback _playback = Playback.Loop;
        [SerializeField, Tooltip("Width in world units when the ability has no radius to follow.")]
        private float _fixedWidth = 1f;
        [SerializeField, Tooltip("Width = this x the ability's diameter (art that fills only part of its canvas needs more than 1). 0 = use the fixed width.")]
        private float _radiusFactor;
        [SerializeField, Range(0f, 1f), Tooltip("Fades out over this last share of the lifetime. 0 = no fade.")]
        private float _fadeTail;
        [SerializeField, Tooltip("Degrees per second the sprite spins (boomerangs, blades). 0 = none.")]
        private float _spin;

        private AbilityEffect _effect;
        private SpriteRenderer _renderer;
        private Color _baseColor;
        private float _localAge;

        private void Awake()
        {
            _effect = GetComponentInParent<AbilityEffect>();
            _renderer = GetComponent<SpriteRenderer>();
            _baseColor = _renderer.color;
            var width = _fixedWidth;
            if (_radiusFactor > 0f && _effect != null && _effect.Ability != null)
                width = _effect.Ability.EffectRadius * 2f * _radiusFactor;
            transform.localScale = new Vector3(width, width, 1f);
        }

        private void LateUpdate()
        {
            if (_frames == null || _frames.Length == 0) return;
            _localAge += Time.deltaTime;
            var age = _effect != null ? _effect.Age : _localAge;
            var life01 = _effect != null ? _effect.Age01 : 0f;

            var index = _playback == Playback.OverLifetime && _effect != null && _effect.Ability.EffectLifetime > 0f
                ? Mathf.FloorToInt(life01 * _frames.Length)
                : Mathf.FloorToInt(age * _fps) % _frames.Length;
            _renderer.sprite = _frames[Mathf.Clamp(index, 0, _frames.Length - 1)];

            if (_fadeTail > 0f)
            {
                var fade = Mathf.Clamp01((1f - life01) / _fadeTail);
                _renderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * fade);
            }
            if (_spin != 0f) transform.localRotation = Quaternion.Euler(0f, 0f, age * _spin);
        }
    }
}
