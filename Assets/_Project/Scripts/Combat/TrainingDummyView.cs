using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Plays the training dummy's art on every PC: an idle sway, and the hit reaction once per hit
    /// (<see cref="Health.HitTaken"/>). Local cosmetic only.
    /// </summary>
    public class TrainingDummyView : MonoBehaviour
    {
        [SerializeField] private TrainingDummyData _data;
        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private Sprite[] _idle;
        [SerializeField] private Sprite[] _hit;

        private float _hitStart = float.NegativeInfinity;

        private void OnEnable() => _health.HitTaken += OnHitTaken;

        private void OnDisable() => _health.HitTaken -= OnHitTaken;

        private void OnHitTaken(DamageInfo info) => _hitStart = Time.time;

        private void LateUpdate()
        {
            var sinceHit = Time.time - _hitStart;
            // Before the first hit sinceHit is infinite and FloorToInt overflows to a negative index.
            var hitIndex = float.IsInfinity(sinceHit) ? -1 : Mathf.FloorToInt(sinceHit * _data.HitFps);
            if (hitIndex >= 0 && hitIndex < _hit.Length)
            {
                _body.sprite = _hit[hitIndex];
                return;
            }
            var idle = Mathf.FloorToInt(Time.time * _data.IdleFps) % _idle.Length;
            _body.sprite = _idle[idle];
        }
    }
}
