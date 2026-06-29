using PptPowerKeys.Core.Text;
using Xunit;

namespace PptPowerKeys.Tests;

public class AddupStatusFormatterTests
{
    private static NumberAggregator.Stats SampleStats() =>
        new(Count: 4, Sum: 10, Min: 2, Max: 8, Average: 5);

    [Fact]
    public void Format_AllMode_MatchesRegressionString()
    {
        var stats = SampleStats();
        var message = AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeAll);

        Assert.Equal("Sum 10 · avg 5 · min 2 · max 8 (4 numbers).", message);
    }

    [Fact]
    public void Format_SumMode_ShowsSumOnly()
    {
        var stats = SampleStats();
        var message = AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeSum);

        Assert.Equal("Sum 10 (4 numbers).", message);
    }

    [Fact]
    public void Format_MinMode_ShowsMinOnly()
    {
        var stats = SampleStats();
        var message = AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeMin);

        Assert.Equal("Min 2 (4 numbers).", message);
    }

    [Fact]
    public void Format_MaxMode_ShowsMaxOnly()
    {
        var stats = SampleStats();
        var message = AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeMax);

        Assert.Equal("Max 8 (4 numbers).", message);
    }

    [Fact]
    public void Format_AverageMode_ShowsAverageOnly()
    {
        var stats = SampleStats();
        var message = AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeAverage);

        Assert.Equal("Avg 5 (4 numbers).", message);
    }

    [Fact]
    public void Format_ZeroCount_ReturnsNoNumbersMessage()
    {
        var stats = new NumberAggregator.Stats(0, 0, 0, 0, 0);

        Assert.Equal(
            "No numbers found in selection.",
            AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeAll));
        Assert.Equal(
            "No numbers found in selection.",
            AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeSum));
    }

    [Fact]
    public void Format_DecimalValues_UsesInvariantFormatting()
    {
        var stats = new NumberAggregator.Stats(2, 15.5, 5.5, 10, 7.75);
        var message = AddupStatusFormatter.Format(stats, AddupStatusFormatter.ModeAll);

        Assert.Equal("Sum 15.5 · avg 7.75 · min 5.5 · max 10 (2 numbers).", message);
    }

    [Theory]
    [InlineData("ALL", "all")]
    [InlineData("Sum", "sum")]
    [InlineData(null, "all")]
    [InlineData("", "all")]
    [InlineData("bogus", "all")]
    public void NormalizeMode_ReturnsExpected(string? input, string expected)
    {
        Assert.Equal(expected, AddupStatusFormatter.NormalizeMode(input));
    }
}
