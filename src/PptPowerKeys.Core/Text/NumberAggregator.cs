using System.Globalization;
using System.Text.RegularExpressions;

namespace PptPowerKeys.Core.Text;

/// <summary>
/// Pure logic behind the "Addup" command: extract numeric values from the text of
/// the selected shapes and compute aggregates. Kept host-independent so it can be
/// unit tested and reused from the backend.
/// </summary>
public static class NumberAggregator
{
    public sealed record Stats(int Count, double Sum, double Min, double Max, double Average);

    private static readonly Regex NumberPattern = new(
        @"-?\d{1,3}(?:[ \u00A0.,]\d{3})*(?:[.,]\d+)?|-?\d+(?:[.,]\d+)?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Parses every number found across the given text fragments and returns the
    /// aggregate. Handles both <c>1,234.56</c> and <c>1 234,56</c> style grouping by
    /// stripping group separators and normalising the decimal mark.
    /// </summary>
    public static Stats Compute(IEnumerable<string?> texts)
    {
        if (texts is null)
        {
            throw new ArgumentNullException(nameof(texts));
        }

        var values = new List<double>();
        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            foreach (Match match in NumberPattern.Matches(text))
            {
                if (TryParse(match.Value, out double value))
                {
                    values.Add(value);
                }
            }
        }

        if (values.Count == 0)
        {
            return new Stats(0, 0, 0, 0, 0);
        }

        return new Stats(
            Count: values.Count,
            Sum: values.Sum(),
            Min: values.Min(),
            Max: values.Max(),
            Average: values.Average());
    }

    /// <summary>Convenience accessor: just the sum of all numbers found.</summary>
    public static double Sum(IEnumerable<string?> texts) => Compute(texts).Sum;

    private static bool TryParse(string raw, out double value)
    {
        value = 0;
        string trimmed = raw.Trim();
        if (trimmed.Length == 0)
        {
            return false;
        }

        // Normalise grouping/decimal separators. Spaces and NBSP are always grouping.
        string cleaned = trimmed.Replace(" ", string.Empty).Replace("\u00A0", string.Empty);

        bool hasComma = cleaned.Contains(',');
        bool hasDot = cleaned.Contains('.');

        if (hasComma && hasDot)
        {
            // The last separator to appear is the decimal mark; the other is grouping.
            char decimalMark = cleaned.LastIndexOf(',') > cleaned.LastIndexOf('.') ? ',' : '.';
            char groupMark = decimalMark == ',' ? '.' : ',';
            cleaned = cleaned.Replace(groupMark.ToString(), string.Empty).Replace(decimalMark, '.');
        }
        else if (hasComma)
        {
            // A lone comma is treated as the decimal mark (e.g. "1,5").
            cleaned = cleaned.Replace(',', '.');
        }

        return double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
