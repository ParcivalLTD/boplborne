using System;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>
    /// A smaller boss that fights alongside the main one (a miniboss): which boss, how many, how big,
    /// how tough and how hard it hits. The fight is won when every body, escorts included, is down.
    /// </summary>
    [Serializable]
    public struct EscortSpec
    {
        [Tooltip("The boss to field as an escort; it keeps its own attacks.")]
        public BossData Boss;
        [Min(1)] public int Count;
        [Min(0.05f), Tooltip("Share of that boss's normal HP.")]
        public float HpScale;
        [Range(0.3f, 1f), Tooltip("Body size against the normal boss (colliders and art scale together).")]
        public float SizeScale;
        [Range(0.1f, 1f), Tooltip("Share of its normal attack damage.")]
        public float DamageScale;
        [Tooltip("Where it appears, in units from the main boss's spawn point; x is mirrored for the second, fourth ... escort.")]
        public Vector2 Offset;
    }
}
