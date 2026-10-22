using GraceRun.Player;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>
    /// The body poses of one ability cast (docs/ART_PIPELINE.md): a wind-up while the effect gathers, a
    /// release pose the moment it leaves, and a recovery pose. Same shape as one weapon move so the
    /// same art pipeline fills it. Referenced by <see cref="AbilityData"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Cast Pose Set", fileName = "CastPoses")]
    public class CastPoseSet : ScriptableObject
    {
        [SerializeField] private WeaponBodyPoses.MovePoses _poses = new();

        public WeaponBodyPoses.MovePoses Poses => _poses;
    }
}
