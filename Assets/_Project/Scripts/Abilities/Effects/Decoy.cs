using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// A copy of the caster that bosses go after while it stands (see
    /// <c>BossTargetSelector.Target</c>): buys time to revive a friend. Lives for the ability's lifetime.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class Decoy : MonoBehaviour
    {
        private static readonly List<Decoy> Active = new();

        private AbilityEffect _effect;

        private void Awake()
        {
            _effect = GetComponent<AbilityEffect>();
        }

        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        /// <summary>The spawned decoy closest to <paramref name="from"/>, if any.</summary>
        public static bool TryNearest(Vector2 from, out Transform decoy)
        {
            decoy = null;
            var best = float.MaxValue;
            for (var i = 0; i < Active.Count; i++)
            {
                var candidate = Active[i];
                if (candidate == null || !candidate._effect.IsSpawned) continue;
                var distance = ((Vector2)candidate.transform.position - from).sqrMagnitude;
                if (distance >= best) continue;
                best = distance;
                decoy = candidate.transform;
            }
            return decoy != null;
        }
    }
}
