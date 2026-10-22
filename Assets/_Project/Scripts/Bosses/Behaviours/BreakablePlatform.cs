using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Added at runtime to an arena ledge when The Storm Wyrm shatters it: the ledge shakes, vanishes (collider
    /// and art) for a while, then comes back. Local to each PC; every PC picks the same ledges from the attack
    /// sequence, so no network state is needed.
    /// </summary>
    public class BreakablePlatform : MonoBehaviour
    {
        private Renderer[] _renderers;
        private Collider2D[] _colliders;
        private Vector3 _home;
        private float _startTime;
        private float _crackSeconds;
        private float _brokenSeconds;
        private float _shake;
        private bool _running;

        /// <summary>Starts a break now: cracking for <paramref name="crackSeconds"/>, then gone for <paramref name="brokenSeconds"/>.</summary>
        public void Begin(float crackSeconds, float brokenSeconds, float shake)
        {
            if (_running) Restore();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _colliders = GetComponentsInChildren<Collider2D>(true);
            _home = transform.localPosition;
            _startTime = Time.time;
            _crackSeconds = crackSeconds;
            _brokenSeconds = brokenSeconds;
            _shake = shake;
            _running = true;
        }

        private void Update()
        {
            if (!_running) return;
            var state = StormWyrmMath.BreakStateAt(Time.time - _startTime, _crackSeconds, _brokenSeconds);
            switch (state)
            {
                case PlatformBreakState.Cracking:
                    transform.localPosition = _home + new Vector3(Mathf.Sin(Time.time * 70f), Mathf.Cos(Time.time * 83f), 0f) * _shake;
                    break;
                case PlatformBreakState.Broken:
                    transform.localPosition = _home;
                    SetSolid(false);
                    break;
                default:
                    Restore();
                    break;
            }
        }

        private void OnDisable()
        {
            if (_running) Restore();
        }

        private void Restore()
        {
            _running = false;
            transform.localPosition = _home;
            SetSolid(true);
        }

        private void SetSolid(bool solid)
        {
            for (var i = 0; i < _colliders.Length; i++) if (_colliders[i] != null) _colliders[i].enabled = solid;
            for (var i = 0; i < _renderers.Length; i++) if (_renderers[i] != null) _renderers[i].enabled = solid;
        }
    }
}
