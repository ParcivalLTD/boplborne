using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.DebugTools
{
    /// <summary>
    /// Draws a hitbox's box as a translucent sprite while it is armed and "Show hitboxes" is on in
    /// the F1 menu. Sits next to the <see cref="Hitbox"/>; the sprite lives on a child object.
    /// </summary>
    [RequireComponent(typeof(Hitbox), typeof(BoxCollider2D))]
    public class HitboxDebugView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _view;

        private Hitbox _hitbox;
        private BoxCollider2D _box;

        private void Awake()
        {
            _hitbox = GetComponent<Hitbox>();
            _box = GetComponent<BoxCollider2D>();
            _view.enabled = false;
        }

        private void LateUpdate()
        {
            var show = _hitbox.IsArmed && DebugMenu.Instance != null && DebugMenu.Instance.ShowHitboxes;
            _view.enabled = show;
            if (!show) return;

            var viewTransform = _view.transform;
            viewTransform.localPosition = _box.offset;
            viewTransform.localScale = new Vector3(_box.size.x, _box.size.y, 1f);
        }
    }
}
