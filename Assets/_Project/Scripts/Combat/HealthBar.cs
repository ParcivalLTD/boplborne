using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// A small world-space HP bar that every client shows above a target. Placeholder until the
    /// HUD (step 4.3) and the boss bar.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private Transform _fillPivot;

        private void LateUpdate()
        {
            var fill = _health.IsSpawned ? Mathf.Clamp01(_health.Normalized) : 1f;
            _fillPivot.localScale = new Vector3(fill, 1f, 1f);
        }
    }
}
