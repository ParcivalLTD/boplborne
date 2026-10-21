using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Bosses
{
    /// <summary>Random modifier a boss can spawn with.</summary>
    public enum BossVariant : byte
    {
        None,
        Swift,
        Brutal,
        Tough,
        Relentless
    }

    /// <summary>Pure rules behind the boss fight feel (no Unity objects, covered by EditMode tests).</summary>
    public static class BossFeelMath
    {
        /// <summary>Idle pause scale from the player count: 1 at one player, shrinking per extra player, never below half.</summary>
        public static float IdleScale(int players, float shrinkPerExtraPlayer) =>
            Mathf.Clamp(1f - shrinkPerExtraPlayer * Mathf.Max(0, players - 1), 0.5f, 1f);

        /// <summary>Chance to skip the idle pause: 0 up to 2 players, then per extra player.</summary>
        public static float PressureChance(int players, float perPlayer) =>
            Mathf.Clamp01(Mathf.Max(0, players - 2) * perPlayer);

        /// <summary>A pool (poise, interrupt damage) scaled like boss HP.</summary>
        public static float ScaledPool(float basePool, float perExtraPlayer, int players) =>
            basePool * (1f + perExtraPlayer * Mathf.Max(0, players - 1));

        /// <summary>True while a hit counts as hitting the weak point: inside the punish window plus the grace.</summary>
        public static bool InWeakPointWindow(bool recovering, float sinceRecovery, float punishWindow, float grace) =>
            recovering && sinceRecovery >= 0f && sinceRecovery <= punishWindow + grace;

        /// <summary>The variant for a spawn: <paramref name="roll01"/> against the chance, then <paramref name="pick"/> chooses which.</summary>
        public static BossVariant PickVariant(float chance, double roll01, int pick)
        {
            if (roll01 >= chance) return BossVariant.None;
            return (BossVariant)(1 + Mathf.Abs(pick) % 4);
        }

        /// <summary>Damage multiplier for a variant's attacks.</summary>
        public static float DamageScale(BossVariant variant) => variant == BossVariant.Brutal ? 1.25f : 1f;

        /// <summary>Boss HP multiplier for a variant.</summary>
        public static float HpScale(BossVariant variant) => variant == BossVariant.Tough ? 1.25f : 1f;

        /// <summary>Idle pause multiplier for a variant.</summary>
        public static float IdleScale(BossVariant variant) =>
            variant == BossVariant.Swift ? 0.75f : variant == BossVariant.Relentless ? 0.85f : 1f;

        /// <summary>Walk speed multiplier for a variant.</summary>
        public static float SpeedScale(BossVariant variant) => variant == BossVariant.Swift ? 1.2f : 1f;
    }

    /// <summary>Poise: damage fills it up, it drains back after a quiet moment, and a full meter is a stagger.</summary>
    public sealed class PoiseMeter
    {
        private float _sinceHit;

        public float Max { get; private set; }
        public float Value { get; private set; }
        public bool IsBroken => Max > 0f && Value >= Max;
        public float Normalized => Max > 0f ? Mathf.Clamp01(Value / Max) : 0f;

        public void Configure(float max)
        {
            Max = Mathf.Max(0f, max);
            Value = 0f;
            _sinceHit = 0f;
        }

        /// <summary>Adds a hit's damage; true when this hit broke the meter.</summary>
        public bool Add(float damage)
        {
            if (Max <= 0f || damage <= 0f) return false;
            var wasBroken = IsBroken;
            Value = Mathf.Min(Max, Value + damage);
            _sinceHit = 0f;
            return !wasBroken && IsBroken;
        }

        /// <summary>Drains <paramref name="regenPerSecond"/> once <paramref name="delay"/> seconds passed without a hit. A broken meter does not drain.</summary>
        public void Tick(float dt, float regenPerSecond, float delay)
        {
            if (Max <= 0f || IsBroken) return;
            _sinceHit += dt;
            if (_sinceHit >= delay) Value = Mathf.Max(0f, Value - regenPerSecond * dt);
        }

        public void Reset()
        {
            Value = 0f;
            _sinceHit = 0f;
        }
    }

    /// <summary>Remembers the last attacks so the boss does not repeat one over and over.</summary>
    public sealed class PatternMemory
    {
        private int _lastId = -1;
        private int _repeats;

        public void Record(int attackId)
        {
            _repeats = attackId == _lastId ? _repeats + 1 : 1;
            _lastId = attackId;
        }

        public bool IsBlocked(int attackId, int maxRepeats) => attackId == _lastId && _repeats >= maxRepeats;

        public void Clear()
        {
            _lastId = -1;
            _repeats = 0;
        }
    }

    /// <summary>Counts which ability the players keep using and names the one the boss should adapt to.</summary>
    public sealed class AdaptationTracker
    {
        private readonly List<float> _times = new();
        private readonly List<ushort> _sources = new();
        private readonly Dictionary<ushort, int> _counts = new();

        /// <summary>Notes a hit by <paramref name="source"/> at <paramref name="now"/> (seconds).</summary>
        public void Record(ushort source, float now)
        {
            _times.Add(now);
            _sources.Add(source);
        }

        /// <summary>
        /// The source that made at least <paramref name="share"/> of the hits in the last
        /// <paramref name="window"/> seconds (with at least <paramref name="minHits"/> hits), or 0.
        /// </summary>
        public ushort Dominant(float now, float window, int minHits, float share)
        {
            var drop = 0;
            while (drop < _times.Count && now - _times[drop] > window) drop++;
            if (drop > 0)
            {
                _times.RemoveRange(0, drop);
                _sources.RemoveRange(0, drop);
            }
            if (_sources.Count < minHits) return 0;

            _counts.Clear();
            for (var i = 0; i < _sources.Count; i++)
                _counts[_sources[i]] = _counts.TryGetValue(_sources[i], out var n) ? n + 1 : 1;
            foreach (var pair in _counts)
                if (pair.Value >= share * _sources.Count) return pair.Key;
            return 0;
        }

        public void Clear()
        {
            _times.Clear();
            _sources.Clear();
        }
    }
}
