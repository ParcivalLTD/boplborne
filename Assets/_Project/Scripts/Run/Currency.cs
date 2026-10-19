using System;

namespace GraceRun.Run
{
    /// <summary>
    /// Shared group currency helper (docs/ARCHITECTURE.md): tracks the team's currency
    /// on the server and synchronizes it to all clients via <see cref="RunManager"/>.
    /// </summary>
    public static class Currency
    {
        /// <summary>The group's shared currency in the active run (0 in the hub).</summary>
        public static int Amount => RunManager.Instance != null ? RunManager.Instance.SharedCurrency : 0;

        /// <summary>Raised on every PC when the shared currency total changes.</summary>
        public static event Action<int> AmountChanged;

        internal static void NotifyAmountChanged(int amount) => AmountChanged?.Invoke(amount);
    }
}
