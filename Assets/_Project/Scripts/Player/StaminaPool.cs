using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Stamina as plain numbers: spending needs the full cost, and refilling only starts after
    /// a delay without spending. Pure logic so it can be tested without a scene.
    /// </summary>
    public class StaminaPool
    {
        private float _delayLeft;

        public StaminaPool(float max)
        {
            Max = max;
            Current = max;
        }

        public float Max { get; }
        public float Current { get; private set; }

        public bool CanSpend(float amount) => Current >= amount;

        /// <summary>Takes the cost if there is enough and restarts the regen delay.</summary>
        public bool TrySpend(float amount, float regenDelay)
        {
            if (!CanSpend(amount)) return false;
            Current -= amount;
            _delayLeft = regenDelay;
            return true;
        }

        /// <summary>Counts down the delay, then refills; time left over from the delay refills too.</summary>
        public void Tick(float deltaTime, float regenPerSecond)
        {
            if (_delayLeft > 0f)
            {
                _delayLeft -= deltaTime;
                if (_delayLeft > 0f) return;
                deltaTime = -_delayLeft;
                _delayLeft = 0f;
            }

            Current = Mathf.Min(Max, Current + regenPerSecond * deltaTime);
        }
    }
}
