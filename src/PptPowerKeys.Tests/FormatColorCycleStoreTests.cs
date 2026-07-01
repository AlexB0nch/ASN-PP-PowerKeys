using PptPowerKeys.Core.Commands;
using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class FormatColorCycleStoreTests
{
    public FormatColorCycleStoreTests()
    {
        FormatColorCycleStore.Clear();
    }

    [Fact]
    public void NextPaletteColor_resets_index_when_selection_changes()
    {
        var palette = new[] { "#FF0000", "#00FF00", "#0000FF" };
        var firstSelection = new[] { "shape-a" };

        Assert.Equal("#FF0000", FormatColorCycleStore.NextPaletteColor(CommandIds.FillColor, palette, firstSelection));
        Assert.Equal("#00FF00", FormatColorCycleStore.NextPaletteColor(CommandIds.FillColor, palette, firstSelection));

        var secondSelection = new[] { "shape-b" };
        Assert.Equal("#FF0000", FormatColorCycleStore.NextPaletteColor(CommandIds.FillColor, palette, secondSelection));
    }

    [Fact]
    public void NextPaletteColor_cycles_independently_per_command()
    {
        var palette = new[] { "#FF0000", "#00FF00" };
        var selection = new[] { "shape-a" };

        Assert.Equal("#FF0000", FormatColorCycleStore.NextPaletteColor(CommandIds.FillColor, palette, selection));
        Assert.Equal("#FF0000", FormatColorCycleStore.NextPaletteColor(CommandIds.LineColor, palette, selection));
        Assert.Equal("#00FF00", FormatColorCycleStore.NextPaletteColor(CommandIds.FillColor, palette, selection));
    }

    [Fact]
    public void NextPaletteColor_uses_fallback_when_palette_empty()
    {
        var color = FormatColorCycleStore.NextPaletteColor(CommandIds.TextColor, [], ["shape-a"]);
        Assert.Equal("#4472C4", color);
    }

    [Fact]
    public void SelectionFingerprint_joins_shape_ids_with_separator()
    {
        Assert.Equal("a\u001fb", FormatColorCycleStore.SelectionFingerprint(["a", "b"]));
    }
}
