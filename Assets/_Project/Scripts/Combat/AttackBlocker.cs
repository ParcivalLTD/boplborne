using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// Marks a solid collider (a Stone Wall) that stops attacks: a victim-side hitbox does not hit a
    /// target when one of these stands between the attacker and the target, and projectiles stop on it.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AttackBlocker : MonoBehaviour
    {
    }
}
