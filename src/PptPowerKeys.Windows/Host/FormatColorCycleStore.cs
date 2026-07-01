using System;
using System.Collections.Generic;
using PptPowerKeys.Core.Colors;
using PptPowerKeys.Core.Commands;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// In-memory palette cycle state per color command. Parity with Web Add-in
    /// <c>formatColorState.ts</c> (fingerprint + index).
    /// </summary>
    public static class FormatColorCycleStore
    {
        private sealed class CycleState
        {
            public string Fingerprint = string.Empty;
            public int Index;
        }

        private static readonly Dictionary<CommandIds, CycleState> CycleByCommand = new();

        /// <summary>
        /// Returns the next palette color for a command, advancing the cycle when the
        /// selection fingerprint is unchanged.
        /// </summary>
        public static string NextPaletteColor(
            CommandIds command,
            IReadOnlyList<string> palette,
            IReadOnlyList<string> shapeIds)
        {
            if (palette == null || palette.Count == 0)
            {
                return DefaultColorPalette.FallbackTheme[0];
            }

            string fingerprint = SelectionFingerprint(shapeIds);
            if (!CycleByCommand.TryGetValue(command, out CycleState? state))
            {
                state = new CycleState();
                CycleByCommand[command] = state;
            }

            if (!string.Equals(fingerprint, state.Fingerprint, StringComparison.Ordinal))
            {
                state.Fingerprint = fingerprint;
                state.Index = 0;
            }

            string color = palette[state.Index % palette.Count];
            state.Index = (state.Index + 1) % palette.Count;
            return color;
        }

        /// <summary>Builds a stable fingerprint from selected shape ids in order.</summary>
        public static string SelectionFingerprint(IReadOnlyList<string> shapeIds) =>
            string.Join("\u001f", shapeIds);

        /// <summary>Clears all cycle state (for tests).</summary>
        public static void Clear() => CycleByCommand.Clear();
    }
}
