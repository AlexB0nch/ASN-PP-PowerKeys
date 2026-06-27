using PptPowerKeys.Core.Text;
using Xunit;

namespace PptPowerKeys.Tests;

public class NumberAggregatorTests
{
    [Fact]
    public void Sum_AddsPlainIntegers()
    {
        Assert.Equal(6, NumberAggregator.Sum(new[] { "1", "2", "3" }), 6);
    }

    [Fact]
    public void Sum_ParsesMultipleNumbersPerField()
    {
        Assert.Equal(10, NumberAggregator.Sum(new[] { "1 and 2", "3 plus 4" }), 6);
    }

    [Fact]
    public void Compute_HandlesUsGrouping()
    {
        var stats = NumberAggregator.Compute(new[] { "1,234.50" });
        Assert.Equal(1, stats.Count);
        Assert.Equal(1234.50, stats.Sum, 6);
    }

    [Fact]
    public void Compute_HandlesEuropeanGrouping()
    {
        var stats = NumberAggregator.Compute(new[] { "1 234,50" });
        Assert.Equal(1234.50, stats.Sum, 6);
    }

    [Fact]
    public void Compute_HandlesNegativesAndDecimals()
    {
        var stats = NumberAggregator.Compute(new[] { "-2.5", "7.5" });
        Assert.Equal(2, stats.Count);
        Assert.Equal(5.0, stats.Sum, 6);
        Assert.Equal(-2.5, stats.Min, 6);
        Assert.Equal(7.5, stats.Max, 6);
        Assert.Equal(2.5, stats.Average, 6);
    }

    [Fact]
    public void Compute_EmptyInput_ReturnsZeroes()
    {
        var stats = NumberAggregator.Compute(new[] { "", "no numbers here", null });
        Assert.Equal(0, stats.Count);
        Assert.Equal(0, stats.Sum, 6);
    }
}
