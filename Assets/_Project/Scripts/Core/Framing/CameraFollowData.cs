using UnityEngine;

namespace GraceRun.Core.Framing
{
    /// <summary>Tuning of the panning arena camera and the off-screen markers. Local presentation only.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Camera Follow Data", fileName = "CameraFollowData")]
    public class CameraFollowData : ScriptableObject
    {
        [Header("Follow")]
        [Tooltip("Seconds the camera takes to catch up (SmoothDamp time).")]
        [SerializeField, Min(0f)] private float _smoothTime = 0.25f;
        [Tooltip("The local player is always kept at least this many units inside the view edge.")]
        [SerializeField, Min(0f)] private float _localMargin = 7f;
        [Tooltip("While the camera glides, the local player never gets closer than this to the view edge.")]
        [SerializeField, Min(0f)] private float _guardMargin = 2.5f;
        [Tooltip("A boss within this many units of the local player is part of the framing.")]
        [SerializeField, Min(0f)] private float _bossFocusRange = 22f;
        [Tooltip("Farther than this from where it should be, the camera jumps instead of gliding (arena swap, respawn).")]
        [SerializeField, Min(0f)] private float _snapDistance = 14f;

        [Header("Off-screen markers")]
        [Tooltip("A target counts as off screen once it is this many units past the view edge or more (negative: earlier).")]
        [SerializeField] private float _hideMargin = 0.5f;
        [Tooltip("Markers sit this many units inside the view edge.")]
        [SerializeField, Min(0f)] private float _edgeInset = 1.2f;
        [SerializeField, Min(4f)] private float _markerSize = 28f;
        [SerializeField, Min(0f)] private float _pulseSpeed = 4f;
        [SerializeField] private Color _bossColor = new Color(0.86f, 0.25f, 0.22f, 1f);
        [SerializeField] private Color _graveColor = new Color(0.82f, 0.9f, 1f, 1f);

        public float SmoothTime => _smoothTime;
        public float LocalMargin => _localMargin;
        public float GuardMargin => _guardMargin;
        public float BossFocusRange => _bossFocusRange;
        public float SnapDistance => _snapDistance;
        public float HideMargin => _hideMargin;
        public float EdgeInset => _edgeInset;
        public float MarkerSize => _markerSize;
        public float PulseSpeed => _pulseSpeed;
        public Color BossColor => _bossColor;
        public Color GraveColor => _graveColor;
    }
}
