using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Stone Wall: a solid wall of the ability's Size, standing on the floor below where it spawns
    /// (every PC snaps it down the same way, since the floor is static). Players can stand on it, the
    /// boss can't walk through it, and its <see cref="Combat.AttackBlocker"/> stops attacks. The
    /// visual rises over the ability's visual time.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect), typeof(BoxCollider2D))]
    public class StoneWall : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;

        private AbilityEffect _effect;
        private BoxCollider2D _collider;
        private bool _placed;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
            _collider = GetComponent<BoxCollider2D>();
        }

        private void LateUpdate()
        {
            if (!_effect.IsSpawned) return;
            var ability = _effect.Ability;
            if (!_placed) Place(ability);

            var rise = ability.VisualTime > 0f ? Mathf.Clamp01(_effect.Age / ability.VisualTime) : 1f;
            var height = ability.Size.y * rise;
            _sprite.transform.localScale = new Vector3(ability.Size.x, height, 1f);
            _sprite.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        }

        private void Place(AbilityData ability)
        {
            _placed = true;
            var floor = Physics2D.Raycast(transform.position, Vector2.down, ability.Range, Layers.Mask(Layers.GroundIndex));
            if (floor.collider != null) transform.position = floor.point;
            _collider.size = ability.Size;
            _collider.offset = new Vector2(0f, ability.Size.y * 0.5f);
        }
    }
}
