using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>Poise: damage fills it up, it drains back after a quiet moment, and a full meter is a stagger.</summary>
    public sealed class PoiseMeter
    {
        private float _sinceHit;

        public float Max { get; private set; }
        public float Value { get; private set; }
        public bool IsBroken => Max > 0f && Value >= Max;
        public float Normalized => Max > 0f ? Mathf.Clamp01(Value / Max) : 0f;

        public void Configure(float max)
        {
            Max = Mathf.Max(0f, max);
            Value = 0f;
            _sinceHit = 0f;
        }

        /// <summary>Adds a hit's damage; true when this hit broke the meter.</summary>
        public bool Add(float damage)
        {
            if (Max <= 0f || damage <= 0f) return false;
            var wasBroken = IsBroken;
            Value = Mathf.Min(Max, Value + damage);
            _sinceHit = 0f;
            return !wasBroken && IsBroken;
        }

        /// <summary>Drains <paramref name="regenPerSecond"/> once <paramref name="delay"/> seconds passed without a hit. A broken meter does not drain.</summary>
        public void Tick(float dt, float regenPerSecond, float delay)
        {
            if (Max <= 0f || IsBroken) return;
            _sinceHit += dt;
            if (_sinceHit >= delay) Value = Mathf.Max(0f, Value - regenPerSecond * dt);
        }

        public void Reset()
        {
            Value = 0f;
            _sinceHit = 0f;
        }
    }
}
