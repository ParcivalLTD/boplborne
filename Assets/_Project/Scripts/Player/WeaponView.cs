using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Placeholder look of the equipped weapon on every PC: a coloured bar that rests on the
    /// player's back and points along the swing while attacking (drawn back during startup,
    /// out during the active frames). Fists draw nothing. Cosmetic only.
    /// </summary>
    public class WeaponView : MonoBehaviour
    {
        private const float RestAngle = 50f;

        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private SpriteRenderer _reference;
        [SerializeField, Min(0f), Tooltip("Distance from the player's centre to the near end of the bar while swinging.")]
        private float _grip = 0.4f;
        [SerializeField, Range(0f, 1f), Tooltip("How far out the bar sits during startup (1 = fully out).")]
        private float _windUp = 0.45f;
        [SerializeField, Tooltip("Where the bar rests, for a player facing right.")]
        private Vector2 _restOffset = new(-0.3f, 0.2f);

        private SpriteRenderer _bar;

        private void Awake()
        {
            var child = new GameObject("WeaponBar");
            child.transform.SetParent(transform, false);
            _bar = child.AddComponent<SpriteRenderer>();
            _bar.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0f, 0.5f), 1f);
            if (_reference != null)
            {
                _bar.sortingLayerID = _reference.sortingLayerID;
                _bar.sortingOrder = _reference.sortingOrder + 1;
            }
            _bar.enabled = false;
        }

        private void OnEnable()
        {
            _weapon.Changed += Show;
            Show(_weapon.Current);
        }

        private void OnDisable() => _weapon.Changed -= Show;

        private void LateUpdate()
        {
            if (!_bar.enabled) return;
            var phase = _attack.Phase;
            var side = AbilityMath.SideOf(_attack.Aim.x);
            Vector2 direction;
            if (phase == SwingPhase.Idle)
            {
                direction = new Vector2(-side * Mathf.Cos(RestAngle * Mathf.Deg2Rad), Mathf.Sin(RestAngle * Mathf.Deg2Rad));
                _bar.transform.localPosition = new Vector3(_restOffset.x * side, _restOffset.y, 0f);
            }
            else
            {
                direction = _attack.SwingDirection;
                var reach = phase == SwingPhase.Startup ? _grip * _windUp : _grip;
                _bar.transform.localPosition = direction * reach;
            }
            _bar.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private void Show(WeaponData weapon)
        {
            var size = weapon.VisualSize;
            _bar.enabled = size.x > 0f;
            _bar.color = weapon.Color;
            _bar.transform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}
