using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The equipped weapon's art on every PC: it rests on the player's back (turning with the way
    /// the player runs), is raised or pulled back during the startup, sweeps or thrusts through the
    /// aim during the active frames with a smear, and goes back to rest. The motion comes from the
    /// pure <see cref="WeaponSwingPose"/>; the look from <see cref="WeaponData"/>. Cosmetic only.
    /// </summary>
    public class WeaponView : MonoBehaviour
    {
        private const float RestAngle = 50f;

        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private PlayerLightAttack _attack;
        [SerializeField] private PlayerAnimSync _sync;
        [SerializeField] private SpriteRenderer _reference;
        [SerializeField, Tooltip("Where the weapon rests, for a player facing right.")]
        private Vector2 _restOffset = new(-0.3f, 0.2f);
        [SerializeField, Min(0f), Tooltip("Distance from the player's centre to the middle of the smear.")]
        private float _smearDistance = 1.1f;

        private SpriteRenderer _sprite;
        private SpriteRenderer _smear;
        private WeaponData _current;

        private void Awake()
        {
            _sprite = CreateRenderer("WeaponSprite");
            _smear = CreateRenderer("WeaponSmear");
            _smear.sortingOrder += 1;
        }

        private void OnEnable()
        {
            _weapon.Changed += Show;
            Show(_weapon.Current);
        }

        private void OnDisable() => _weapon.Changed -= Show;

        private void LateUpdate()
        {
            if (_current == null || _current.Sprite == null) return;

            var phase = _attack.Phase;
            var swinging = phase != SwingPhase.Idle;
            var aim = swinging ? _attack.SwingDirection : Vector2.right;
            var side = swinging ? AbilityMath.SideOf(_attack.Aim.x) : _sync.Facing >= 0f ? 1f : -1f;
            var aimAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

            var pose = WeaponSwingPose.Evaluate(_current.SwingLook, phase, _attack.PhaseProgress, aimAngle, side, Rest(side));
            // Behind the body while resting on the back, in front of it while swinging.
            // A weapon with its own animations is drawn into the body (in hand), so the back sprite is hidden.
            _sprite.enabled = !(swinging && _current.BodyPoses != null) && _current.Animations == null;
            _sprite.sortingOrder = _reference != null ? _reference.sortingOrder + (swinging ? 1 : -1) : 0;
            _sprite.transform.localPosition = pose.Position;
            _sprite.transform.localRotation = Quaternion.Euler(0f, 0f, pose.Angle);
            _sprite.transform.localScale = new Vector3(1f, side, 1f);
            ShowSmear(pose.Smear, side, swinging ? aim : Vector2.right);
        }

        private WeaponPose Rest(float side)
        {
            var angle = side > 0f ? 180f - RestAngle : RestAngle;
            return new WeaponPose(new Vector2(_restOffset.x * side, _restOffset.y), angle, 0f);
        }

        private void ShowSmear(float strength, float side, Vector2 aim)
        {
            var visible = strength > 0.01f && _current.Smear != null;
            _smear.enabled = visible;
            if (!visible) return;

            var thin = _current.SwingLook.Style == WeaponSwingStyle.Thrust;
            var scale = _current.SmearScale;
            _smear.transform.localPosition = aim * _smearDistance;
            _smear.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
            _smear.transform.localScale = new Vector3(scale * (thin ? 0.8f : 1f), side * scale * (thin ? 0.35f : 1f), 1f);
            var color = _smear.color;
            color.a = strength;
            _smear.color = color;
        }

        private void Show(WeaponData weapon)
        {
            _current = weapon;
            _sprite.sprite = weapon.Sprite;
            _sprite.enabled = weapon.Sprite != null;
            _smear.sprite = weapon.Smear;
            _smear.enabled = false;
        }

        private SpriteRenderer CreateRenderer(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            if (_reference != null)
            {
                renderer.sortingLayerID = _reference.sortingLayerID;
                renderer.sortingOrder = _reference.sortingOrder + 1;
            }
            renderer.enabled = false;
            return renderer;
        }
    }
}
