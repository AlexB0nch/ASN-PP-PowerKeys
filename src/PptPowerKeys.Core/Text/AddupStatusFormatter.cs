using System.Globalization;

namespace PptPowerKeys.Core.Text;

/// <summary>
/// Formats addup aggregate statistics for display in the status bar.
/// </summary>
public static class AddupStatusFormatter
{
    public const string ModeAll = "all";
    public const string ModeSum = "sum";
    public const string ModeMin = "min";
    public const string ModeMax = "max";
    public const string ModeAverage = "average";

    public const string UnknownModeWarning = "Unknown addupDisplayMode — using 'all'.";

    private static readonly HashSet<string> ValidModes = new(StringComparer.OrdinalIgnoreCase)
    {
        ModeAll,
        ModeSum,
        ModeMin,
        ModeMax,
        ModeAverage,
    };

    /// <summary>
    /// Normalizes a display-mode string to a known lowercase value, defaulting to <see cref="ModeAll"/>.
    /// </summary>
    public static string NormalizeMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return ModeAll;
        }

        var lower = mode.Trim().ToLowerInvariant();
        return ValidModes.Contains(lower) ? lower : ModeAll;
    }

    /// <summary>
    /// Returns true when <paramref name="mode"/> is a recognized display mode (case-insensitive).
    /// </summary>
    public static bool IsValidMode(string? mode) =>
        !string.IsNullOrWhiteSpace(mode) && ValidModes.Contains(mode.Trim());

    /// <summary>
    /// Builds the status-bar message for the given stats and display mode.
    /// </summary>
    public static string Format(NumberAggregator.Stats stats, string? mode)
    {
        if (stats.Count == 0)
        {
            return "No numbers found in selection.";
        }

        var sum = FormatNumber(stats.Sum);
        var min = FormatNumber(stats.Min);
        var max = FormatNumber(stats.Max);
        var average = FormatNumber(stats.Average);
        var count = stats.Count;

        return NormalizeMode(mode) switch
        {
            ModeSum => $"Sum {sum} ({count} numbers).",
            ModeMin => $"Min {min} ({count} numbers).",
            ModeMax => $"Max {max} ({count} numbers).",
            ModeAverage => $"Avg {average} ({count} numbers).",
            _ => $"Sum {sum} · avg {average} · min {min} · max {max} ({count} numbers).",
        };
    }

    private static string FormatNumber(double value) =>
        value.ToString("G", CultureInfo.InvariantCulture);
}
