using PptPowerKeys.Core.Colors;
using Xunit;

namespace PptPowerKeys.Tests;

public class ColorPaletteBuilderTests
{
    private static readonly string[] DefaultFallback =
    [
        "#4472C4",
        "#ED7D31",
        "#A5A5A5",
    ];

    [Fact]
    public void Build_MergesThemeThenRecent_PreservesOrder()
    {
        var palette = ColorPaletteBuilder.Build(
            ["#FF0000", "#00FF00"],
            ["#0000FF", "#FFFF00"]);

        Assert.Equal(["#FF0000", "#00FF00", "#0000FF", "#FFFF00"], palette);
    }

    [Fact]
    public void Build_DeduplicatesAcrossThemeAndRecent()
    {
        var palette = ColorPaletteBuilder.Build(
            ["#FF0000", "#00FF00"],
            ["#ff0000", "#0000FF"]);

        Assert.Equal(["#FF0000", "#00FF00", "#0000FF"], palette);
    }

    [Fact]
    public void Build_SkipsInvalidHex()
    {
        var palette = ColorPaletteBuilder.Build(
            ["#AABBCC", "not-a-color", "#112233"],
            ["garbage", "#445566"]);

        Assert.Equal(["#AABBCC", "#112233", "#445566"], palette);
    }

    [Fact]
    public void Build_EmptyTheme_UsesFallback()
    {
        var palette = ColorPaletteBuilder.Build(
            [],
            ["#123456"],
            DefaultFallback);

        Assert.Equal(["#4472C4", "#ED7D31", "#A5A5A5", "#123456"], palette);
    }

    [Fact]
    public void Build_NormalizesHexCaseAndHashPrefix()
    {
        var palette = ColorPaletteBuilder.Build(
            ["aabbcc", "#112233"],
            ["445566"]);

        Assert.Equal(["#AABBCC", "#112233", "#445566"], palette);
    }

    [Fact]
    public void Build_CapsThemeAtTenAndRecentAtFive()
    {
        var theme = Enumerable.Range(1, 12).Select(i => $"#{i:X6}").ToArray();
        var recent = Enumerable.Range(13, 8).Select(i => $"#{i:X6}").ToArray();

        var palette = ColorPaletteBuilder.Build(theme, recent);

        Assert.Equal(15, palette.Count);
        Assert.Equal(theme.Take(10), palette.Take(10));
        Assert.Equal(recent.Take(5), palette.Skip(10));
    }

    [Fact]
    public void Build_RecentOrdering_KeepsInputOrder()
    {
        var palette = ColorPaletteBuilder.Build(
            ["#111111"],
            ["#222222", "#333333", "#444444"]);

        Assert.Equal(["#111111", "#222222", "#333333", "#444444"], palette);
    }
}
