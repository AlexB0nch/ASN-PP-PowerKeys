namespace PptPowerKeys.Core.Settings;

/// <summary>
/// Detects duplicate shortcut key bindings (case-insensitive, trimmed keys).
/// </summary>
public static class ShortcutBindingValidator
{
    public static string NormalizeKeys(string keys) => keys.Trim();

    /// <summary>
    /// Returns groups of bindings that share the same normalized keys.
    /// Empty or whitespace-only keys are ignored.
    /// </summary>
    public static IReadOnlyList<DuplicateKeyGroup> FindDuplicateKeys(IEnumerable<ShortcutBinding> bindings)
    {
        if (bindings is null)
        {
            throw new ArgumentNullException(nameof(bindings));
        }

        return bindings
            .Where(b => !string.IsNullOrWhiteSpace(b.Keys))
            .GroupBy(b => NormalizeKeys(b.Keys), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => new DuplicateKeyGroup(
                g.Key,
                g.Select(b => b.CommandId).ToList()))
            .ToList();
    }
}

public sealed record DuplicateKeyGroup(string Keys, IReadOnlyList<string> CommandIds);
