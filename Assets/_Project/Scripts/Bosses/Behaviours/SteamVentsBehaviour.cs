using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// BossAttackBehaviour that erupts a series of spaced SteamVent hazards across the arena floor.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Behaviours/Steam Vents", fileName = "SteamVentsBehaviour")]
    public class SteamVentsBehaviour : BossAttackBehaviour
    {
        [SerializeField] private AttackPhase _triggerPhase = AttackPhase.Active;
        [SerializeField] private GameObject _ventPrefab;
        [SerializeField, Min(1)] private int _count = 5;
        [SerializeField, Min(1f)] private float _spacing = 10f;
        [SerializeField] private float _groundY = -5f;
        [SerializeField, Min(0.1f)] private float _windup = 0.85f;
        [SerializeField, Min(0.1f)] private float _activeTime = 0.25f;
        [SerializeField, Min(0)] private int _damage = 22;
        [SerializeField] private Vector2 _knockback = new(0f, 12f);

        public override void OnPhaseChanged(BossAttackRunner runner, AttackData attack, AttackPhase phase)
        {
            if (phase != _triggerPhase || !runner.IsServer || _ventPrefab == null) return;

            var startX = -((_count - 1) * _spacing * 0.5f);
            for (var i = 0; i < _count; i++)
            {
                var x = startX + (i * _spacing);
                var pos = new Vector3(x, _groundY, 0f);
                var instance = Instantiate(_ventPrefab, pos, Quaternion.identity);

                if (instance.TryGetComponent(out SteamVent vent))
                {
                    vent.ServerConfigure(_windup, _activeTime, _damage, _knockback);
                }

                if (instance.TryGetComponent(out Unity.Netcode.NetworkObject netObj))
                {
                    netObj.Spawn();
                }
            }
        }
    }
}
