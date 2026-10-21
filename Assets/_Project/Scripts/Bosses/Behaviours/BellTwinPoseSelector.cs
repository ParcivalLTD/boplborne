using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Both Bell Twins share one prefab. The first twin keeps the pose set wired on
    /// <see cref="BossPoseView"/>; the partner twin switches to its own pose set when it spawns,
    /// so the two bodies look different on every PC. Local cosmetic only.
    /// </summary>
    [RequireComponent(typeof(BellTwinLink), typeof(BossPoseView))]
    public class BellTwinPoseSelector : NetworkBehaviour
    {
        [SerializeField] private BossPoseSet _partnerPoses;
        [SerializeField] private BellTwinLink _link;
        [SerializeField] private BossPoseView _view;
        [SerializeField] private SpriteRenderer _body;

        public override void OnNetworkSpawn()
        {
            if (!_link.IsPartnerBody || _partnerPoses == null) return;
            _view.SetPoses(_partnerPoses);
            if (_partnerPoses.Idle != null && _partnerPoses.Idle.Length > 0) _body.sprite = _partnerPoses.Idle[0];
        }
    }
}
