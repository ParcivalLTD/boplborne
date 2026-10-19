using GraceRun.Core;
using UnityEngine;

namespace GraceRun.Abilities.Effects
{
    /// <summary>
    /// Puts a placed effect (trap, pad, totem, turret, decoy) on the floor or ledge below where it
    /// spawned, looking down at most the ability's range. Every PC does it the same way at start,
    /// since floors don't move.
    /// </summary>
    [RequireComponent(typeof(AbilityEffect))]
    public class FloorSnap : MonoBehaviour
    {
        private void Start()
        {
            var ability = GetComponent<AbilityEffect>().Ability;
            var floor = Physics2D.Raycast(transform.position, Vector2.down, ability.Range, Layers.StandableMask);
            if (floor.collider != null) transform.position = floor.point;
        }
    }
}
