using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// A small world-space stamina bar above the player you own. Placeholder until the HUD
    /// (step 4.3); other players never see it.
    /// </summary>
    public class PlayerStaminaBar : NetworkBehaviour
    {
        [SerializeField] private PlayerStamina _stamina;
        [SerializeField] private GameObject _barRoot;
        [SerializeField] private Transform _fillPivot;

        private void Awake()
        {
            _barRoot.SetActive(false);
        }

        public override void OnNetworkSpawn()
        {
            _barRoot.SetActive(IsOwner);
            enabled = IsOwner;
        }

        public override void OnNetworkDespawn()
        {
            if (_barRoot != null) _barRoot.SetActive(false);
        }

        private void LateUpdate()
        {
            var fill = Mathf.Clamp01(_stamina.Normalized);
            _fillPivot.localScale = new Vector3(fill, 1f, 1f);
        }
    }
}
