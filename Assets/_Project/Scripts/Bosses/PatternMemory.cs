namespace GraceRun.Bosses
{
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
}
