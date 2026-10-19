using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>
    /// Pure aiming for the player's own actions: the light attack's hitbox and knockback turned
    /// toward an aim, and the dodge roll's direction.
    /// </summary>
    public static class ActionAim
    {
        /// <summary>
        /// Z rotation (degrees) for a hitbox authored facing right, once its offset is mirrored with
        /// <see cref="HitboxLocalOffset"/>: turns it up or down toward <paramref name="aim"/>.
        /// </summary>
        public static float HitboxAngle(Vector2 aim) => AbilityMath.SideOf(aim.x) * AbilityMath.Elevation(aim);

        /// <summary>The authored (right-facing) hitbox offset mirrored to the aim's side.</summary>
        public static Vector2 HitboxLocalOffset(Vector2 offset, Vector2 aim) =>
            new(offset.x * AbilityMath.SideOf(aim.x), offset.y);

        /// <summary>Authored knockback (x away from the attacker) turned toward <paramref name="aim"/>.</summary>
        public static Vector2 Knockback(Vector2 knockback, Vector2 aim) =>
            AbilityMath.AimedOffset(knockback, AbilityMath.SideOf(aim.x), AbilityMath.Elevation(aim));

        /// <summary>
        /// Unit direction of a dodge roll: the move input in any direction, never into the floor
        /// while grounded; with no input, along <paramref name="facing"/>.
        /// </summary>
        public static Vector2 DodgeDirection(Vector2 move, float facing, bool grounded)
        {
            if (grounded && move.y < 0f) move.y = 0f;
            if (move.sqrMagnitude < 0.0001f) return new Vector2(AbilityMath.SideOf(facing), 0f);
            return move.normalized;
        }
    }
}
