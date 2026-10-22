using System;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// The held key poses of one boss (docs/ART_PIPELINE.md step 4): an idle loop, a stagger,
    /// a death collapse, and per attack a wind-up, strike and recovery pose. The poses follow the
    /// attack phases from <see cref="AttackData"/>, so a pose stays up exactly as long as its phase.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Boss Pose Set", fileName = "BossPoses")]
    public class BossPoseSet : ScriptableObject
    {
        [Serializable]
        public class PoseHurtbox
        {
            public Sprite Sprite;
            public Vector2 Offset;
            public Vector2 Size;
        }

        [Serializable]
        public class AttackPose
        {
            [Tooltip("AttackData ids that use these poses.")]
            public string[] AttackIds;
            public Sprite Windup;
            public Sprite Strike;
            public Sprite Recovery;
        }

        [SerializeField] private Sprite[] _idle;
        [SerializeField, Min(0.1f)] private float _idleFps = 6f;
        [SerializeField] private Sprite _stagger;
        [SerializeField, Min(0f), Tooltip("Seconds the stagger pose shows when the boss changes phase.")]
        private float _staggerTime = 0.8f;
        [SerializeField] private Sprite[] _death;
        [SerializeField, Min(0.1f)] private float _deathFps = 5f;
        [SerializeField] private AttackPose[] _attacks;
        [SerializeField, Tooltip("Body-only hurtbox per pose sprite, for a boss facing right, relative to the boss root. Filled by Grace Run > Bosses > Fit Pose Hurtboxes. A sprite without an entry keeps the prefab's box.")]
        private PoseHurtbox[] _hurtboxes = System.Array.Empty<PoseHurtbox>();

        public Sprite[] Idle => _idle;
        public float IdleFps => _idleFps;
        public Sprite Stagger => _stagger;
        public float StaggerTime => _staggerTime;
        public Sprite[] Death => _death;
        public float DeathFps => _deathFps;

        /// <summary>The body hurtbox for <paramref name="sprite"/> (facing right), or false when the pose has none.</summary>
        public bool TryGetHurtbox(Sprite sprite, out Vector2 offset, out Vector2 size)
        {
            foreach (var entry in _hurtboxes)
            {
                if (entry.Sprite != sprite) continue;
                offset = entry.Offset;
                size = entry.Size;
                return true;
            }
            offset = default;
            size = default;
            return false;
        }

        public AttackPose PoseFor(string attackId)
        {
            foreach (var pose in _attacks)
            {
                if (Array.IndexOf(pose.AttackIds, attackId) >= 0) return pose;
            }
            return null;
        }

        /// <summary>The frame of a sprite list at <paramref name="seconds"/>, looping or holding the last frame.</summary>
        public static Sprite Frame(Sprite[] frames, float fps, float seconds, bool loop)
        {
            if (frames == null || frames.Length == 0) return null;
            var index = Mathf.FloorToInt(Mathf.Max(0f, seconds) * fps);
            return frames[loop ? index % frames.Length : Mathf.Min(index, frames.Length - 1)];
        }
    }
}
