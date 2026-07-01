using PptPowerKeys.Windows.Host;
using Xunit;

namespace PptPowerKeys.Tests;

public class ColorRgbHelperTests
{
    [Fact]
    public void HexToOleRgb_and_OleRgbToHex_round_trip()
    {
        const string hex = "#4472C4";
        int ole = ColorRgbHelper.HexToOleRgb(hex);
        Assert.Equal(hex, ColorRgbHelper.OleRgbToHex(ole));
    }

    [Theory]
    [InlineData("#000000", true)]
    [InlineData("#2F2F2F", true)]
    [InlineData("#303030", false)]
    [InlineData("#FFFFFF", false)]
    [InlineData("#4472C4", false)]
    public void IsNearBlackHex_matches_web_threshold(string hex, bool expected)
    {
        Assert.Equal(expected, ColorRgbHelper.IsNearBlackHex(hex));
    }

    [Fact]
    public void IsNearBlackOle_matches_hex_helper()
    {
        int black = ColorRgbHelper.HexToOleRgb("#101010");
        Assert.True(ColorRgbHelper.IsNearBlackOle(black));
    }
}
