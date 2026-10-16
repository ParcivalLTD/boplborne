using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Placeholder look for round effects on every PC: the sprite grows to the ability's radius and
    /// fades out over the effect's lifetime.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class PulseView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;

        private AbilityEffect _effect;
        private Color _baseColor;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
            _baseColor = _sprite.color;
        }

        private void LateUpdate()
        {
            var t = _effect.Age01;
            var diameter = _effect.Ability.EffectRadius * 2f * t;
            _sprite.transform.localScale = new Vector3(diameter, diameter, 1f);
            _sprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * (1f - t));
        }
    }
}
