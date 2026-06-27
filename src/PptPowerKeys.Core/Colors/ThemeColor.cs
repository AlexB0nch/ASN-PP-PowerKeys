using System.Globalization;
using System.Text.RegularExpressions;

namespace PptPowerKeys.Core.Colors;

/// <summary>
/// A named color from a presentation theme / slide master palette, stored as an
/// uppercase <c>#RRGGBB</c> hex string. Office.js exposes colors as hex strings,
/// so this avoids any dependency on <c>System.Drawing.Color</c> (which the legacy
/// <c>IColorSchemeReader</c> used).
/// </summary>
public sealed partial record ThemeColor(string Name, string Hex)
{
    [GeneratedRegex("^#?[0-9a-fA-F]{6}$")]
    private static partial Regex HexPattern();

    public static bool IsValidHex(string? hex) => hex is not null && HexPattern().IsMatch(hex);

    /// <summary>Normalises any accepted hex form to uppercase <c>#RRGGBB</c>.</summary>
    public static string NormalizeHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        if (!IsValidHex(hex))
        {
            throw new FormatException($"'{hex}' is not a valid #RRGGBB color.");
        }

        string digits = hex.StartsWith('#') ? hex[1..] : hex;
        return "#" + digits.ToUpperInvariant();
    }

    public static ThemeColor Create(string name, string hex) => new(name, NormalizeHex(hex));

    /// <summary>Relative luminance (0..1) per the sRGB WCAG formula.</summary>
    public double Luminance()
    {
        string digits = Hex.TrimStart('#');
        double r = int.Parse(digits[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
        double g = int.Parse(digits[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
        double b = int.Parse(digits[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
        return (0.2126 * Linearize(r)) + (0.7152 * Linearize(g)) + (0.0722 * Linearize(b));
    }

    /// <summary>Picks black or white for readable text on top of this color.</summary>
    public string ContrastingTextHex() => Luminance() > 0.5 ? "#000000" : "#FFFFFF";

    private static double Linearize(double channel) =>
        channel <= 0.03928 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
}
