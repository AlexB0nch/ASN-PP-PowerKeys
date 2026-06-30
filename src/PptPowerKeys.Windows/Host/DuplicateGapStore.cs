using System;
using System.Collections.Generic;
using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// In-memory per-<see cref="CommandIds"/> gap memory for Smart Duplicate.
    /// Parity with Web Add-in <c>duplicateGapMemory.ts</c> — session scope, not persisted.
    /// </summary>
    public static class DuplicateGapStore
    {
        private static readonly Dictionary<CommandIds, double> Gaps = new();

        public static double GetGap(CommandIds command) =>
            Gaps.TryGetValue(command, out var gap) ? gap : 0.0;

        public static void SetGap(CommandIds command, double gap) =>
            Gaps[command] = Math.Max(0.0, gap);

        /// <summary>Clears all remembered gaps (for tests).</summary>
        public static void Clear() => Gaps.Clear();
    }
}
