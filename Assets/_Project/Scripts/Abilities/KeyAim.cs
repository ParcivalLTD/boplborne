using System.Collections.Generic;
using GraceRun.Combat;
using UnityEngine;

namespace GraceRun.Abilities
{
    /// <summary>The aim of an ability fired with keys or a stick (see <see cref="KeyAim.Resolve"/>).</summary>
    public readonly struct KeyAimResult
    {
        public KeyAimResult(Vector2 aim, bool directed, bool tilted, float distance, bool hasTarget, Vector2 target)
        {
            Tilted = tilted;
            Aim = aim;
            Directed = directed;
            Distance = distance;
            HasTarget = hasTarget;
            Target = target;
        }

        /// <summary>Unit aim direction.</summary>
        public Vector2 Aim { get; }

        /// <summary>True when the effect should fly exactly along <see cref="Aim"/> (up or down held, or snapped to an enemy); false when only the side counts.</summary>
        public bool Directed { get; }

        /// <summary>Distance to the snapped enemy, for aim-point abilities; 0 when not snapped.</summary>
        public float Distance { get; }

        /// <summary>Up or down is held, so the player is deliberately aiming.</summary>
        public bool Tilted { get; }

        public bool HasTarget { get; }

        /// <summary>World position of the snapped enemy.</summary>
        public Vector2 Target { get; }
    }

    /// <summary>
    /// Resolves keyboard / gamepad ability aim on the owner's PC: move input first
    /// (<see cref="AbilityAim.FromMove"/>), then an optional snap onto the enemy closest to the aim line.
    /// </summary>
    public static class KeyAim
    {
        private static readonly List<Vector2> Targets = new();

        /// <summary>Aim for <paramref name="move"/> from <paramref name="origin"/>; <paramref name="data"/> may be null (no assist).</summary>
        public static KeyAimResult Resolve(Vector2 origin, Vector2 move, float facing, AimAssistData data, bool assist)
        {
            var aim = AbilityAim.FromMove(move, facing, out var tilted);
            if (assist && data != null)
            {
                CollectTargets();
                var index = AbilityAim.PickTarget(origin, aim, Targets, data.ConeDegrees, data.MaxRange);
                if (index >= 0)
                {
                    var target = Targets[index];
                    return new KeyAimResult((target - origin).normalized, true, tilted, Vector2.Distance(origin, target), true, target);
                }
            }
            return new KeyAimResult(aim, tilted, tilted, 0f, false, default);
        }

        private static void CollectTargets()
        {
            Targets.Clear();
            foreach (var hurtbox in Hurtbox.All)
            {
                if (hurtbox == null || hurtbox.Health == null || hurtbox.Health.Team == Team.Player ||hurtbox.Health.IsDead) continue;
                if (!hurtbox.TryGetComponent(out Collider2D collider) || !collider.enabled) continue;
                Targets.Add(collider.bounds.center);
            }
        }
    }
}
