using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>Placeholder look on every PC: a sprite the size of the ability's radius that fades out over the effect's lifetime.</summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class FadeView : MonoBehaviour
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
            var diameter = _effect.Ability.EffectRadius * 2f;
            _sprite.transform.localScale = new Vector3(diameter, diameter, 1f);
            _sprite.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * (1f - _effect.Age01));
        }
    }
}
