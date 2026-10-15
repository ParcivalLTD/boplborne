using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Shows a downed player's gravestone at the spot it fell, on every PC, with a ring that fills
    /// while a revive is held. Lives on a child of the player but stays in world space, so the
    /// ghost can float away from it. Local visuals only; the state comes from <see cref="ReviveSystem"/>.
    /// </summary>
    public class GravestoneView : MonoBehaviour
    {
        private const int RingSegments = 32;

        [SerializeField] private ReviveSystem _revive;
        [SerializeField, Tooltip("Stone, hat and ring; hidden while the player is alive.")]
        private GameObject _visuals;
        [SerializeField] private LineRenderer _ring;
        [SerializeField, Tooltip("The player's hat, copied onto the stone so everyone sees whose grave it is.")]
        private SpriteRenderer _playerHat;
        [SerializeField] private SpriteRenderer _graveHat;
        [SerializeField, Min(0.05f)] private float _ringRadius = 0.9f;
        [SerializeField, Tooltip("Ring centre, measured from the gravestone's foot.")]
        private Vector2 _ringOffset = new(0f, 0.6f);

        private void Awake()
        {
            _visuals.SetActive(false);
            _ring.useWorldSpace = false;
        }

        private void LateUpdate()
        {
            var show = _revive.IsSpawned && _revive.HasGrave;
            if (_visuals.activeSelf != show)
            {
                _visuals.SetActive(show);
                if (show) CopyHat();
            }
            if (!show) return;

            transform.position = _revive.GravePosition;
            DrawRing(_revive.IsReviving ? _revive.Progress : 0f);
        }

        private void CopyHat()
        {
            var color = _playerHat.color;
            _graveHat.sprite = _playerHat.sprite;
            _graveHat.enabled = _playerHat.enabled && _playerHat.sprite != null;
            _graveHat.color = new Color(color.r, color.g, color.b, 1f);
        }

        private void DrawRing(float progress)
        {
            var count = Mathf.CeilToInt(progress * RingSegments) + 1;
            if (progress <= 0f) count = 0;
            _ring.positionCount = count;
            for (var i = 0; i < count; i++)
            {
                var angle = Mathf.PI * 0.5f - Mathf.Min(i / (float)RingSegments, progress) * Mathf.PI * 2f;
                _ring.SetPosition(i, new Vector3(_ringOffset.x + Mathf.Cos(angle) * _ringRadius,
                    _ringOffset.y + Mathf.Sin(angle) * _ringRadius, 0f));
            }
        }
    }
}
