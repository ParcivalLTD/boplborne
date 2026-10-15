using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Freezes one entity for a few frames when a hit lands: its animators stop and its visuals
    /// can jitter in place. Never touches <c>Time.timeScale</c> (docs/DECISIONS.md #10), so the
    /// rest of the game keeps running. With <c>_freezeBody</c> on, a body this PC simulates also
    /// holds still; turn that on only for owner-simulated players, never for server bodies
    /// (a host must not freeze a boss for everyone). Local cosmetic only.
    /// </summary>
    public class Hitstop : MonoBehaviour
    {
        [SerializeField] private Transform _visuals;
        [SerializeField] private Rigidbody2D _body;
        [SerializeField, Tooltip("Hold the body still while frozen. Players only: their owner simulates them.")]
        private bool _freezeBody;

        private Animator[] _animators;
        private float[] _animatorSpeeds;
        private Vector3 _restPosition;
        private float _timeLeft;
        private float _jitter;
        private bool _bodyFrozen;
        private RigidbodyConstraints2D _savedConstraints;
        private Vector2 _savedVelocity;
        private bool _hasPendingVelocity;
        private Vector2 _pendingVelocity;

        public bool IsFrozen => _timeLeft > 0f;

        private void Awake()
        {
            _animators = GetComponentsInChildren<Animator>(true);
            _animatorSpeeds = new float[_animators.Length];
        }

        private void Update()
        {
            if (!IsFrozen) return;

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                Thaw();
                return;
            }
            if (_jitter > 0f) _visuals.localPosition = _restPosition + (Vector3)(Random.insideUnitCircle * _jitter);
        }

        private void OnDisable()
        {
            if (IsFrozen) Thaw();
        }

        /// <summary>Freezes for <paramref name="duration"/> seconds; a longer freeze extends a running one.</summary>
        public void Freeze(float duration, float jitter)
        {
            if (duration <= 0f || !isActiveAndEnabled) return;
            if (!IsFrozen) Begin();
            _timeLeft = Mathf.Max(_timeLeft, duration);
            _jitter = Mathf.Max(_jitter, jitter);
        }

        /// <summary>
        /// While the body is frozen, keeps <paramref name="velocity"/> to apply when it thaws and
        /// returns true. Returns false when nothing is frozen (apply it now).
        /// </summary>
        public bool DeferVelocity(Vector2 velocity)
        {
            if (!_bodyFrozen) return false;
            _pendingVelocity = velocity;
            _hasPendingVelocity = true;
            return true;
        }

        private void Begin()
        {
            _restPosition = _visuals.localPosition;
            for (var i = 0; i < _animators.Length; i++)
            {
                _animatorSpeeds[i] = _animators[i].speed;
                _animators[i].speed = 0f;
            }

            if (!_freezeBody || _body == null || _body.bodyType != RigidbodyType2D.Dynamic) return;
            _savedConstraints = _body.constraints;
            _savedVelocity = _body.linearVelocity;
            _body.constraints = RigidbodyConstraints2D.FreezeAll;
            _bodyFrozen = true;
        }

        private void Thaw()
        {
            _timeLeft = 0f;
            _jitter = 0f;
            _visuals.localPosition = _restPosition;
            for (var i = 0; i < _animators.Length; i++) _animators[i].speed = _animatorSpeeds[i];

            if (!_bodyFrozen) return;
            _bodyFrozen = false;
            _body.constraints = _savedConstraints;
            _body.linearVelocity = _hasPendingVelocity ? _pendingVelocity : _savedVelocity;
            _hasPendingVelocity = false;
        }
    }
}
