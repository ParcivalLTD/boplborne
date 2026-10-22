using System.Collections.Generic;

namespace GraceRun.Bosses
{
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
