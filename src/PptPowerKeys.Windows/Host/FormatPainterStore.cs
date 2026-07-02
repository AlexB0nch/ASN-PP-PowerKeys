namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// In-memory session flag for format painter PickUp/Apply two-phase flow.
    /// Parity with PowerPoint native format painter — not persisted to disk.
    /// </summary>
    public static class FormatPainterStore
    {
        private static bool _hasPickedUp;

        public static bool HasPickedUp => _hasPickedUp;

        public static void SetPickedUp() => _hasPickedUp = true;

        /// <summary>Clears picked-up state (for tests and after Apply).</summary>
        public static void Clear() => _hasPickedUp = false;
    }
}
