using System;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// The held body poses of one weapon (docs/ART_PIPELINE.md): per <see cref="BodyMove"/> a
    /// wind-up, strike and recovery pose with the weapon drawn in, shown for the matching swing
    /// phase so the art follows the move's timing. Filled by Grace Run → Build Weapon Art.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Weapon Body Poses", fileName = "WeaponBodyPoses")]
    public class WeaponBodyPoses : ScriptableObject
    {
        [Serializable]
        public class MovePoses
        {
            public Sprite Windup;
            public Sprite Strike;
            public Sprite Recovery;

            [Tooltip("Where the hat sits in each pose, relative to the rig (the head moves in a lunge).")]
            public Vector2 HatWindup;
            public Vector2 HatStrike;
            public Vector2 HatRecovery;
        }

        [SerializeField] private MovePoses _lightA = new();
        [SerializeField] private MovePoses _lightB = new();
        [SerializeField] private MovePoses _heavy = new();
        [SerializeField] private MovePoses _aerial = new();
        [SerializeField] private MovePoses _up = new();
        [SerializeField] private MovePoses _down = new();

        public MovePoses Get(BodyMove move) => move switch
        {
            BodyMove.LightA => _lightA,
            BodyMove.LightB => _lightB,
            BodyMove.Heavy => _heavy,
            BodyMove.Aerial => _aerial,
            BodyMove.Up => _up,
            _ => _down,
        };

        /// <summary>The sprite and hat offset for a swing phase; the recovery pose also covers any other phase.</summary>
        public static Sprite SpriteFor(MovePoses poses, SwingPhase phase) => phase switch
        {
            SwingPhase.Startup => poses.Windup,
            SwingPhase.Active => poses.Strike,
            _ => poses.Recovery,
        };

        public static Vector2 HatFor(MovePoses poses, SwingPhase phase) => phase switch
        {
            SwingPhase.Startup => poses.HatWindup,
            SwingPhase.Active => poses.HatStrike,
            _ => poses.HatRecovery,
        };
    }
}
