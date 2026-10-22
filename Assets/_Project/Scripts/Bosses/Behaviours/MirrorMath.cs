using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>What The Mirror Duelist throws back: the kind of the last ability that hit it.</summary>
    public enum MirrorKind : byte
    {
        /// <summary>A plain weapon hit (or anything unknown): just a quick riposte cut.</summary>
        Slash,
        /// <summary>A projectile ability: a glass shard flies at the target.</summary>
        Shard,
        /// <summary>An area ability: shockwaves run along the floor both ways.</summary>
        Burst,
        /// <summary>A movement ability: the Duelist dashes through the target.</summary>
        Dash,
    }

    /// <summary>Pure math of The Mirror Duelist.</summary>
    public static class MirrorMath
    {
        /// <summary>The echo for a hit by an ability with <paramref name="tags"/>; area beats projectile beats movement.</summary>
        public static MirrorKind KindOf(AbilityTags tags)
        {
            if ((tags & AbilityTags.Area) != 0) return MirrorKind.Burst;
            if ((tags & (AbilityTags.Projectile | AbilityTags.Summon)) != 0) return MirrorKind.Shard;
            return (tags & AbilityTags.Movement) != 0 ? MirrorKind.Dash : MirrorKind.Slash;
        }

        /// <summary>The same spot on the other side of <paramref name="pivot"/>.</summary>
        public static float Reflect(float x, float pivot) => 2f * pivot - x;

        /// <summary>The height a shard flies at: at the target, but never below knee or above head height of the arena.</summary>
        public static float ShardY(float targetY, float floorY, float minAbove, float maxAbove) =>
            Mathf.Clamp(targetY, floorY + minAbove, floorY + maxAbove);

        /// <summary>Where reflection <paramref name="index"/> stands: 0 across the target from the boss, 1 across the arena centre.</summary>
        public static float ReflectionX(int index, float bossX, float targetX, float halfWidth) =>
            BroodMotherMath.ClampArenaX(index == 0 ? Reflect(bossX, targetX) : Reflect(bossX, 0f), halfWidth);

        /// <summary>
        /// Moves the second reflection away from the first when they would stand closer than <paramref name="minGap"/>:
        /// pushed to the side it already leans to, or to the other side when that would leave the arena.
        /// </summary>
        public static float SeparatedX(float first, float second, float minGap, float halfWidth)
        {
            if (Mathf.Abs(second - first) >= minGap) return second;
            var dir = second >= first ? 1f : -1f;
            var pushed = BroodMotherMath.ClampArenaX(first + dir * minGap, halfWidth);
            if (Mathf.Abs(pushed - first) >= minGap - 0.001f) return pushed;
            return BroodMotherMath.ClampArenaX(first - dir * minGap, halfWidth);
        }
    }
}
