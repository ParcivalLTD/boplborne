using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>Draws the grapple rope on every PC from the synced <see cref="GrappleHook"/> state.</summary>
    public class GrappleRopeView : MonoBehaviour
    {
        [SerializeField] private GrappleHook _hook;
        [SerializeField] private LineRenderer _line;

        private void LateUpdate()
        {
            var show = _hook.IsSpawned && _hook.Attached;
            _line.enabled = show;
            if (!show) return;
            _line.SetPosition(0, _hook.transform.position);
            _line.SetPosition(1, _hook.Anchor);
        }
    }
}
