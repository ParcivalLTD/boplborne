using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// BossAttackBehaviour that spawns four floating SplitLantern lights across the arena in Phase 2.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Behaviours/Split Lanterns", fileName = "SplitLanternsBehaviour")]
    public class SplitLanternsBehaviour : BossAttackBehaviour
    {
        [SerializeField] private AttackPhase _triggerPhase = AttackPhase.Active;
        [SerializeField] private GameObject _lanternPrefab;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField, Min(1)] private int _count = 4;
        [SerializeField, Min(5f)] private float _orbitRadius = 16f;

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != _triggerPhase || !runner.IsServer || _lanternPrefab == null) return;
            if (SplitLantern.ActiveCount >= _count) return;

            var center = (Vector2)runner.transform.position;
            var positions = LanternWitchRules.ComputeWispPositions(center, _count, _orbitRadius);

            for (var i = 0; i < positions.Length; i++)
            {
                var instance = Instantiate(_lanternPrefab, positions[i], Quaternion.identity);
                if (instance.TryGetComponent(out SplitLantern lantern))
                {
                    lantern.ServerConfigure(positions[i], _projectilePrefab);
                }

                if (instance.TryGetComponent(out NetworkObject netObj))
                {
                    netObj.Spawn();
                }
            }
        }
    }
}
