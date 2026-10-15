using System.Collections.Generic;

namespace GraceRun.Net
{
    /// <summary>
    /// Hands out the player slots 0 to capacity-1: a joining client gets the lowest free one,
    /// a leaving client gives it back. Slot 0 is P1, slot 3 is P4.
    /// </summary>
    public class PlayerSlots
    {
        private readonly int _capacity;
        private readonly Dictionary<ulong, int> _byClient = new();

        public PlayerSlots(int capacity)
        {
            _capacity = capacity;
        }

        public int Count => _byClient.Count;

        public bool Has(ulong clientId) => _byClient.ContainsKey(clientId);

        /// <summary>Gives the client the lowest free slot, or false when every slot is taken.</summary>
        public bool TryTake(ulong clientId, out int slot)
        {
            if (_byClient.TryGetValue(clientId, out slot)) return true;

            for (slot = 0; slot < _capacity; slot++)
            {
                if (IsTaken(slot)) continue;
                _byClient.Add(clientId, slot);
                return true;
            }

            slot = -1;
            return false;
        }

        public void Release(ulong clientId) => _byClient.Remove(clientId);

        private bool IsTaken(int slot)
        {
            foreach (var taken in _byClient.Values)
            {
                if (taken == slot) return true;
            }

            return false;
        }
    }
}
