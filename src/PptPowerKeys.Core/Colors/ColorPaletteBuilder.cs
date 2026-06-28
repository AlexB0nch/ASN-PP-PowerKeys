namespace PptPowerKeys.Core.Colors;

/// <summary>
/// Merges presentation theme colors with recent picks into a single cycling palette.
/// Theme slots come first (up to <see cref="MaxThemeColors"/>), then recent (up to
/// <see cref="MaxRecentColors"/>), with deduplication and hex normalization.
/// </summary>
public static class ColorPaletteBuilder
{
    public const int MaxThemeColors = 10;
    public const int MaxRecentColors = 5;

    /// <summary>
    /// Builds an ordered palette: theme colors first, then recent colors not already present.
    /// When <paramref name="themeColors"/> yields no valid colors, <paramref name="fallbackTheme"/>
    /// is used for the theme section instead.
    /// </summary>
    public static IReadOnlyList<string> Build(
        IReadOnlyList<string>? themeColors,
        IReadOnlyList<string>? recentColors,
        IReadOnlyList<string>? fallbackTheme = null)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var palette = new List<string>();

        var theme = TakeValidColors(themeColors, MaxThemeColors, seen);
        if (theme.Count == 0)
        {
            seen.Clear();
            theme = TakeValidColors(fallbackTheme, MaxThemeColors, seen);
        }

        palette.AddRange(theme);
        AppendRecent(recentColors, palette, seen);

        return palette;
    }

    private static void AppendRecent(
        IReadOnlyList<string>? recentColors,
        List<string> palette,
        HashSet<string> seen)
    {
        if (recentColors is null)
        {
            return;
        }

        var recentAdded = 0;
        foreach (var raw in recentColors)
        {
            if (recentAdded >= MaxRecentColors)
            {
                break;
            }

            if (!TryNormalize(raw, out var hex))
            {
                continue;
            }

            if (!seen.Add(hex))
            {
                continue;
            }

            palette.Add(hex);
            recentAdded++;
        }
    }

    private static List<string> TakeValidColors(
        IReadOnlyList<string>? colors,
        int max,
        HashSet<string> seen)
    {
        var result = new List<string>();
        if (colors is null)
        {
            return result;
        }

        foreach (var raw in colors)
        {
            if (result.Count >= max)
            {
                break;
            }

            if (!TryNormalize(raw, out var hex))
            {
                continue;
            }

            if (!seen.Add(hex))
            {
                continue;
            }

            result.Add(hex);
        }

        return result;
    }

    private static bool TryNormalize(string raw, out string hex)
    {
        hex = string.Empty;
        if (!ThemeColor.IsValidHex(raw))
        {
            return false;
        }

        try
        {
            hex = ThemeColor.NormalizeHex(raw);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
