namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// In-memory session store for Copy/Paste object position (Left/Top only).
    /// Parity with Web Add-in <c>positionClipboard.ts</c> — not persisted to disk.
    /// </summary>
    public static class PositionClipboardStore
    {
        private static PositionSnapshot? _snapshot;

        public static void Set(double left, double top) =>
            _snapshot = new PositionSnapshot(left, top);

        public static PositionSnapshot? Get() => _snapshot;

        /// <summary>Clears the clipboard (for tests).</summary>
        public static void Clear() => _snapshot = null;

        public readonly record struct PositionSnapshot(double Left, double Top);
    }
}
