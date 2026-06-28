using PptPowerKeys.Core.Colors;

namespace PptPowerKeys.Api.Contracts;

/// <summary>Request body for <c>POST /api/colors/build-palette</c>.</summary>
public sealed class BuildPaletteApiRequest
{
    public List<string>? ThemeColors { get; set; }

    public List<string>? RecentColors { get; set; }

    public List<string>? FallbackTheme { get; set; }
}

/// <summary>Response body for <c>POST /api/colors/build-palette</c>.</summary>
public sealed class BuildPaletteApiResponse
{
    public List<string> Palette { get; set; } = new();

    public static BuildPaletteApiResponse FromCore(BuildPaletteApiRequest request) => new()
    {
        Palette = ColorPaletteBuilder
            .Build(request.ThemeColors, request.RecentColors, request.FallbackTheme)
            .ToList(),
    };
}
